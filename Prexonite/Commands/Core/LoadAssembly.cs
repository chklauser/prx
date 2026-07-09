

using System.Reflection;
using Prexonite.Compiler;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core;

/// <summary>
///     Implementation of the LoadAssembly command which dynamically loads an assembly from a file.
/// </summary>
public sealed class LoadAssembly : PCommand, ICilCompilerAware
{
    LoadAssembly()
    {
    }

    public static LoadAssembly Instance { get; } = new();

    /// <summary>
    ///     Implementation of the LoadAssembly command which dynamically loads an assembly from a file.
    /// </summary>
    /// <param name = "sctx">The stack context in which to load the assembly</param>
    /// <param name = "args">A list of file paths to assemblies.</param>
    /// <returns>Null</returns>
    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return RunStatically(sctx, args.ToArray());
    }

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        args ??= [];

        var eng = sctx.ParentEngine;
        foreach (var arg in args)
        {
            var path = arg.CallToString(sctx);
            var ldrOptions = new LoaderOptions(sctx.ParentEngine, sctx.ParentApplication)
            {
                ReconstructSymbols = false,
            };
            var ldr = sctx as Loader ?? new Loader(ldrOptions);
            var asmFile = ldr.ApplyLoadPaths(path);
            if (asmFile == null)
                throw new FileNotFoundException("Prexonite can't load assembly located in " +
                    path);

            Assembly assembly;
            if (asmFile is FileSpec { FullName: var asmFilePath })
            {
                // Use file path based loading to give the CLR file system context.
                assembly = Assembly.LoadFrom(asmFilePath);
            }
            else
            {
                using var stream = asmFile.OpenStream();
                using var buf = new MemoryStream();
                stream.CopyTo(buf);
                assembly = Assembly.Load(buf.GetBuffer());
            }

            eng.RegisterAssembly(assembly);
        }

        return PType.Null.CreatePValue();
    }

    #region Implementation of ICilCompilerAware

    /// <summary>
    ///     Asses qualification and preferences for a certain instruction.
    /// </summary>
    /// <param name = "ins">The instruction that is about to be compiled.</param>
    /// <returns>A set of <see cref = "CompilationFlags" />.</returns>
    public CompilationFlags CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    /// <summary>
    ///     Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
    /// </summary>
    /// <param name = "state">The compiler state.</param>
    /// <param name = "ins">The instruction to compile.</param>
    public void ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException();
    }

    #endregion
}