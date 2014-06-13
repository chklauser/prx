// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System;
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
            return OnTab(attr, pref, root, _sctx);
        }

        public virtual IEnumerable<string> OnTab(string attr, string pref, string root,
            StackContext sctx)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx",
                    "OnTab must either be called from via the ReadLine method or with a valid stack context.");
            if (_onTab != null && !_onTab.IsNull)
            {
                var plst = _onTab.IndirectCall(_sctx, new PValue[] {pref, root});
                plst.ConvertTo(_sctx, PType.Object[typeof (IEnumerable)], true);
                foreach (var o in (IEnumerable) plst.Value)
                {
                    yield return ((PValue) o).CallToString(_sctx);
                }
            }
        }

        #region ICommand Members

        /// <summary>
        ///     Returns a reference to the prexonite console.
        /// </summary>
        /// <param name = "sctx">The stack context in which the command is executed.</param>
        /// <param name = "args">The array of arguments supplied to the command.</param>
        /// <returns>A reference to the prexonite console.</returns>
        public PValue Run(StackContext sctx, PValue[] args)
        {
            return sctx.CreateNativePValue(this);
        }

        /// <summary>
        ///     Indicates whether the command behaves like a pure function.
        /// </summary>
        public bool IsPure
        {
            get { return false; }
        }

        #endregion

        #region IObject Members

        private StackContext _sctx;

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
                case "readlineinteractive":
                    try
                    {
                        _sctx = sctx;
                        result = ReadLineInteractive();
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