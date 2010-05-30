/*
 * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
 *  Copyright (C) 2007  Christian "SealedSun" Klauser
 *  E-mail  sealedsun a.t gmail d.ot com
 *  Web     http://www.sealedsun.ch/
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Diagnostics;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler
{
    [DebuggerStepThrough]
    public class LoaderOptions
    {
        #region Construction

        public LoaderOptions(Engine parentEngine, Application targetApplication)
        {
            if (parentEngine == null)
                throw new ArgumentNullException("parentEngine");
            if (targetApplication == null)
                throw new ArgumentNullException("targetApplication");

            _parentEngine = parentEngine;
            _targetApplication = targetApplication;
            StoreSymbols = true;
            RegisterCommands = true;
            ReconstructSymbols = true;
            Compress = false;
            UseIndicesLocally = true;
            StoreSourceInformation = false;
        }

        #endregion

        #region Properties

        private readonly Engine _parentEngine;

        public Engine ParentEngine
        {
            get { return _parentEngine; }
        }

        private readonly Application _targetApplication;

        public Application TargetApplication
        {
            get { return _targetApplication; }
        }

        public bool RegisterCommands { get; set; }

        public bool ReconstructSymbols { get; set; }

        public bool StoreSymbols { get; set; }

        public bool Compress { get; set; }

        public bool UseIndicesLocally { get; set; }

        public bool StoreSourceInformation { get; set; }

        #endregion

        public void InheritFrom(LoaderOptions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");

            RegisterCommands = options.RegisterCommands;
            ReconstructSymbols = options.ReconstructSymbols;
            StoreSymbols = options.StoreSymbols;
            Compress = options.Compress;
            UseIndicesLocally = options.UseIndicesLocally;
            StoreSourceInformation = options.StoreSourceInformation;
        }
    }
}