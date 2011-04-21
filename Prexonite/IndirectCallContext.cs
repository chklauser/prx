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
using System.Collections.Generic;
using System.Text;
using Prexonite.Types;

namespace Prexonite
{
    public class IndirectCallContext : StackContext
    {
        private readonly StackContext _originalStackContext;

        public IndirectCallContext(StackContext parent, IIndirectCall callable, PValue[] args)
            : this(parent, parent.ParentEngine, parent.ParentApplication, parent.ImportedNamespaces, callable, args)
        {
        }

        public IndirectCallContext(Engine parentEngine,
            Application parentApplication,
            IEnumerable<string> importedNamespaces,
            IIndirectCall callable,
            PValue[] args)
            : this(null, parentEngine, parentApplication, importedNamespaces, callable, args)
        {
        }

        public IndirectCallContext(StackContext originalSctx,
            Engine parentEngine,
            Application parentApplication,
            IEnumerable<string> importedNamespaces,
            IIndirectCall callable,
            PValue[] args)
        {
            if (parentEngine == null)
                throw new ArgumentNullException("parentEngine");
            if (parentApplication == null)
                throw new ArgumentNullException("parentApplication");
            if (importedNamespaces == null)
                throw new ArgumentNullException("importedNamespaces");
            if (callable == null)
                throw new ArgumentNullException("callable");
            if (args == null)
                throw new ArgumentNullException("args");

            _engine = parentEngine;
            _application = parentApplication;
            _importedNamespaces = (importedNamespaces as SymbolCollection) ?? new SymbolCollection(importedNamespaces);
            _callable = callable;
            _arguments = args;
            _originalStackContext = originalSctx;
        }

        public IIndirectCall Callable
        {
            get { return _callable; }
        }

        private readonly IIndirectCall _callable;

        public PValue[] Arguments
        {
            get { return _arguments; }
        }

        private readonly PValue[] _arguments;

        private readonly Engine _engine;
        private readonly Application _application;
        private readonly SymbolCollection _importedNamespaces;
        private PValue _returnValue = PType.Null;

        /// <summary>
        /// Represents the engine this context is part of.
        /// </summary>
        public override Engine ParentEngine
        {
            get { return _engine; }
        }

        /// <summary>
        /// The parent application.
        /// </summary>
        public override Application ParentApplication
        {
            get { return _application; }
        }

        public override SymbolCollection ImportedNamespaces
        {
            get { return _importedNamespaces; }
        }

        /// <summary>
        /// Indicates whether the context still has code/work to do.
        /// </summary>
        /// <returns>True if the context has additional work to perform in the next cycle, False if it has finished it's work and can be removed from the stack</returns>
        protected override bool PerformNextCycle(StackContext lastContext)
        {
            //Remove this context if possible (IndirectCallContext should be transparent)
            var sctx = _originalStackContext ?? this;

            _returnValue = _callable.IndirectCall(sctx, _arguments);
            return false;
        }

        /// <summary>
        /// Tries to handle the supplied exception.
        /// </summary>
        /// <param name="exc">The exception to be handled.</param>
        /// <returns>True if the exception has been handled, false otherwise.</returns>
        public override bool TryHandleException(Exception exc)
        {
            return false;
        }

        /// <summary>
        /// Represents the return value of the context.
        /// Just providing a value here does not mean that it gets consumed by the caller.
        /// If the context does not provide a return value, this property should return null (not NullPType).
        /// </summary>
        public override PValue ReturnValue
        {
            get { return _returnValue ?? PType.Null; }
        }
    }
}