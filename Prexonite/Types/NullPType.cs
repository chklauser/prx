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
#define SINGLE_NULL

#region

using System.Diagnostics;
using Prexonite.Compiler.Cil;

#endregion

namespace Prexonite.Types;

[PTypeLiteral(nameof(Null))]
public class NullPType : PType, ICilCompilerAware
{
    #region Singleton

    NullPType()
    {
    }

    /// <summary>
    ///     The one and only instance of <see cref = "NullPType" />.
    /// </summary>
    public static NullPType Instance { get; } = new();

    #endregion

    #region Static

#if SINGLE_NULL
    static readonly PValue SingleNull = new(null, Instance);
#endif

    /// <summary>
    ///     Returns a PValue(null).
    /// </summary>
    /// <returns>PValue(null)</returns>
    public static PValue CreateValue()
    {
#if SINGLE_NULL
        return SingleNull;
#else
            return new PValue(null, Instance);
#endif
    }

    #endregion

    /// <summary>
    ///     Returns a PValue(null).
    /// </summary>
    /// <returns>PValue(null)</returns>
    [DebuggerStepThrough]
    public PValue CreatePValue()
    {
#if SINGLE_NULL
        return SingleNull;
#else
            return new PValue(null, this);
#endif
    }

    #region Access interface implementation

    public override PValue Construct(StackContext sctx, PValue[] args)
    {
        return Null.CreatePValue();
    }

    public override bool TryConstruct(StackContext sctx, PValue[] args, out PValue result)
    {
        result = Null.CreatePValue();
        return true;
    }

    [SuppressMessage("ReSharper", "ObjectProducedWithMustDisposeAnnotatedMethodIsNotDisposed")]
    public override bool TryDynamicCall(
        StackContext sctx,
        PValue subject,
        PValue[] args,
        PCall call,
        string id,
        [NotNullWhen(true)] out PValue? result
    )
    {
        result = null;
        if (Engine.StringsAreEqual(id, "tostring"))
            result = String.CreatePValue("");
        else if (Engine.StringsAreEqual(id, @"\boxed"))
            result = sctx.CreateNativePValue(CreatePValue());
        else if (Engine.StringsAreEqual(id, "GetEnumerator"))
            result = sctx.CreateNativePValue(Enumerable.Empty<PValue>().GetEnumerator());
        return result != null;
    }

    public override bool TryStaticCall(
        StackContext sctx, PValue[] args, PCall call, string id, [NotNullWhen(true)] out PValue? result)
    {
        result = null;
        return false;
    }

    protected override bool InternalConvertTo(
        StackContext sctx,
        PValue subject,
        PType target,
        bool useExplicit,
        [NotNullWhen(true)] out PValue? result)
    {
        result = target.ToBuiltIn() switch
        {
            BuiltIn.Real => Real.CreatePValue(0.0),
            BuiltIn.Int => Int.CreatePValue(0),
            BuiltIn.String => String.CreatePValue(""),
            BuiltIn.Bool => Bool.CreatePValue(false),
            _ => null,
        };

        return result != null;
    }

    protected override bool InternalConvertFrom(
        StackContext sctx,
        PValue subject,
        bool useExplicit,
        out PValue result)
    {
        result = Null.CreatePValue();
        return true;
    }

    protected override bool InternalIsEqual(PType otherType)
    {
        return otherType is NullPType;
    }

    public override int GetHashCode()
    {
        return 1357155649;
    }

    #region Operators (no action)

    //UNARY
    public override bool Increment(StackContext sctx, PValue operand, out PValue result)
    {
        result = operand;
        return true;
    }

    public override bool Decrement(StackContext sctx, PValue operand, out PValue result)
    {
        result = operand;
        return true;
    }

    public override bool LogicalNot(StackContext sctx, PValue operand, out PValue result)
    {
        result = operand;
        return true;
    }

    public override bool OnesComplement(StackContext sctx, PValue operand, out PValue result)
    {
        result = operand;
        return true;
    }

    public override bool UnaryNegation(StackContext sctx, PValue operand, out PValue result)
    {
        result = operand;
        return true;
    }

    //BINARY

    static bool _coalesce(PValue leftOperand, PValue rightOperand, out PValue? result)
    {
        result = null;
        var leftIsNull = leftOperand.Value == null;
        var rightIsNull = rightOperand.Value == null;

        if (leftIsNull && rightIsNull)
            result = Null.CreatePValue();
        else if (leftIsNull)
            result = rightOperand;
        else if (rightIsNull)
            result = leftOperand;

        return result != null;
    }

    public override bool Addition(
        StackContext sctx, PValue leftOperand, PValue rightOperand, [NotNullWhen(true)] out PValue? result)
    {
        return _coalesce(leftOperand, rightOperand, out result);
    }

    public override bool Subtraction(
        StackContext sctx, PValue leftOperand, PValue rightOperand, [NotNullWhen(true)] out PValue? result)
    {
        return _coalesce(leftOperand, rightOperand, out result);
    }

    public override bool Multiply(
        StackContext sctx, PValue leftOperand, PValue rightOperand, [NotNullWhen(true)] out PValue? result)
    {
        return _coalesce(leftOperand, rightOperand, out result);
    }

    public override bool Division(
        StackContext sctx, PValue leftOperand, PValue rightOperand, [NotNullWhen(true)] out PValue? result)
    {
        return _coalesce(leftOperand, rightOperand, out result);
    }

    public override bool Modulus(
        StackContext sctx, PValue leftOperand, PValue rightOperand, [NotNullWhen(true)] out PValue? result)
    {
        return _coalesce(leftOperand, rightOperand, out result);
    }

    public override bool BitwiseAnd(
        StackContext sctx, PValue leftOperand, PValue rightOperand, [NotNullWhen(true)] out PValue? result)
    {
        return _coalesce(leftOperand, rightOperand, out result);
    }

    public override bool BitwiseOr(
        StackContext sctx, PValue leftOperand, PValue rightOperand, [NotNullWhen(true)] out PValue? result)
    {
        return _coalesce(leftOperand, rightOperand, out result);
    }

    public override bool ExclusiveOr(
        StackContext sctx, PValue leftOperand, PValue rightOperand, [NotNullWhen(true)] out PValue? result)
    {
        return _coalesce(leftOperand, rightOperand, out result);
    }

    public override bool Equality(
        StackContext sctx, PValue leftOperand, PValue rightOperand, [NotNullWhen(true)] out PValue? result)
    {
        var leftIsNull = leftOperand.Value == null;
        var rightIsNull = rightOperand.Value == null;

        if (leftIsNull && rightIsNull)
            result = Bool.CreatePValue(true);
        else if (leftIsNull ^ rightIsNull)
            result = Bool.CreatePValue(false);
        else
            result = null; //unknown

        return result != null;
    }

    public override bool Inequality(
        StackContext sctx, PValue leftOperand, PValue rightOperand, [NotNullWhen(true)] out PValue? result)
    {
        var leftIsNull = leftOperand.Value == null;
        var rightIsNull = rightOperand.Value == null;

        if (leftIsNull && rightIsNull)
            result = Bool.CreatePValue(false);
        else if (leftIsNull ^ rightIsNull)
            result = Bool.CreatePValue(true);
        else
            result = null; //unknown

        return result != null;
    }

    public override bool GreaterThan(
        StackContext sctx, PValue leftOperand, PValue rightOperand, [NotNullWhen(true)] out PValue? result)
    {
        result = null;
        var leftIsNull = leftOperand.Value == null;
        var rightIsNull = rightOperand.Value == null;

        if (leftIsNull && rightIsNull)
            result = Bool.CreatePValue(false);
        else if (leftIsNull)
            result = Bool.CreatePValue(false); //everything else is greater than null
        else if (rightIsNull)
            result = Bool.CreatePValue(true);

        return result != null;
    }

    public override bool GreaterThanOrEqual(
        StackContext sctx,
        PValue leftOperand,
        PValue rightOperand,
        [NotNullWhen(true)] out PValue? result)
    {
        result = null;
        var leftIsNull = leftOperand.Value == null;
        var rightIsNull = rightOperand.Value == null;

        if (leftIsNull && rightIsNull)
            result = Bool.CreatePValue(true);
        else if (leftIsNull)
            result = Bool.CreatePValue(false); //everything else is greater than null
        else if (rightIsNull)
            result = Bool.CreatePValue(true);

        return result != null;
    }

    public override bool LessThan(
        StackContext sctx, PValue leftOperand, PValue rightOperand, [NotNullWhen(true)] out PValue? result)
    {
        result = null;
        var leftIsNull = leftOperand.Value == null;
        var rightIsNull = rightOperand.Value == null;

        if (leftIsNull && rightIsNull)
            result = Bool.CreatePValue(false);
        else if (leftIsNull)
            result = Bool.CreatePValue(true); //everything else is greater than null
        else if (rightIsNull)
            result = Bool.CreatePValue(false);

        return result != null;
    }

    public override bool LessThanOrEqual(
        StackContext sctx,
        PValue leftOperand,
        PValue rightOperand,
        [NotNullWhen(true)] out PValue? result)
    {
        result = null;
        var leftIsNull = leftOperand.Value == null;
        var rightIsNull = rightOperand.Value == null;

        if (leftIsNull && rightIsNull)
            result = Bool.CreatePValue(true);
        else if (leftIsNull)
            result = Bool.CreatePValue(true); //everything else is greater than null
        else if (rightIsNull)
            result = Bool.CreatePValue(false);

        return result != null;
    }

    #endregion

    #endregion

    /// <summary>
    ///     The indirect call implementation of null values: Do nothing.
    /// </summary>
    /// <param name = "sctx">The context in which to do nothing. (ignored).</param>
    /// <param name = "subject">The subject on which to do nothing (ignored).</param>
    /// <param name = "args">The list of arguments (ignored).</param>
    /// <param name = "result">The result of doing nothing. Always PValue(null).</param>
    /// <returns>Always true (doing nothing can't possibly fail...)</returns>
    [DebuggerStepThrough]
    public override bool IndirectCall(
        StackContext sctx, PValue subject, PValue[] args, out PValue result)
    {
        //Does nothing
        result = CreatePValue();
        return true;
    }

    public const string Literal = nameof(Null);

    /// <summary>
    ///     Returns the Null <see cref = "Literal" />.
    /// </summary>
    /// <returns>The Null <see cref = "Literal" />.</returns>
    [DebuggerStepThrough]
    public override string ToString()
    {
        return Literal;
    }

    [DebuggerStepThrough]
    public static implicit operator PValue(NullPType T)
    {
#if SINGLE_NULL
        return SingleNull;
#else
            return new PValue(null, this);
#endif
    }

    #region ICilCompilerAware Members

    /// <summary>
    ///     Asses qualification and preferences for a certain instruction.
    /// </summary>
    /// <param name = "ins">The instruction that is about to be compiled.</param>
    /// <returns>A set of <see cref = "CompilationFlags" />.</returns>
    CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersCustomImplementation;
    }

    /// <summary>
    ///     Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
    /// </summary>
    /// <param name = "state">The compiler state.</param>
    /// <param name = "ins">The instruction to compile.</param>
    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        state.EmitCall(Compiler.Cil.Compiler.GetNullPType);
    }

    #endregion
}