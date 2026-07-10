using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace PrexoniteTests.Generators;

[Generator]
public sealed class TestConfigurationGenerator : IIncrementalGenerator
{
    const string ConfigurationFileName = "testconfig.json";

    static readonly DiagnosticDescriptor InvalidConfiguration = new(
        "PRXTEST001",
        "Invalid test configuration",
        "{0}",
        "PrexoniteTests",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    static readonly DiagnosticDescriptor MissingConfiguration = new(
        "PRXTEST002",
        "Missing test configuration",
        "Expected one version {0} test configuration named testconfig.json",
        "PrexoniteTests",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var configurations = context
            .AdditionalTextsProvider.Where(static file =>
                string.Equals(
                    Path.GetFileName(file.Path),
                    ConfigurationFileName,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            .Select(static (file, cancellationToken) => Parse(file, cancellationToken))
            .Collect();

        context.RegisterSourceOutput(configurations, Emit);
    }

    static ParseResult Parse(AdditionalText file, CancellationToken cancellationToken)
    {
        try
        {
            var text = file.GetText(cancellationToken)?.ToString();
            if (text is null)
                return ParseResult.Failure(file.Path, "Could not read the test configuration.");

            var configuration = JsonSerializer.Deserialize<TestConfiguration>(text, JsonOptions);
            if (configuration is null)
                return ParseResult.Failure(file.Path, "The test configuration is empty.");

            configuration.Suites ??= [];
            foreach (var suite in configuration.Suites)
            {
                suite.File = NormalizePath(suite.File ?? "");
                suite.Tests ??= [];
                suite.UnitsUnderTest ??= [];
                suite.TestDependencies ??= [];
                foreach (var dependency in suite.UnitsUnderTest.Concat(suite.TestDependencies))
                {
                    dependency.File = NormalizePath(dependency.File ?? "");
                    dependency.Dependencies ??= [];
                    for (var i = 0; i < dependency.Dependencies.Count; i++)
                        dependency.Dependencies[i] = NormalizePath(
                            dependency.Dependencies[i] ?? ""
                        );
                }
            }

            return ParseResult.Success(file.Path, configuration);
        }
        catch (JsonException exception)
        {
            return ParseResult.Failure(
                file.Path,
                $"Invalid JSON at line {exception.LineNumber}, byte {exception.BytePositionInLine}: {exception.Message}"
            );
        }
    }

    static void Emit(SourceProductionContext context, ImmutableArray<ParseResult> results)
    {
        var configurations = new Dictionary<int, TestConfiguration>();
        foreach (var result in results)
        {
            if (result.Error is { } error)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        InvalidConfiguration,
                        Location.None,
                        $"{result.Path}: {error}"
                    )
                );
                continue;
            }

            var configuration = result.Configuration!;
            if (configuration.Version is not (1 or 2))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        InvalidConfiguration,
                        Location.None,
                        $"{result.Path}: unsupported configuration version {configuration.Version}."
                    )
                );
                continue;
            }

            if (!configurations.TryAdd(configuration.Version, configuration))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        InvalidConfiguration,
                        Location.None,
                        $"More than one version {configuration.Version} test configuration was supplied."
                    )
                );
            }
        }

        foreach (var version in new[] { 1, 2 })
        {
            if (!configurations.ContainsKey(version))
                context.ReportDiagnostic(
                    Diagnostic.Create(MissingConfiguration, Location.None, version)
                );
        }

        if (
            !configurations.TryGetValue(1, out var v1) || !configurations.TryGetValue(2, out var v2)
        )
            return;

        var v1IsValid = Validate(context, v1, 1);
        var v2IsValid = Validate(context, v2, 2);
        if (!v1IsValid || !v2IsValid)
            return;

        context.AddSource(
            "PsrUnitTests.g.cs",
            SourceText.From(GenerateScriptTests(v1, v2), Encoding.UTF8)
        );
        context.AddSource(
            "VMTestConfigurations.g.cs",
            SourceText.From(GenerateVmFixtures(v1, v2), Encoding.UTF8)
        );
    }

    static bool Validate(
        SourceProductionContext context,
        TestConfiguration configuration,
        int expectedVersion
    )
    {
        var valid = true;
        if (configuration.Suites.Count == 0)
        {
            Report($"Version {expectedVersion} test configuration contains no suites.");
            valid = false;
        }

        var classNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var suite in configuration.Suites)
        {
            if (string.IsNullOrWhiteSpace(suite.File))
            {
                Report($"Version {expectedVersion} contains a suite without a file name.");
                valid = false;
                continue;
            }

            if (suite.Tests.Count == 0)
            {
                Report($"Suite '{suite.File}' contains no tests.");
                valid = false;
            }

            var className = ToPsrClassName(suite.File) + (expectedVersion == 2 ? "V2" : "");
            if (!classNames.Add(className))
            {
                Report($"Suite '{suite.File}' produces duplicate class name '{className}'.");
                valid = false;
            }

            var methodNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (var test in suite.Tests)
            {
                if (string.IsNullOrWhiteSpace(test))
                {
                    Report($"Suite '{suite.File}' contains an empty test name.");
                    valid = false;
                }
                else if (!methodNames.Add(ValidTestName(test)))
                {
                    Report(
                        $"Suite '{suite.File}' contains test names that map to the same C# identifier '{ValidTestName(test)}'."
                    );
                    valid = false;
                }
            }

            foreach (var dependency in suite.UnitsUnderTest.Concat(suite.TestDependencies))
            {
                if (string.IsNullOrWhiteSpace(dependency.File))
                {
                    Report($"Suite '{suite.File}' contains a dependency without a file name.");
                    valid = false;
                }
            }
        }

        return valid;

        void Report(string message) =>
            context.ReportDiagnostic(
                Diagnostic.Create(InvalidConfiguration, Location.None, message)
            );
    }

    static string GenerateScriptTests(TestConfiguration v1, TestConfiguration v2)
    {
        var source = Header();
        source.AppendLine("namespace PrexoniteTests.Tests.Configurations;");
        source.AppendLine();

        foreach (var suite in v1.Suites)
        {
            var className = ToPsrClassName(suite.File);
            source.AppendLine("[GeneratedCode(\"TestConfigurationGenerator\", \"1.0\")]");
            source
                .Append("internal abstract class ")
                .Append(className)
                .AppendLine(" : ScriptedUnitTestContainer");
            source.AppendLine("{");
            source.AppendLine("    [OneTimeSetUp]");
            source.AppendLine("    public void SetupTestFile()");
            source.AppendLine("    {");
            source.AppendLine("        var model = new TestModel");
            source.AppendLine("        {");
            source
                .Append("            TestSuiteScript = ")
                .Append(Literal(suite.File))
                .AppendLine(",");
            AppendDependencies(
                source,
                "UnitsUnderTest",
                suite.UnitsUnderTest,
                includeTestFramework: false
            );
            AppendDependencies(
                source,
                "TestDependencies",
                suite.TestDependencies,
                includeTestFramework: true
            );
            source.AppendLine("        };");
            source.AppendLine("        Initialize();");
            source.AppendLine("        Runner.Configure(model, this);");
            source.AppendLine("    }");
            source.AppendLine();
            AppendTests(source, suite.Tests, "RunUnitTest");
            source.AppendLine("}");
            source.AppendLine();
        }

        foreach (var suite in v2.Suites)
        {
            var className = ToPsrClassName(suite.File) + "V2";
            source.AppendLine("[GeneratedCode(\"TestConfigurationGenerator\", \"1.0\")]");
            source
                .Append("internal abstract class ")
                .Append(className)
                .AppendLine(" : V2UnitTestContainer");
            source.AppendLine("{");
            source
                .Append("    protected ")
                .Append(className)
                .Append("(bool compileToCil) : base(")
                .Append(Literal(suite.File))
                .AppendLine(", compileToCil)");
            source.AppendLine("    {");
            source.AppendLine("    }");
            source.AppendLine();
            AppendTests(source, suite.Tests, "RunTestCase");
            source.AppendLine("}");
            source.AppendLine();
        }

        return source.ToString();
    }

    static string GenerateVmFixtures(TestConfiguration v1, TestConfiguration v2)
    {
        var source = Header();
        source.AppendLine("namespace PrexoniteTests.Tests.Configurations;");
        source.AppendLine();

        foreach (var suite in v1.Suites)
        {
            var className = ToPsrClassName(suite.File);
            var baseName = ToIdentifier(suite.File);
            AppendV1Fixture(
                source,
                baseName + "_Interpreted",
                className,
                "new UnitTestConfiguration.InMemory()"
            );
            AppendV1Fixture(
                source,
                baseName + "_CilStatic",
                className,
                "new UnitTestConfiguration.InMemory { CompileToCil = true }"
            );
            AppendV1Fixture(
                source,
                baseName + "_CilIsolated",
                className,
                "new UnitTestConfiguration.InMemory { CompileToCil = true, Linking = FunctionLinking.FullyIsolated }"
            );
        }

        foreach (var suite in v2.Suites)
        {
            var className = ToPsrClassName(suite.File) + "V2";
            var baseName = ToIdentifier(suite.File) + "V2";
            AppendV2Fixture(source, baseName + "_InterpretedV2", className, compileToCil: false);
            AppendV2Fixture(source, baseName + "_CilIsolatedV2", className, compileToCil: true);
        }

        foreach (
            var vmClass in new[]
            {
                "PrexoniteTests.Tests.VMTests",
                "PrexoniteTests.Tests.PartialApplication",
                "PrexoniteTests.Tests.Lazy",
                "PrexoniteTests.Tests.Translation",
                "PrexoniteTests.Tests.BuiltInTypeTests",
                "PrexoniteTests.Tests.ShellExtensions",
            }
        )
        {
            var baseName = vmClass[(vmClass.LastIndexOf('.') + 1)..];
            AppendVmFixture(source, baseName + "_Interpreted", vmClass, false, null);
            AppendVmFixture(
                source,
                baseName + "_CilStatic",
                vmClass,
                true,
                "FunctionLinking.FullyStatic"
            );
            AppendVmFixture(
                source,
                baseName + "_CilIsolated",
                vmClass,
                true,
                "FunctionLinking.FullyIsolated"
            );
        }

        return source.ToString();
    }

    static StringBuilder Header() =>
        new(
            """
            // <auto-generated />
            using System.CodeDom.Compiler;
            using NUnit.Framework;
            using Prexonite.Compiler.Cil;

            """
        );

    static void AppendDependencies(
        StringBuilder source,
        string property,
        List<TestDependency> dependencies,
        bool includeTestFramework
    )
    {
        source.Append("            ").Append(property).AppendLine(" =");
        source.AppendLine("            [");
        foreach (var dependency in dependencies)
        {
            source.AppendLine("                new TestDependency");
            source.AppendLine("                {");
            source
                .Append("                    ScriptName = ")
                .Append(Literal(dependency.File))
                .AppendLine(",");
            source.Append("                    Dependencies = [");
            var entries = dependency.Dependencies.Select(Literal).ToList();
            if (includeTestFramework)
                entries.Add("PrexoniteUnitTestFramework");
            source.Append(string.Join(", ", entries)).AppendLine("],");
            source.AppendLine("                },");
        }
        source.AppendLine("            ],");
    }

    static void AppendTests(StringBuilder source, IEnumerable<string> tests, string runner)
    {
        foreach (var test in tests)
        {
            source.AppendLine("    [Test]");
            source.Append("    public void ").Append(ValidTestName(test)).AppendLine("()");
            source.AppendLine("    {");
            source
                .Append("        ")
                .Append(runner)
                .Append('(')
                .Append(Literal(test))
                .AppendLine(");");
            source.AppendLine("    }");
            source.AppendLine();
        }
    }

    static void AppendV1Fixture(StringBuilder source, string name, string baseClass, string runner)
    {
        source.AppendLine("[TestFixture]");
        source.AppendLine("[GeneratedCode(\"TestConfigurationGenerator\", \"1.0\")]");
        source.Append("internal sealed class ").Append(name).Append(" : ").AppendLine(baseClass);
        source.AppendLine("{");
        source
            .Append("    private readonly UnitTestConfiguration _runner = ")
            .Append(runner)
            .AppendLine(";");
        source.AppendLine("    protected override UnitTestConfiguration Runner => _runner;");
        source.AppendLine("}");
        source.AppendLine();
    }

    static void AppendV2Fixture(
        StringBuilder source,
        string name,
        string baseClass,
        bool compileToCil
    )
    {
        source.AppendLine("[TestFixture]");
        source.AppendLine("[GeneratedCode(\"TestConfigurationGenerator\", \"1.0\")]");
        source.Append("internal sealed class ").Append(name).Append(" : ").AppendLine(baseClass);
        source.AppendLine("{");
        source
            .Append("    public ")
            .Append(name)
            .Append("() : base(")
            .Append(compileToCil ? "true" : "false")
            .AppendLine(")");
        source.AppendLine("    {");
        source.AppendLine("    }");
        source.AppendLine("}");
        source.AppendLine();
    }

    static void AppendVmFixture(
        StringBuilder source,
        string name,
        string baseClass,
        bool compileToCil,
        string? linking
    )
    {
        source.AppendLine("[TestFixture]");
        source.AppendLine("[GeneratedCode(\"TestConfigurationGenerator\", \"1.0\")]");
        source.Append("internal sealed class ").Append(name).Append(" : ").AppendLine(baseClass);
        source.AppendLine("{");
        source.Append("    public ").Append(name).AppendLine("()");
        source.AppendLine("    {");
        source
            .Append("        CompileToCil = ")
            .Append(compileToCil ? "true" : "false")
            .AppendLine(";");
        if (linking is not null)
            source.Append("        StaticLinking = ").Append(linking).AppendLine(";");
        source.AppendLine("    }");
        source.AppendLine("}");
        source.AppendLine();
    }

    static string ToPsrClassName(string fileName) => "Unit_" + ToIdentifier(fileName);

    static string NormalizePath(string path) => path.Replace('\\', '/');

    static string ToIdentifier(string fileName)
    {
        var name = Path.GetFileName(fileName.Replace('\\', '/'));
        const string suffix = ".test.pxs";
        if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            name = name[..^suffix.Length];
        return name.Replace('-', '_');
    }

    static string ValidTestName(string richName)
    {
        var characters = richName.ToCharArray();
        for (var i = 0; i < characters.Length; i++)
        {
            var character = characters[i];
            if (
                !(character is >= 'a' and <= 'z')
                && !(character is >= 'A' and <= 'Z')
                && !(character is >= '0' and <= '9')
                && character != '_'
            )
                characters[i] = '_';
        }
        return new string(characters);
    }

    static string Literal(string value) => JsonSerializer.Serialize(value);

    sealed record ParseResult(string Path, TestConfiguration? Configuration, string? Error)
    {
        public static ParseResult Success(string path, TestConfiguration configuration) =>
            new(path, configuration, null);

        public static ParseResult Failure(string path, string error) => new(path, null, error);
    }

    sealed class TestConfiguration
    {
        public int Version { get; set; }
        public List<TestSuite> Suites { get; set; } = [];
    }

    sealed class TestSuite
    {
        public string File { get; set; } = "";
        public List<TestDependency> UnitsUnderTest { get; set; } = [];
        public List<TestDependency> TestDependencies { get; set; } = [];
        public List<string> Tests { get; set; } = [];
    }

    sealed class TestDependency
    {
        public string File { get; set; } = "";
        public List<string> Dependencies { get; set; } = [];
    }
}
