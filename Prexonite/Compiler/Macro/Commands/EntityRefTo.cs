using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Prexonite.Compiler.Ast;
using Prexonite.Compiler.Internal;
using Prexonite.Modular;
using Prexonite.Types;

namespace Prexonite.Compiler.Macro.Commands
{
    /// <summary>
    /// Implements the <code>entityref_to(x)</code> syntax, where <code>x</code> is a simple call or expansion prototype. 
    /// Expands into an expression that constructs a reference to the entity called by or expanded by <code>x</code>.
    /// </summary>
    public class EntityRefTo : MacroCommand
    {
        public const string Alias = "entityref_to";

        public EntityRefTo() : base(Alias)
        {
        }

        #region Singleton

        private static readonly EntityRefTo _instance = new EntityRefTo();

        public static EntityRefTo Instance
        {
            get { return _instance; }
        }

        #endregion

        protected override void DoExpand(MacroContext context)
        {
            // entityref_to(IndirectCall(Reference(X))) or entityref_to(Expand(X))
            //  results in an expression that produces the entity reference X at runtime

            if (context.Invocation.Arguments.Count == 0)
            {
                context.ReportMessage(Message.Error(string.Format("{0} requires one argument.", Alias),
                    context.Invocation.Position, MessageClasses.EntityRefTo));
                return;
            }
            
            var prototype = context.Invocation.Arguments[0];
            EntityRef entityRef;
            AstExpand expand;
            if ((expand = prototype as AstExpand) != null)
            {
                entityRef = expand.Entity;
            }
            else if (prototype.TryMatchCall(out entityRef))
            {
                // entityRef already assigned
            }
            else
            {
                context.ReportMessage(
                    Message.Error(
                        string.Format(
                            "{0} requires its argument to be a direct call or expansion. Instead a {1} was supplied.",
                            Alias, prototype.GetType().Name), context.Invocation.Position, MessageClasses.EntityRefTo));
                return;
            }

            context.Block.Expression = ToExpr(context.Factory, context.Invocation.Position, entityRef);
        }

        private class Lifter : IEntityRefMatcher<Tuple<IAstFactory,ISourcePosition>,AstExpr>
        {
            private static AstExpr _lift<T>(Tuple<IAstFactory, ISourcePosition> argument, params object[] callArgs) where T : EntityRef
            {
                var create = argument.Item1;
                var pos = argument.Item2;
                var call = create.StaticMemberAccess(pos,
                    create.ConstantType(pos, PType.Object[typeof (T)].ToString()), "Create");
                call.Arguments.AddRange(from arg in callArgs select create.Constant(pos, arg));
                return call;
            }

            public AstExpr OnFunction(EntityRef.Function function, Tuple<IAstFactory, ISourcePosition> argument)
            {
                return _lift<EntityRef.Function>(argument, function.Id, function.ModuleName);
            }

            public AstExpr OnCommand(EntityRef.Command command, Tuple<IAstFactory, ISourcePosition> argument)
            {
                return _lift<EntityRef.Command>(argument, command.Id);
            }

            public AstExpr OnMacroCommand(EntityRef.MacroCommand macroCommand, Tuple<IAstFactory, ISourcePosition> argument)
            {
                return _lift<EntityRef.MacroCommand>(argument, macroCommand.Id);
            }

            public AstExpr OnLocalVariable(EntityRef.Variable.Local variable, Tuple<IAstFactory, ISourcePosition> argument)
            {
                return _lift<EntityRef.Variable.Local>(argument, variable.Id);
            }

            public AstExpr OnGlobalVariable(EntityRef.Variable.Global variable, Tuple<IAstFactory, ISourcePosition> argument)
            {
                return _lift<EntityRef.Variable.Global>(argument, variable.Id, variable.ModuleName);
            }
        }
        private static readonly Lifter _lifter = new Lifter();

        [NotNull]
        public static AstExpr ToExpr([NotNull] IAstFactory factory, [NotNull] ISourcePosition position, [NotNull] EntityRef entityRef)
        {
            if (entityRef == null)
                throw new ArgumentNullException("entityRef");

            return entityRef.Match(_lifter, Tuple.Create(factory, position));
        }
    }
}
