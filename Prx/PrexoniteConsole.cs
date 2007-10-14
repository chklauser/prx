using System.Collections;
using System.Collections.Generic;
using Prexonite;
using Prexonite.Commands;
using Prexonite.Types;

namespace Prx
{
    public class PrexoniteConsole : SuperConsole,
                                    ICommand,
                                    IObject
    {
        public PrexoniteConsole(bool colorfulConsole)
            : base(colorfulConsole)
        {
        }

        public PValue Tab
        {
            get { return _onTab; }
            set { _onTab = value; }
        }

        private PValue _onTab;

        public override bool IsPartOfIdentifier(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_' || c == '\\';
        }

        public override IEnumerable<string> OnTab(string attr, string pref, string root)
        {
            if (_onTab != null && !_onTab.IsNull)
            {
                PValue plst = _onTab.IndirectCall(_sctx, new PValue[] {pref, root});
                plst.ConvertTo(_sctx, PType.Object[typeof(IEnumerable)], true);
                foreach (object o in (IEnumerable) plst.Value)
                {
                    yield return ((PValue) o).CallToString(_sctx);
                }
            }
        }

        #region ICommand Members

        /// <summary>
        /// Returns a reference to the prexonite console.
        /// </summary>
        /// <param name="sctx">The stack context in which the command is executed.</param>
        /// <param name="args">The array of arguments supplied to the command.</param>
        /// <returns>A reference to the prexonite console.</returns>
        public PValue Run(StackContext sctx, PValue[] args)
        {
            return sctx.CreateNativePValue(this);
        }

        #endregion

        #region IObject Members

        private StackContext _sctx = null;

        public bool TryDynamicCall(
            StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
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
                            "You cannot perform a set call with no arguments.");
                    }
                    break;
                case "readline":
                    try
                    {
                        _sctx = sctx;
                        result = ReadLine();
                    }
                    finally
                    {
                        _sctx = null;
                    }
                    break;
            }

            return result != null;
        }

        #endregion
    }
}