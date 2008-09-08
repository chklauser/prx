#region

using System;
using System.Reflection;
using Prexonite.Compiler.Cil;

#endregion

namespace Prexonite.Types
{
    /// <summary>
    /// Bult-in type for programmatically constructed user defined structures.
    /// </summary>
    [PTypeLiteral(Literal)]
    public class StructurePType : PType, ICilCompilerAware
    {
        /// <summary>
        /// The official name for this type.
        /// </summary>
        public const string Literal = "Structure";

        /// <summary>
        /// Reserved for the member that reacts on <see cref="IndirectCall"/>.
        /// </summary>
        public const string IndirectCallId = "IndirectCall";

        /// <summary>
        /// Reserved for the member that reacts on failed calls to <see cref="TryDynamicCall"/>.
        /// </summary>
        public const string CallId = "Call";

        /// <summary>
        /// Reserved for the member that force-assigns a value to a member.
        /// </summary>
        public const string SetId = @"\set";

        /// <summary>
        /// Alternative id for <see cref="SetId"/>.
        /// </summary>
        public const string SetIdAlternative = @"\";

        /// <summary>
        /// Reserved for the memver that force-assigns a reference value (e.g., method) to a member.
        /// </summary>
        public const string SetRefId = @"\\";

        /// <summary>
        /// Reserved for later use.
        /// </summary>
        public const string ConstructorId = "New";

        #region Creation

        internal class Member : IIndirectCall
        {
            public bool IsReference;
            public PValue Value;

            public Member(bool isReference)
            {
                IsReference = isReference;
                Value = Null.CreatePValue();
            }

            public Member()
                : this(false)
            {
            }

            public PValue Invoke(StackContext sctx, PValue[] args, PCall call)
            {
                if (IsReference)
                    return Value.IndirectCall(sctx, args);
                if (call == PCall.Set && args.Length != 0)
                    Value = args[1];
                return Value;
            }

            #region IIndirectCall Members

            public PValue IndirectCall(StackContext sctx, PValue[] args)
            {
                if (IsReference)
                    return Value.IndirectCall(sctx, args);
                if (args.Length > 1)
                    Value = args[1];
                return Value;
            }

            #endregion
        }

        private StructurePType()
        {
        }

        private static readonly StructurePType _instance = new StructurePType();

        public static StructurePType Instance
        {
            get { return _instance; }
        }

        #endregion

        #region PType implementation

        internal static PValue[] _addThis(PValue Subject, PValue[] args)
        {
            var argst = new PValue[args.Length + 1];
            argst[0] = Subject;
            Array.Copy(args, 0, argst, 1, args.Length);
            return argst;
        }

        internal static PValue[] _addId(string id, PValue[] args)
        {
            var argst = new PValue[args.Length + 1];
            argst[0] = args[0]; //subject
            argst[1] = id;
            Array.Copy(args, 1, argst, 2, args.Length - 1);
            return argst;
        }

        public override bool TryDynamicCall(
            StackContext sctx,
            PValue subject,
            PValue[] args,
            PCall call,
            string id,
            out PValue result)
        {
            result = null;
            var obj = subject.Value as SymbolTable<Member>;
            if (obj == null)
                return false;

            PValue[] argst = _addThis(subject, args);

            Member m;
            bool reference = false;

            //Try to call the member
            if (obj.TryGetValue(id, out m) && m != null)
                result = m.Invoke(sctx, argst, call);
            else
                switch (id.ToLowerInvariant())
                {
                    case SetRefId:
                        reference = true;
                        goto case SetId;
                    case SetId:
                    case SetIdAlternative:
                        if (args.Length < 2)
                            goto default;

                        var mid = (string) args[0].ConvertTo(sctx, String).Value;

                        if (reference || args.Length > 2)
                            reference = (bool) args[1].ConvertTo(sctx, Bool).Value;

                        if (obj.ContainsKey(mid))
                            m = obj[mid];
                        else
                        {
                            m = new Member();
                            obj.Add(mid, m);
                        }

                        m.Value = args[args.Length - 1];
                        m.IsReference = reference;

                        result = m.Value;

                        break;
                    default:
                        //Try to call the generic "call" member
                        if (obj.TryGetValue(CallId, out m) && m != null)
                            result = m.Invoke(sctx, _addId(id, argst), call);
                        else
                            return false;
                        break;
                }

            return result != null;
        }

        public override bool TryStaticCall(
            StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            result = null;
            return false;
        }

        /// <summary>
        /// Tries to construct a new Structure instance.
        /// </summary>
        /// <param name="sctx">The stack context in which to construct the Structure.</param>
        /// <param name="args">An array of arguments. Ignored in the current implementation.</param>
        /// <param name="result">The out parameter that holds the resulting PValue.</param>
        /// <returns>True if the construction was successful; false otherwise.</returns>
        public override bool TryContruct(StackContext sctx, PValue[] args, out PValue result)
        {
            result = new PValue(new SymbolTable<Member>(), this);
            return true;
        }

        protected override bool InternalConvertTo(
            StackContext sctx,
            PValue subject,
            PType target,
            bool useExplicit,
            out PValue result)
        {
            result = null;
            var obj = subject.Value as SymbolTable<Member>;
            if (obj == null)
                return false;

            switch (target.ToBuiltIn())
            {
                case BuiltIn.String:
                    normalString:
                    if (!TryDynamicCall(sctx, subject, new PValue[] {}, PCall.Get, "ToString", out result))
                        result = null;
                    break;
                case BuiltIn.Object:
                    Type clrType = ((ObjectPType) target).ClrType;
                    TypeCode tc = Type.GetTypeCode(clrType);
                    switch (tc)
                    {
                        case TypeCode.String:
                            goto normalString;
                    }
                    break;
            }

            return result != null;
        }

        protected override bool InternalConvertFrom(
            StackContext sctx,
            PValue subject,
            bool useExplicit,
            out PValue result)
        {
            result = null;
            return false;
        }

        public override bool IndirectCall(
            StackContext sctx, PValue subject, PValue[] args, out PValue result)
        {
            result = null;
            var obj = subject.Value as SymbolTable<Member>;
            if (obj == null)
                return false;

            Member m;
            if (obj.TryGetValue(IndirectCallId, out m) && m != null)
                result = m.IndirectCall(sctx, _addThis(subject, args));

            return result != null;
        }

        protected override bool InternalIsEqual(PType otherType)
        {
            return otherType is StructurePType;
        }

        private const int _code = 1558687994;

        /// <summary>
        /// returns a constant hash code.
        /// </summary>
        /// <returns>A constant hash code.</returns>
        public override int GetHashCode()
        {
            return _code;
        }

        /// <summary>
        /// Returns a PTypeExpression for this structure.
        /// </summary>
        /// <returns>A PTypeExpression for this structure.</returns>
        public override string ToString()
        {
            return Literal;
        }

        #endregion

        #region ICilCompilerAware Members

        /// <summary>
        /// Asses qualification and preferences for a certain instruction.
        /// </summary>
        /// <param name="ins">The instruction that is about to be compiled.</param>
        /// <returns>A set of <see cref="CompilationFlags"/>.</returns>
        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.PreferCustomImplementation;
        }

        private static readonly MethodInfo GetStructurePType = typeof (PType).GetProperty("Structure").GetGetMethod();

        /// <summary>
        /// Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
        /// </summary>
        /// <param name="state">The compiler state.</param>
        /// <param name="ins">The instruction to compile.</param>
        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            state.EmitCall(GetStructurePType);
        }

        #endregion
    }
}