using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Prexonite;
using Prexonite.Commands;
using Prexonite.Types;
using Resources = Prx.Properties.Resources;

namespace Prx;

public class PrexoniteConsole : SuperConsole, ICommand, IObject
{
    public PrexoniteConsole(bool colorfulConsole)
        : base(colorfulConsole) { }

    public PValue? Tab { get; set; }

    public override bool IsPartOfIdentifier(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_' || c == '\\';
    }

    public override IEnumerable<string> OnTab(string attr, string pref, string root)
    {
        return OnTab(attr, pref, root, sctx);
    }

    public virtual IEnumerable<string> OnTab(
        string attr,
        string pref,
        string root,
        StackContext? callingSctx
    )
    {
        if (callingSctx == null)
            throw new ArgumentNullException(
                nameof(callingSctx),
                Resources.PrexoniteConsole_OnTab_RequiresSctx
            );
        if (Tab is { IsNull: false })
        {
            var plst = Tab.IndirectCall(callingSctx, pref, root);
            plst.ConvertTo(callingSctx, PType.Object[typeof(IEnumerable)], true);
            foreach (var o in (IEnumerable)plst.Value!)
            {
                yield return ((PValue)o).CallToString(callingSctx);
            }
        }
    }

    #region ICommand Members

    /// <summary>
    ///     Returns a reference to the prexonite console.
    /// </summary>
    /// <param name = "callingSctx">The stack context in which the command is executed.</param>
    /// <param name = "args">The array of arguments supplied to the command.</param>
    /// <returns>A reference to the prexonite console.</returns>
    public PValue Run(StackContext callingSctx, ReadOnlySpan<PValue> args)
    {
        return callingSctx.CreateNativePValue(this);
    }

    /// <summary>
    ///     Indicates whether the command behaves like a pure function.
    /// </summary>
    public bool IsPure => false;

    #endregion

    #region IObject Members

    StackContext? sctx;

    public bool TryDynamicCall(
        StackContext callingSctx,
        ReadOnlySpan<PValue> args,
        PCall call,
        string id,
        [NotNullWhen(true)] out PValue? result
    )
    {
        result = null;

        switch (id.ToLowerInvariant())
        {
            case "tab":
                if (call == PCall.Get)
                {
                    result = Tab;
                }
                else if (args.Length > 0)
                {
                    Tab = args[0];
                    result = PType.Null.CreatePValue();
                }
                else
                {
                    throw new PrexoniteException(
                        "You cannot perform a set call with no arguments."
                    );
                }
                break;
            case "readline":
                try
                {
                    sctx = callingSctx;
                    result = ReadLine() ?? PType.Null.CreatePValue();
                }
                finally
                {
                    sctx = null;
                }
                break;
            case "readlineinteractive":
                try
                {
                    sctx = callingSctx;
                    result = ReadLineInteractive() ?? PType.Null.CreatePValue();
                }
                finally
                {
                    sctx = null;
                }
                break;
        }

        return result != null;
    }

    #endregion
}
