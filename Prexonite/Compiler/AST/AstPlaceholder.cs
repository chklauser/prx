﻿// Prexonite
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Prexonite.Properties;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast;

/// <summary>
///     AST node that represents a partial application placeholder ('?'). Optionally has an index assigned (e.g., '?5')
/// </summary>
public class AstPlaceholder : AstGetSetImplBase
{
    public const int MaxPlaceholderIndex = 127;

    private int? _index;

    /// <summary>
    ///     The explicit argument index, if one is set. 0-based.
    /// </summary>
    public int? Index
    {
        get => _index;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value),
                    Resources.AstPlaceholder_PlaceholdeIndexNegative);
            _index = value;
        }
    }

    public AstPlaceholder(string file, int line, int column) : this(file, line, column, null)
    {
    }

    public AstPlaceholder(string file, int line, int column, int? index)
        : base(file, line, column, PCall.Get)
    {
        Index = index;
    }

    internal AstPlaceholder(Parser p, int? index = null) : base(p, PCall.Get)
    {
        Index = index;
    }

    #region Overrides of AstNode

    protected override void EmitGetCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        _throwSyntaxNotSupported();
    }

    private void _throwSyntaxNotSupported()
    {
        throw new PartialApplicationSyntaxNotSupportedException(
            $"This syntax does not support placeholders. (Position {File}:{Line} col {Column})");
    }

    protected override void EmitSetCode(CompilerTarget target)
    {
        _throwSyntaxNotSupported();
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
                if (placeholder.Index.Value > 127)
                {
                    throw new PrexoniteException(
                        $"The placeholder (at {placeholder.Position.GetSourcePositionString()}) has a custom index value that exceeds the maximum mappable index.");
                }

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
                Debug.Assert(ReferenceEquals(assigned[placeholder.Index.Value], placeholder),
                    "placeholder was not inserted at the right spot.");
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

            if (index > MaxPlaceholderIndex)
                throw new PrexoniteException(
                    $"The placeholder (at {placeholder.Position.GetSourcePositionString()}) would be assigned an index that exceeds the maximum mappable index.");
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

[Serializable]
public class PartialApplicationSyntaxNotSupportedException : PrexoniteException
{
    public PartialApplicationSyntaxNotSupportedException()
    {
    }

    public PartialApplicationSyntaxNotSupportedException(string message) : base(message)
    {
    }

    public PartialApplicationSyntaxNotSupportedException(string message, Exception inner)
        : base(message, inner)
    {
    }

    protected PartialApplicationSyntaxNotSupportedException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}