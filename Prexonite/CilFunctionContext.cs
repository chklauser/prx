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
using System.Diagnostics;
using System.Text;
using Prexonite.Types;
using System.Reflection;

namespace Prexonite
{
    [DebuggerStepThrough]
    public class CilFunctionContext : StackContext
    {

        public  static CilFunctionContext New(StackContext caller, PFunction originalImplementation)
        {
            if (originalImplementation == null)
                throw new ArgumentNullException("originalImplementation");
            return New(caller, originalImplementation.ImportedNamespaces);
        }

        public static CilFunctionContext New(StackContext caller, SymbolCollection importedNamespaces)
        {
            if (caller == null)
                throw new ArgumentNullException("caller"); 

            if(importedNamespaces == null)
                importedNamespaces = new SymbolCollection();

            return new CilFunctionContext(caller.ParentEngine, caller.ParentApplication, importedNamespaces);
        }

        public  static CilFunctionContext New(StackContext caller)
        {
            return New(caller, (SymbolCollection) null);
        }

        private static readonly MethodInfo _NewMethod =
            typeof(CilFunctionContext).GetMethod(
                "New", new Type[] {typeof(StackContext), typeof(PFunction)});
        internal static MethodInfo NewMethod
        {
            get
            {
                return _NewMethod;
            }
        }

        private CilFunctionContext(Engine parentEngine, Application parentApplication, SymbolCollection importedNamespaces)
        {
            if (parentEngine == null)
                throw new ArgumentNullException("parentEngine"); 
            this.parentEngine = parentEngine;
            if (parentApplication == null)
                throw new ArgumentNullException("parentApplication"); 
            this.parentApplication = parentApplication;
            if (importedNamespaces == null)
                throw new ArgumentNullException("importedNamespaces"); 
            this.importedNamespaces = importedNamespaces;
        }

        private readonly Engine parentEngine;
        private readonly Application parentApplication;
        private readonly SymbolCollection importedNamespaces;

        /// <summary>
        /// Represents the engine this context is part of.
        /// </summary>
        public override Engine ParentEngine
        {
            get { return parentEngine; }
        }

        /// <summary>
        /// The parent application.
        /// </summary>
        public override Application ParentApplication
        {
            get { return parentApplication; }
        }

        public override SymbolCollection ImportedNamespaces
        {
            get { return importedNamespaces; }
        }

        /// <summary>
        /// Indicates whether the context still has code/work to do.
        /// </summary>
        /// <returns>True if the context has additional work to perform in the next cycle, False if it has finished it's work and can be removed from the stack</returns>
        protected override bool PerformNextCylce(StackContext lastContext)
        {
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
            get { return PType.Null; }
        }
    }
}
