using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Prexonite.Compiler.Ast;
using Prexonite.Properties;

namespace Prexonite.Compiler.Macro.Commands
{
    public abstract class BuiltInTypeCommandBase : PartialMacroCommand
    {
        protected abstract int NumAdditionalArguments { get; }
        protected abstract string IncompleteMessageClass { get; }
        protected abstract string OperationName { get; }
        public string RegistryId { get; }

        protected BuiltInTypeCommandBase(string id, string registryId) : base(id)
        {
            RegistryId = registryId;
        }

        [PublicAPI]
        public const string SupportsTypeArgumentsId = @"pxs\supportsTypeArguments";

        protected static bool SupportsFeature<T>(Loader ldr, AstNode node, string metaSwitchId) where T: MacroCommand
        {
            if (node is AstExpand {Entity: {} entityRef})
            {
                if (entityRef.TryGetMacroCommand(out var mCmdRef) 
                    && mCmdRef.TryGetEntity(ldr, out var entity)
                    && entity is { Value: MacroCommand mCmd })
                {
                    return mCmd is T;
                }
                else if (entityRef.TryGetFunction(out var funcRef)
                    && funcRef.TryGetEntity(ldr, out var funcValue)
                    && funcValue is {Value: PFunction func})
                {
                    return func.Meta[metaSwitchId].Switch;
                }
                else
                {
                    return false;
                }
            }
            else if (node is AstIndirectCall {Subject: AstReference {Entity: { } callRef}})
            {
                if (callRef.TryGetFunction(out var funcRef)
                    && funcRef.TryGetEntity(ldr, out var funcValue)
                    && funcValue is {Value: PFunction func})
                {
                    return func.Meta[metaSwitchId].Switch;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        
        public static bool SupportsTypeArguments(Loader ldr, AstNode node) => 
            SupportsFeature<BuiltInTypeCommandBase>(ldr, node, SupportsTypeArgumentsId);

        protected sealed override void DoExpand(MacroContext context)
        {
            if (context.Invocation.Arguments.Count < NumAdditionalArguments + 1)
            {
                var message = string.Format(
                    Resources.BuiltInTypeCommandBase_DoExpand_0_for_type_1_requires_at_least_2_arguments,
                    OperationName,
                    RegistryId,
                    NumAdditionalArguments + 1,
                    context.Invocation.Arguments.Count
                );
                context.ReportMessage(Message.Error(message, context.Invocation.Position, IncompleteMessageClass));
                return;
            }

            if (context.GetOptimizedNode(context.Invocation.Arguments[0]) is not AstConstant {Constant: int arity})
            {
                context.ReportMessage(Message.Error(string.Format(Resources.BuiltInTypeCommandBase_DoExpand_0_for_type_1_requires_an_arity_integer_as_the_first_argument, "Static call", RegistryId), context.Invocation.Position, MessageClasses.IncompleteBuiltinStaticCall));
                return;
            }
            
            var typeArguments = context.Invocation.Arguments.Skip(1).Take(arity);
            var typeExpr = new AstDynamicTypeExpression(context.Invocation.Position, RegistryId);
            typeExpr.Arguments.AddRange(typeArguments);

            Instantiate(context, typeExpr,
                context.Invocation.Arguments.Skip(1 + arity).Take(NumAdditionalArguments),
                context.Invocation.Arguments.Skip(1 + arity + NumAdditionalArguments));
        }

        protected abstract void Instantiate(MacroContext context, 
            AstDynamicTypeExpression typeExpr,
            IEnumerable<AstExpr> additionalArguments, 
            IEnumerable<AstExpr> operationArguments);

        protected override bool DoExpandPartialApplication(MacroContext context)
        {
            DoExpand(context);
            return true;
        }
    }
}