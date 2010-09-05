using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    /// <summary>
    /// AST node that represents a partial application placeholder ('?'). Optionally has an index assigned (e.g., '?5')
    /// </summary>
    public class AstPlaceholder : AstGetSet, IAstExpression
    {
        private int? _index;
        public int? Index
        {
            get { return _index; }
            set
            {
                if(value.HasValue && value.Value < 0)
                    throw new ArgumentOutOfRangeException("value","A placeholder index cannot be negtive");
                _index = value;
            }
        }

        public AstPlaceholder(string file, int line, int column) : this(file, line, column, null)
        {
        }

        public AstPlaceholder(string file, int line, int column, int? index) : base(file, line, column, PCall.Get)
        {
            Index = index;
        }

        internal AstPlaceholder(Parser p, int? index = null) : base(p,PCall.Get)
        {
            Index = index;
        }

        #region Overrides of AstNode

        protected override void EmitGetCode(CompilerTarget target, bool justEffect)
        {
            throw new PrexoniteException(string.Format("This syntax does not support placeholders. {0}:{1} col {2}", File, Line, Column));
        }

        protected override void EmitSetCode(CompilerTarget target)
        {
            throw new PrexoniteException(string.Format("This syntax does not support placeholders. {0}:{1} col {2}", File, Line, Column));
        }

        public override AstGetSet GetCopy()
        {
            var copy = new AstPlaceholder(File, Line, Column, Index);
            CopyBaseMembers(copy);
            return copy;
        }

        #endregion

        public static void DeterminePlaceholderIndices(IEnumerable<AstPlaceholder> placeholders)
        {
            //Placeholders must be assigned in two phases, because placeholders that already have an index set take priority
            var assigned = new List<AstPlaceholder>();
            var unassigned = new List<AstPlaceholder>();

            //Phase 1: assign placeholders with an index set, and keep the rest for later (but retain their order)
            foreach (var placeholder in placeholders)
            {
                if (placeholder.Index.HasValue)
                {
                    if (assigned.Count <= placeholder.Index)
                    {
                        for (var i = assigned.Count; i < placeholder.Index; i++)
                            assigned.Add(null);
                        assigned.Add(placeholder);
                    }
                    else
                    {
                        assigned[placeholder.Index.Value] = placeholder;
                    }
                    Debug.Assert(ReferenceEquals(assigned[placeholder.Index.Value], placeholder), "placeholder was not inserted at the right spot.");
                }
                else
                {
                    unassigned.Add(placeholder);
                }
            }

            //Phase 2: assign placeholders with no index in the order they appeared
            var index = 0;
            foreach (var placeholder in unassigned)
            {
                //search for free spot
                for (; index < assigned.Count; index++)
                    if (assigned[index] == null)
                        break;
                //it is not actually necessary to add the placeholder to the assigned list
                //  instead we just assign the index it would occupy
                placeholder.Index = index++;
            }
        }

        public override string ToString()
        {
            if (_index.HasValue)
                return "?" + _index.Value;
            else
                return "?";
        }
    }
}
