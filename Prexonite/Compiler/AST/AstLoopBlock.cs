using System.Diagnostics;

namespace Prexonite.Compiler.Ast
{
    public class AstLoopBlock : AstSubBlock, ILoopBlock
    {
        public const string ContinueWord = "continue";
        public const string BreakWord = "break";
        public const string BeginWord = "begin";
        private readonly string _continueLabel;
        private readonly string _breakLabel;
        private readonly string _beginLabel;

        [DebuggerStepThrough]
        public AstLoopBlock(string file, int line, int column, AstBlock parentBlock, 
                            string uid = null,
                            string prefix = null)
            : this (new SourcePosition(file,line,column), parentBlock, uid, prefix)
        {
            
        }

        [DebuggerStepThrough]
        internal AstLoopBlock(ISourcePosition p, AstBlock parentNode = null, string uid = null, string prefix = null)
            : base(p, parentNode, uid, prefix)
        {
            //See other ctor!
            _continueLabel = CreateLabel(ContinueWord);
            _breakLabel = CreateLabel(BreakWord);
            _beginLabel = CreateLabel(BeginWord);
        }

        public string ContinueLabel
        {
            get { return _continueLabel; }
        }

        public string BreakLabel
        {
            get { return _breakLabel; }
        }

        public string BeginLabel
        {
            get { return _beginLabel; }
        }
    }
}