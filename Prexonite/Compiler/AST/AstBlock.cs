/*
 * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
 *  Copyright (C) 2007  Christian "SealedSun" Klauser
 *  E-mail  sealedsun a.t gmail d.ot com
 *  Web     http://www.sealedsun.ch/
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler.Ast
{
    public class AstBlock : AstNode,
                            IList<AstNode>
    {
        [NoDebug]
        public AstBlock(string file, int line, int column)
            : base(file, line, column)
        {
        }

        [NoDebug]
        internal AstBlock(Parser p)
            : base(p)
        {
        }

        public List<AstNode> Statements = new List<AstNode>();

        public override void EmitCode(CompilerTarget target)
        {
            if (target == null)
                throw new ArgumentNullException("target", "The compiler target cannot be null");
            foreach (AstNode node in Statements)
            {
                AstNode stmt = node;
                IAstExpression expr = stmt as IAstExpression;
                if (expr != null)
                    stmt = (AstNode) GetOptimizedNode(target, expr);

                if (stmt is IAstEffect)
                    ((IAstEffect) stmt).EmitEffectCode(target);
                else
                    stmt.EmitCode(target);
            }
        }

        public virtual  bool IsEmpty
        {
            get { return Count == 0; }
        }

        public virtual bool IsSingleStatement
        {
            get { return Count == 1; }
        }

        #region IList<AstNode> Members

        [NoDebug]
        public int IndexOf(AstNode item)
        {
            return Statements.IndexOf(item);
        }

        [NoDebug]
        public void Insert(int index, AstNode item)
        {
            Statements.Insert(index, item);
        }

        [NoDebug]
        public void RemoveAt(int index)
        {
            Statements.RemoveAt(index);
        }

        public AstNode this[int index]
        {
            [NoDebug]
            get { return Statements[index]; }
            [NoDebug]
            set { Statements[index] = value; }
        }

        #endregion

        #region ICollection<AstNode> Members

        [NoDebug]
        public void Add(AstNode item)
        {
            Statements.Add(item);
        }

        [NoDebug]
        public void AddRange(IEnumerable<AstNode> collection)
        {
            Statements.AddRange(collection);
        }

        [NoDebug]
        public void Clear()
        {
            Statements.Clear();
        }

        [NoDebug]
        public bool Contains(AstNode item)
        {
            return Statements.Contains(item);
        }

        [NoDebug]
        public void CopyTo(AstNode[] array, int arrayIndex)
        {
            Statements.CopyTo(array, arrayIndex);
        }

        [NoDebug]
        public int Count
        {
            get { return Statements.Count; }
        }

        public bool IsReadOnly
        {
            [NoDebug]
            get { return ((IList<AstNode>) Statements).IsReadOnly; }
        }

        [NoDebug]
        public bool Remove(AstNode item)
        {
            return Statements.Remove(item);
        }

        #endregion

        #region IEnumerable<AstNode> Members

        [NoDebug]
        public IEnumerator<AstNode> GetEnumerator()
        {
            return Statements.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        [NoDebug]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Statements.GetEnumerator();
        }

        #endregion

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            foreach (AstNode node in Statements)
                buffer.AppendFormat("{0} ;", node);
            return buffer.ToString();
        }
    }
}