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

        // ReSharper disable once UnusedParameter.Local
        public CompilerPass(Application app, bool makeAvailableForLinking)
        {
            // TODO: respect 'makeAvailableForLinking' again https://github.com/chklauser/prx/issues/115
            MakeAvailableForLinking = true;
            if (MakeAvailableForLinking)
            {
                var sequenceName = _createNextTypeName(app?.Id);
                var asmName = new AssemblyName(sequenceName);
                _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndCollect);
                _moduleBuilder = _assemblyBuilder.DefineDynamicModule(asmName.Name);
                _typeBuilder = _moduleBuilder.DefineType(sequenceName);
            }
        }

        public MethodInfo DefineImplementationMethod(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var parameterTypes = new[]
                {
                    typeof (PFunction),
                    typeof (StackContext),
                    typeof (PValue[]),
                    typeof (PVariable[]),
                    typeof (PValue).MakeByRefType(),
                    typeof (ReturnMode).MakeByRefType(),
                };

            
            // TODO: Once .NET Core 3.x has support for DynamicMethod.DefineParameter, emit dynamic methods again
            var makeAvailableForLinking = MakeAvailableForLinking;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (makeAvailableForLinking)
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
                        (_mkFieldName(id), typeof (PFunction),
                            FieldAttributes.Public | FieldAttributes.Static);
                FunctionFields.Add(id, fb);

                return dm;
            }
            //var cilm =
            //    new DynamicMethod
            //        (
            //        id,
            //        typeof (void),
            //        parameterTypes,
            //        typeof (Runtime));

            //cilm.DefineParameter(1, ParameterAttributes.In, "source");
            //cilm.DefineParameter(2, ParameterAttributes.In, "sctx");
            //cilm.DefineParameter(3, ParameterAttributes.In, "args");
            //cilm.DefineParameter(4, ParameterAttributes.In, "sharedVariables");
            //cilm.DefineParameter(5, ParameterAttributes.Out, "result");

            //Implementations.Add(id, cilm);

            //return cilm;

            throw new NotSupportedException("Cannot emit dynamic methods. Make functions available for linking instead.");
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

        private readonly SymbolTable<MethodInfo> _implementationTable =
            new SymbolTable<MethodInfo>();

        public SymbolTable<MethodInfo> Implementations
        {
            [DebuggerStepThrough]
            get { return _implementationTable; }
        }

        public ILGenerator GetIlGenerator(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            MethodInfo m;
            if (!_implementationTable.TryGetValue(id, out m))
                throw new PrexoniteException("No implementation stub for a function named " + id +
                    " exists.");

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
                "CIL Implementation " + m.Name +
                    " is neither a dynamic method nor a method builder but a " +
                        m.GetType());
        }

        public bool MakeAvailableForLinking { get; }

        public CompilerPass(FunctionLinking linking)
            : this(null, linking)
        {
        }

        public CompilerPass(bool makeAvailableForLinking)
            : this(null, makeAvailableForLinking)
        {
        }

        public CompilerPass(Application app, FunctionLinking linking)
            : this(
                app,
                (linking & FunctionLinking.AvailableForLinking) ==
                    FunctionLinking.AvailableForLinking)
        {
        }

        private readonly Dictionary<MethodInfo, CilFunction> _delegateCache =
            new Dictionary<MethodInfo, CilFunction>();

        private Type _cachedTypeReference;

        public CilFunction GetDelegate(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            MethodInfo m;
            if (!_implementationTable.TryGetValue(id, out m))
                throw new PrexoniteException("No implementation for a function named " + id +
                    " exists.");

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