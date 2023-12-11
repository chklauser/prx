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

using Prexonite.Compiler.Ast;
using Prexonite.Properties;

namespace Prexonite.Compiler;

class ParserAstFactory : AstFactoryBase
{
    readonly Parser _parser;

    protected override AstBlock CurrentBlock => _parser.CurrentBlock ??
        throw new PrexoniteException("Internal error: current block cannot be accessed on the top level.");

    protected override AstGetSet CreateNullNode(ISourcePosition position)
    {
        return Parser._NullNode(position);
    }

    protected override bool IsOuterVariable(string id)
    {
        if (_parser.target == null)
            return false;
        else
            return _parser.target._IsOuterVariable(id);
    }

    protected override void RequireOuterVariable(string id)
    {
        if (_parser.target == null)
        {
            ReportMessage(
                Message.Error(Resources.ParserAstFactory_RequireOuterVariable_Outside_function,
                    _parser.GetPosition(),
                    MessageClasses.ParserInternal));
        }
        else
        {
            _parser.target.RequireOuterVariable(id);
        }
    }

    public override void ReportMessage(Message message)
    {
        _parser.Loader.ReportMessage(message);
    }

    protected override CompilerTarget CompileTimeExecutionContext
    {
        get
        {
            var compilerTarget = _parser.target;
            if (compilerTarget == null)
            {
                throw new InvalidOperationException("Internal parser error. Cannot access compilation target on top level.");
            }
            else
            {
                return compilerTarget;
            }
        }
    }

    public ParserAstFactory(Parser parser)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
    }
}