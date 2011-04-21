// /*
//  * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
//  *  Copyright (C) 2007  Christian "SealedSun" Klauser
//  *  E-mail  sealedsun a.t gmail d.ot com
//  *  Web     http://www.sealedsun.ch/
//  *
//  *  This program is free software; you can redistribute it and/or modify
//  *  it under the terms of the GNU General Public License as published by
//  *  the Free Software Foundation; either version 2 of the License, or
//  *  (at your option) any later version.
//  *
//  *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
//  * 
//  *  This program is distributed in the hope that it will be useful,
//  *  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  *  GNU General Public License for more details.
//  *
//  *  You should have received a copy of the GNU General Public License along
//  *  with this program; if not, write to the Free Software Foundation, Inc.,
//  *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
//  */

#region Namespace Imports

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

#endregion

namespace Prexonite.Compiler.Cil
{
    public class CompilerPass
    {
        private static int _numberOfPasses;

        [DebuggerStepThrough]
        private static string _createNextTypeName(string applicationId)
        {
            if (String.IsNullOrEmpty(applicationId))
                applicationId = "cilimpl";

            return applicationId + "_" + _numberOfPasses++ + "";
        }

        private readonly bool _makeAvailableForLinking;

        private readonly AssemblyBuilder _assemblyBuilder;

        public AssemblyBuilder Assembly
        {
            [DebuggerStepThrough]
            get
            {
                if (!MakeAvailableForLinking)
                    throw new NotSupportedException
                        ("The compiler pass is not configured to make implementations available for static linking.");
                return _assemblyBuilder;
            }
        }

        private readonly ModuleBuilder _moduleBuilder;

        public ModuleBuilder Module
        {
            [DebuggerStepThrough]
            get
            {
                if (!MakeAvailableForLinking)
                    throw new NotSupportedException
                        ("The compiler pass is not configured to make implementations available for static linking.");
                return _moduleBuilder;
            }
        }

        private readonly TypeBuilder _typeBuilder;

        public TypeBuilder TargetType
        {
            [DebuggerStepThrough]
            get
            {
                if (!MakeAvailableForLinking)
                    throw new NotSupportedException
                        ("The compiler pass is not configured to make implementations available for static linking.");
                return _typeBuilder;
            }
        }

        public CompilerPass(Application app, bool makeAvailableForLinking)
        {
            _makeAvailableForLinking = makeAvailableForLinking;
            if (MakeAvailableForLinking)
            {
                var sequenceName = _createNextTypeName(app != null ? app.Id : null);
                var asmName = new AssemblyName(sequenceName);
                _assemblyBuilder =
                    AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
                _moduleBuilder = _assemblyBuilder.DefineDynamicModule(asmName.Name, asmName.Name + ".dll");
                _typeBuilder = _moduleBuilder.DefineType(sequenceName);
            }
        }

        public MethodInfo DefineImplementationMethod(string id)
        {
            if (id == null)
                throw new ArgumentNullException("id");

            var parameterTypes = new[]
                {
                    typeof (PFunction),
                    typeof (StackContext),
                    typeof (PValue[]),
                    typeof (PVariable[]),
                    typeof (PValue).MakeByRefType(),
                    typeof (ReturnMode).MakeByRefType(),
                };
            if (MakeAvailableForLinking)
            {
                //Create method stub
                
                var dm = TargetType.DefineMethod
                    (
                    id,
                    MethodAttributes.Static | MethodAttributes.Public,
                    typeof (void),
                    parameterTypes);
                dm.DefineParameter(1, ParameterAttributes.In, "source");
                dm.DefineParameter(2, ParameterAttributes.In, "sctx");
                dm.DefineParameter(3, ParameterAttributes.In, "args");
                dm.DefineParameter(4, ParameterAttributes.In, "sharedVariables");
                dm.DefineParameter(5, ParameterAttributes.Out, "result");
                dm.DefineParameter(6, ParameterAttributes.Out, "returnMode");

                Implementations.Add(id, dm);

                //Create function field
                var fb =
                    TargetType.DefineField
                        (_mkFieldName(id), typeof (PFunction), FieldAttributes.Public | FieldAttributes.Static);
                FunctionFields.Add(id, fb);

                return dm;
            }
            var cilm =
                new DynamicMethod
                    (
                    id,
                    typeof (void),
                    parameterTypes,
                    typeof (Runtime));

            cilm.DefineParameter(1, ParameterAttributes.In, "source");
            cilm.DefineParameter(2, ParameterAttributes.In, "sctx");
            cilm.DefineParameter(3, ParameterAttributes.In, "args");
            cilm.DefineParameter(4, ParameterAttributes.In, "sharedVariables");
            cilm.DefineParameter(5, ParameterAttributes.Out, "result");

            Implementations.Add(id, cilm);

            return cilm;
        }

        private static string _mkFieldName(string id)
        {
            return id + "<field>";
        }

        private readonly SymbolTable<FieldInfo> _functionFieldTable = new SymbolTable<FieldInfo>();

        public SymbolTable<FieldInfo> FunctionFields
        {
            get
            {
                if (!MakeAvailableForLinking)
                    throw new NotSupportedException
                        ("The compiler pass is not configured to make implementations available for static linking.");
                return _functionFieldTable;
            }
        }

        private readonly SymbolTable<MethodInfo> _implementationTable = new SymbolTable<MethodInfo>();

        public SymbolTable<MethodInfo> Implementations
        {
            [DebuggerStepThrough]
            get { return _implementationTable; }
        }

        public ILGenerator GetIlGenerator(string id)
        {
            if (id == null)
                throw new ArgumentNullException("id");
            MethodInfo m;
            if (!_implementationTable.TryGetValue(id, out m))
                throw new PrexoniteException("No implementation stub for a function named " + id + " exists.");

            return GetIlGenerator(m);
        }

        public static ILGenerator GetIlGenerator(MethodInfo m)
        {
            DynamicMethod dm;
            MethodBuilder mb;
            if ((dm = m as DynamicMethod) != null)
                return dm.GetILGenerator();
            if ((mb = m as MethodBuilder) != null)
                return mb.GetILGenerator();
            throw new PrexoniteException
                (
                "CIL Implementation " + m.Name + " is neither a dynamic method nor a method builder but a " +
                m.GetType());
        }

        public bool MakeAvailableForLinking
        {
            get { return _makeAvailableForLinking; }
        }

        public CompilerPass(FunctionLinking linking)
            : this(null, linking)
        {
        }

        public CompilerPass(bool makeAvailableForLinking)
            : this(null, makeAvailableForLinking)
        {
        }

        public CompilerPass(Application app, FunctionLinking linking)
            : this(app, (linking & FunctionLinking.AvailableForLinking) == FunctionLinking.AvailableForLinking)
        {
        }

        private readonly Dictionary<MethodInfo, CilFunction> _delegateCache = new Dictionary<MethodInfo, CilFunction>();
        private Type _cachedTypeReference;

        public CilFunction GetDelegate(string id)
        {
            if (id == null)
                throw new ArgumentNullException("id");
            MethodInfo m;
            if (!_implementationTable.TryGetValue(id, out m))
                throw new PrexoniteException("No implementation for a function named " + id + " exists.");

            return GetDelegate(m);
        }

        public CilFunction GetDelegate(MethodInfo m)
        {
            if (_delegateCache.ContainsKey(m))
                return _delegateCache[m];

            DynamicMethod dm;
            if ((dm = m as DynamicMethod) != null)
                return _delegateCache[m] = (CilFunction) dm.CreateDelegate(typeof (CilFunction));
            return
                _delegateCache[m] =
                (CilFunction)
                Delegate.CreateDelegate
                    (
                    typeof (CilFunction),
                    (_getRuntimeType()).GetMethod(m.Name),
                    true);
        }

        private Type _getRuntimeType()
        {
            return _cachedTypeReference ?? (_cachedTypeReference = TargetType.CreateType());
        }

        public void LinkMetadata(PFunction func)
        {
            if (!MakeAvailableForLinking)
                return;

            var T = _getRuntimeType();

            T.GetField(_mkFieldName(func.Id)).SetValue(null, func);
        }
    }
}