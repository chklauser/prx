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
using System.Text;
using Prexonite.Modular;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast;

public class AstConstant : AstExpr
{
    public readonly object Constant;

    internal AstConstant(Parser p, object constant)
        : this(p.scanner.File, p.t.line, p.t.col, constant)
    {
    }

    public AstConstant(string file, int line, int column, object constant)
        : base(file, line, column)
    {
        Constant = constant;
    }

    public static bool TryCreateConstant(
        CompilerTarget target,
        ISourcePosition position,
        PValue value,
        out AstExpr expr)
    {
        expr = null;
        if (value.Type is ObjectPType)
            target.Loader.Options.ParentEngine.CreateNativePValue(value.Value);
        if (value.Type is IntPType or RealPType or BoolPType or StringPType or NullPType || _isModuleName(value))
            expr = new AstConstant(position.File, position.Line, position.Column, value.Value);
        else //Cannot represent value in a constant instruction
            return false;
        return true;
    }

    private static bool _isModuleName(PValue value)
    {
        ObjectPType objectType;
        return (object)(objectType = value.Type as ObjectPType) != null && typeof(ModuleName).IsAssignableFrom(objectType.ClrType);
    }

    public PValue ToPValue(CompilerTarget target)
    {
        return target.Loader.Options.ParentEngine.CreateNativePValue(Constant);
    }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        if(stackSemantics == StackSemantics.Effect)
            return;

        if (Constant == null)
            target.EmitNull(Position);
        else
            switch (Type.GetTypeCode(Constant.GetType()))
            {
                case TypeCode.Boolean:
                    target.EmitConstant(Position, (bool) Constant);
                    break;
                case TypeCode.Int16:
                case TypeCode.Byte:
                case TypeCode.Int32:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                    target.EmitConstant(Position, (int) Constant);
                    break;
                case TypeCode.Single:
                case TypeCode.Double:
                    target.EmitConstant(Position, (double) Constant);
                    break;
                case TypeCode.String:
                    target.EmitConstant(Position, (string) Constant);
                    break;
                default:
                    if (Constant is ModuleName moduleName)
                    {
                        target.EmitConstant(Position, moduleName);
                    }
                    else
                    {
                        throw new PrexoniteException(
                            "Prexonite does not support constants of type " +
                            Constant.GetType().Name + ".");
                    }
                    break;
            }
    }

    #region AstExpr Members

    public override bool TryOptimize(CompilerTarget target, out AstExpr expr)
    {
        expr = null;
        return false;
    }

    #endregion

    public override string ToString()
    {
        string str;
        if (Constant != null)
            if ((str = Constant as string) != null)
                return string.Concat("\"", StringPType.Escape(str), "\"");
            else
                return Constant.ToString();
        else return "-null-";
    }
}