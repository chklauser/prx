namespace PrexoniteTests.Tests.Configurations;

record TestModel
{
    public required string TestSuiteScript { get; init; }
    public required TestDependency[] UnitsUnderTest { get; init; }
    public required TestDependency[] TestDependencies { get; init; }
}

public record TestDependency
{
    public required string ScriptName { get; init; }
    public required string[] Dependencies { get; init; }
}
