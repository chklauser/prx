using System;
using System.Collections.Generic;
using System.Text;

namespace Prexonite.Types
{
    [PTypeLiteral(Literal)]
    public class StructurePType : PType
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

        /// <summary>
        /// Returns a string that both identifies and defines the structure.
        /// </summary>
        public string TypeSignature
        {
            get
            {
                if (_typeSignature == null)
                    _typeSignature = ToString();
                return _typeSignature;
            }
        }

        private string _typeSignature = null;

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
                else
                {
                    if (call == PCall.Set && args.Length != 0)
                        Value = args[1];
                    return Value;
                }
            }

            #region IIndirectCall Members

            public PValue IndirectCall(StackContext sctx, PValue[] args)
            {
                if (IsReference)
                    return Value.IndirectCall(sctx, args);
                else
                {
                    if (args.Length > 1)
                        Value = args[1];
                    return Value;
                }
            }

            #endregion
        }

        private SymbolTable<Member> _prototypes = new SymbolTable<Member>();

        private static string[] _toStringArray(StackContext sctx, PValue[] args)
        {
            string[] sargs = new string[args.Length];
            for (int i = 0; i < sargs.Length; i++)
                sargs[i] = args[i] != null ? args[i].CallToString(sctx) : null;
            return sargs;
        }

        public StructurePType(StackContext sctx, PValue[] args)
            : this(_toStringArray(sctx, args))
        {
        }

        public StructurePType(params string[] definitionElements)
        {
            if (definitionElements == null)
                throw new ArgumentNullException("definitionElements");

            for (int i = 0; i < definitionElements.Length; i++)
            {
                string s = definitionElements[i];
                if (s == null)
                    s = "";
                bool reference = false;
                if (s.Equals("r", StringComparison.InvariantCultureIgnoreCase) &&
                    i < definitionElements.Length - 1) // "r" is not last element
                {
                    //Treat next element as a reference instead
                    reference = true;
                    s = definitionElements[++i];
                }

                if (_prototypes.ContainsKey(s))
                    throw new ArgumentException("Duplicate definition of member " + s);
                _prototypes.Add(s, new Member(reference));
            }
        }

        #endregion

        #region PType implementation

        internal static PValue[] _addThis(PValue Subject, PValue[] args)
        {
            PValue[] argst = new PValue[args.Length + 1];
            argst[0] = Subject;
            Array.Copy(args, 0, argst, 1, args.Length);
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
            SymbolTable<Member> obj = subject.Value as SymbolTable<Member>;
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

                        string mid = (string) args[0].ConvertTo(sctx, String).Value;

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
                        _typeSignature = null; //Make sure, _typeSignature is reset.

                        result = m.Value;

                        break;
                    default:
                        //Try to call the generic "call" member
                        if (obj.TryGetValue(CallId, out m) && m != null)
                            result = m.Invoke(sctx, _addThis(subject, argst), call);
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
            //Create structure
            SymbolTable<Member> obj = new SymbolTable<Member>(_prototypes.Count);
            foreach (KeyValuePair<string, Member> kvp in _prototypes)
                obj.Add(kvp.Key, new Member(kvp.Value.IsReference));

            result = new PValue(obj, this);
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
            SymbolTable<Member> obj = subject.Value as SymbolTable<Member>;
            if (obj == null)
                return false;

            /*
            if(target is StringPType)
            {
                StringBuilder sb = new StringBuilder("{");
                foreach (KeyValuePair<string, Member> kvp in obj)
                {
                    sb.Append(kvp.Key);
                    sb.Append(" => ");
                    sb.Append(kvp.Value.Value);
                    sb.Append(", ");
                }
                if(obj.Count > 0)
                    sb.Length -= 2;
                sb.Append("}");
                result = sb.ToString();
            }
            //*/

            if (target is StringPType || target == Object[typeof(string)])
            {
                if (
                    !TryDynamicCall(
                         sctx, subject, new PValue[] {}, PCall.Get, "ToString", out result))
                    result = null;
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
            SymbolTable<Member> obj = subject.Value as SymbolTable<Member>;
            if (obj == null)
                return false;

            Member m;
            if (obj.TryGetValue(IndirectCallId, out m) && m != null)
                result = m.IndirectCall(sctx, _addThis(subject, args));

            return result != null;
        }

        protected override bool InternalIsEqual(PType otherType)
        {
            StructurePType s = otherType as StructurePType;
            if (s == null)
                return false;

            return
                s.TypeSignature.Equals(TypeSignature, StringComparison.InvariantCultureIgnoreCase);
        }

        private const int _code = 1558687994;

        /// <summary>
        /// returns a hash code based on the <see cref="TypeSignature"/>.
        /// </summary>
        /// <returns>A hash code based on the <see cref="TypeSignature"/>.</returns>
        public override int GetHashCode()
        {
            return _CombineHashes(_code, TypeSignature.GetHashCode());
        }

        /// <summary>
        /// Returns a PTypeExpression for this structure.
        /// </summary>
        /// <returns>A PTypeExpression for this structure.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(Literal);
            sb.Append("(");
            foreach (KeyValuePair<string, Member> kvp in _prototypes)
            {
                if (kvp.Value.IsReference)
                    sb.Append("r,");
                sb.Append(StringPType.ToIdOrLiteral(kvp.Key));
                sb.Append(",");
            }
            if (_prototypes.Count != 0)
                sb.Length -= 1;
            sb.Append(")");
            return sb.ToString();
        }

        #endregion
    }
}