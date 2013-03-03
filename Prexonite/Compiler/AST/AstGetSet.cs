using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public abstract class AstGetSet : AstExpr, IAstHasExpressions
    {
        protected AstGetSet([NotNull] ISourcePosition position) : base(position)
        {
        }

        [NotNull]
        public abstract ArgumentsProxy Arguments { [DebuggerNonUserCode] get; }

        /// <summary>
        ///     <para>Indicates whether this node uses get or set syntax</para>
        ///     <para>(set syntax involves an equal sign (=); get syntax does not)</para>
        /// </summary>
        public abstract PCall Call { get; set; }

        [NotNull]
        public virtual AstExpr[] Expressions
        {
            get { return Arguments.ToArray(); }
        }

        public virtual int DefaultAdditionalArguments
        {
            get { return 0; }
        }

        public override bool TryOptimize(CompilerTarget target, out AstExpr expr)
        {
            expr = null;

            //Optimize arguments
            for (var i = 0; i < Arguments.Count; i++)
            {
                var arg = Arguments[i];
                if (arg == null)
                    throw new PrexoniteException(
                        "Invalid (null) argument in GetSet node (" + ToString() +
                            ") detected at position " + Arguments.IndexOf(arg) + ".");
                var oArg = _GetOptimizedNode(target, arg);
                if (!ReferenceEquals(oArg, arg))
                    Arguments[i] = oArg;
            }

            return false;
        }

        protected void EmitArguments(CompilerTarget target)
        {
            EmitArguments(target, false, DefaultAdditionalArguments);
        }

        protected void EmitArguments(CompilerTarget target, bool duplicateLast)
        {
            EmitArguments(target, duplicateLast, DefaultAdditionalArguments);
        }

        protected void EmitArguments(CompilerTarget target, bool duplicateLast,
            int additionalArguments)
        {
            Object lastArg = null;
            foreach (AstExpr expr in Arguments)
            {
                Debug.Assert(expr != null,
                    "Argument list of get-set-complex contains null reference");
                if (ReferenceEquals(lastArg, expr))
                    target.EmitDuplicate(Position);
                else
                    expr.EmitValueCode(target);
                lastArg = expr;
            }
            var argc = Arguments.Count;
            if (duplicateLast && argc > 0)
            {
                target.EmitDuplicate(Position);
                if (argc + additionalArguments > 1)
                    target.EmitRotate(Position, -1, argc + 1 + additionalArguments);
            }
        }

        protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            if (Call == PCall.Get)
            {
                EmitArguments(target);
                EmitGetCode(target, stackSemantics);
            }
            else
            {
                EmitArguments(target, stackSemantics == StackSemantics.Value);
                EmitSetCode(target);
            }
        }

        #region AstExpr Members

        protected abstract void EmitGetCode(CompilerTarget target, StackSemantics stackSemantics);
        protected abstract void EmitSetCode(CompilerTarget target);

        #endregion

        protected string ArgumentsToString()
        {
            var buffer = new StringBuilder();
            buffer.Append("(");
            var i = 0;
            foreach (AstExpr expr in Arguments)
            {
                i++;

                if (expr != null)
                    buffer.Append(expr);
                else
                    buffer.Append("{null}");

                if (i != Arguments.Count)
                    buffer.Append(", ");
            }
            return buffer + ")";
        }

        /// <summary>
        ///     Copies the base class fields from this to the target.
        /// </summary>
        /// <param name = "target">The object that shall reveice the values from this object.</param>
        protected virtual void CopyBaseMembers(AstGetSet target)
        {
            target.Arguments.AddRange(Arguments);
        }

        public override bool CheckForPlaceholders()
        {
            return this is IAstPartiallyApplicable &&
                (base.CheckForPlaceholders() || Arguments.Any(AstPartiallyApplicable.IsPlaceholder));
        }

        public abstract AstGetSet GetCopy();

        public override string ToString()
        {
            string typeName;
            var name = Enum.GetName(typeof (PCall), Call);
            return String.Format(
                "{0}: {1}",
                name == null ? "-" : name.ToLowerInvariant(),
                (typeName = GetType().Name).StartsWith("AstGetSet")
                    ? typeName.Substring(9)
                    : typeName);
        }
    }
}