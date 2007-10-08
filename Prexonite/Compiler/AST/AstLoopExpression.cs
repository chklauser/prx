using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstLoopExpression : AstNode,
                                     IAstExpression,
                                     IAstHasBlocks
    {
        public AstLoopExpression(string file, int line, int column, AstLoop loop)
            : base(file, line, column)
        {
            Loop = loop;
        }

        internal AstLoopExpression(Parser p, AstLoop loop)
            : base(p)
        {
            Loop = loop;
        }

        public AstLoop Loop;
        private string lstVar;
        private string tmpVar;
        private bool useTmpVar = false;

        #region IAstHasBlocks Members

        public AstBlock[] Blocks
        {
            get { return Loop.Blocks; }
        }

        #endregion

        private static bool _tmpIsUsed(AstBlock block)
        {
            //Scanning pass (find out if tmp is used or not)
            for (int i = 0; i < block.Count; i++)
            {
                AstReturn ret = block[i] as AstReturn;
                if (ret != null)
                {
                    if ((ret.ReturnVariant == ReturnVariant.Continue && ret.Expression == null)
                        || ret.ReturnVariant == ReturnVariant.Set)
                        return true;
                }

                IAstHasBlocks hasBlocks = block[i] as IAstHasBlocks;
                if (hasBlocks != null)
                {
                    foreach (AstBlock subBlock in hasBlocks.Blocks)
                    {
                        if (_tmpIsUsed(subBlock))
                            return true;
                    }
                }
            }

            return false;
        }

        private void _transformBlock(AstBlock block)
        {
            for (int i = 0; i < block.Count; i++)
            {
                #region Transformation

                AstReturn ret = block[i] as AstReturn;
                if (ret != null)
                {
                    if (ret.ReturnVariant == ReturnVariant.Continue)
                    {
                        if (ret.Expression == null)
                        {
                            //Replace {yield;} by {lst[] = tmp;}
                            AstGetSet addTmpToList =
                                new AstGetSetMemberAccess(
                                    ret.File,
                                    ret.Line,
                                    ret.Column,
                                    PCall.Set,
                                    new AstGetSetSymbol(
                                        ret.File,
                                        ret.Line,
                                        ret.Column,
                                        PCall.Get,
                                        lstVar,
                                        SymbolInterpretations.LocalObjectVariable),
                                    "");
                            addTmpToList.Arguments.Add(
                                new AstGetSetSymbol(
                                    ret.File,
                                    ret.Line,
                                    ret.Column,
                                    PCall.Get,
                                    tmpVar,
                                    SymbolInterpretations.LocalObjectVariable));
                            block[i] = addTmpToList;
                        }
                        else
                        {
                            //Replace {yield expr;} by {if($useTmpVar) tmp = expr; lst[] = if($useTmpVar) tmp else expr;}

                            if (useTmpVar)
                            {
                                AstBlock replacement = new AstBlock(ret.File, ret.Line, ret.Column);
                                AstGetSetSymbol setTmp =
                                    new AstGetSetSymbol(
                                        ret.File,
                                        ret.Line,
                                        ret.Column,
                                        PCall.Set,
                                        tmpVar,
                                        SymbolInterpretations.LocalObjectVariable);
                                setTmp.Arguments.Add(ret.Expression);
                                AstGetSet addExprToList =
                                    new AstGetSetMemberAccess(
                                        ret.File,
                                        ret.Line,
                                        ret.Column,
                                        PCall.Set,
                                        new AstGetSetSymbol(
                                            ret.File,
                                            ret.Line,
                                            ret.Column,
                                            PCall.Get,
                                            lstVar,
                                            SymbolInterpretations.LocalObjectVariable),
                                        "");
                                addExprToList.Arguments.Add(
                                    new AstGetSetSymbol(
                                        ret.File,
                                        ret.Line,
                                        ret.Column,
                                        PCall.Get,
                                        tmpVar,
                                        SymbolInterpretations.LocalObjectVariable));

                                replacement.Add(setTmp);
                                replacement.Add(addExprToList);

                                block[i] = replacement;
                            }
                            else
                            {
                                AstGetSet addExprToList =
                                    new AstGetSetMemberAccess(
                                        ret.File,
                                        ret.Line,
                                        ret.Column,
                                        PCall.Set,
                                        new AstGetSetSymbol(
                                            ret.File,
                                            ret.Line,
                                            ret.Column,
                                            PCall.Get,
                                            lstVar,
                                            SymbolInterpretations.LocalObjectVariable),
                                        "");
                                addExprToList.Arguments.Add(ret.Expression);
                                block[i] = addExprToList;
                            }
                        }
                    }
                    else if (ret.ReturnVariant == ReturnVariant.Set)
                    {
                        //Replace {return = expr;} and {yield = expr;} by {tmp = expr;}.
                        AstGetSet setTmp =
                            new AstGetSetSymbol(
                                ret.File,
                                ret.Line,
                                ret.Column,
                                PCall.Set,
                                tmpVar,
                                SymbolInterpretations.LocalObjectVariable);
                        setTmp.Arguments.Add(ret.Expression);
                        block[i] = setTmp;
                    }
                }

                #endregion

                #region Recursive Descent

                IAstHasBlocks hasBlocks = block[i] as IAstHasBlocks;

                if (hasBlocks != null)
                {
                    foreach (AstBlock subBlock in hasBlocks.Blocks)
                    {
                        _transformBlock(subBlock);
                    }
                }

                #endregion
            }
        }

        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            if (lstVar != null)
                goto leave;

            //Perform statement to expression transformation
            lstVar = Loop.Labels.CreateLabel("lst");
            tmpVar = Loop.Labels.CreateLabel("tmp");

            foreach (AstBlock block in Loop.Blocks)
            {
                if (_tmpIsUsed(block))
                    useTmpVar = true;
            }

            foreach (AstBlock block in Loop.Blocks)
            {
                _transformBlock(block);
            }

            leave: //Optimization occurs during code generation
            expr = null;
            return false;
        }

        public override void EmitCode(CompilerTarget target)
        {
            if (lstVar == null)
            {
                IAstExpression dummy; //Won't return anything anyway...
                TryOptimize(target, out dummy);
            }

            //Register variables
            target.Function.Variables.Add(lstVar);
            if (useTmpVar)
                target.Function.Variables.Add(tmpVar);

            //Initialize the list
            target.EmitStaticGetCall(0, "List", "Create");
            target.EmitStoreLocal(lstVar);

            //Emit the modified loop
            Loop.EmitCode(target);

            //Return the list
            target.EmitLoadLocal(lstVar);
        }
    }
}