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

using System.Collections.ObjectModel;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Prexonite.Modular;

/// <summary>
/// A Prexonite module is a named container for  Prexonite functions and variable declarations.
/// </summary>
[DebuggerDisplay("module {Name}")]
public abstract class Module : IHasMetaTable, IMetaFilter
{
    public const string NameKey = Application.NameKey;
    public const string ReferencesKey = "references";
    public const string NoStandardLibraryKey = "nostdlib";

    public abstract ModuleName Name { get; }

    public abstract MetaTable Meta { get; }

    public abstract FunctionTable Functions { get; }

    public abstract VariableTable Variables { get; }

    public abstract CentralCache Cache { get; internal set; }

    #region IMetaFilter Members

    [DebuggerStepThrough]
    string? IMetaFilter.GetTransform(string? key)
    {
        return GetTransform(key);
    }

    protected virtual string? GetTransform(string? key)
    {
        if (Engine.StringsAreEqual(key, Application.NameKey))
            return Application.IdKey;
        else if (Engine.StringsAreEqual(key, "imports"))
            return Application.ImportKey;
        else
            return key;
    }

    [DebuggerStepThrough]
    KeyValuePair<string?, MetaEntry>? IMetaFilter.SetTransform(KeyValuePair<string?, MetaEntry> item)
    {
        return SetTransform(item);
    }

    protected virtual KeyValuePair<string?, MetaEntry>? SetTransform(KeyValuePair<string?, MetaEntry> item)
    {
        if (Engine.StringsAreEqual(item.Key, Application.NameKey))
            item = new(Application.IdKey, item.Value);
        else if (Engine.StringsAreEqual(item.Key, "imports"))
            item = new(Application.ImportKey, item.Value);
        return item;
    }
        
    #endregion

    public static Module Create(ModuleName moduleName)
    {
        var moduleImpl = new ModuleImpl(moduleName);
        Debug.Assert(moduleImpl.Functions.Contains(Application.InitializationId));
        return moduleImpl;
    }

    public FunctionDeclaration CreateFunction(string id)
    {
        if (Functions.Contains(id))
            throw new PrexoniteException(
                $"Cannot declare function with physical ID '{id}'. A function with that ID already exists in {Name}.");
        var decl = FunctionDeclaration._Create(id,this);
        Functions.Add(decl);
        return decl;
    }
}

public class FunctionTable : KeyedCollection<string, FunctionDeclaration>
{
    public FunctionTable() : base(StringComparer.InvariantCultureIgnoreCase)
    {
    }

    protected override string GetKeyForItem(FunctionDeclaration item)
    {
        return item.Id;
    }

    /// <summary>
    /// Returns the function declaration with the specified id, if it exists in the table.
    /// </summary>
    /// <param name="id">The physical id of the function declaration to return.</param>
    /// <param name="declaration">On success, will contain the function declaration. Undefined on failure.</param>
    /// <returns>True on success; false otherwise</returns>
    [PublicAPI]
    public bool TryGetFunction(string id, [NotNullWhen(true)] out FunctionDeclaration? declaration)
    {
        if(Contains(id))
        {
            declaration = this[id];
            return true;
        }
        else
        {
            declaration = null;
            return false;
        }
    }

    public void Store(TextWriter writer)
    {
        foreach (var decl in this)
            decl.Store(writer);
    }
}

class ModuleImpl : Module
{
    CentralCache _cache = CentralCache.Create();

    public ModuleImpl(ModuleName name)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));
            
        Name = name;
        var m = MetaTable.Create(this);
        m[Application.IdKey] = name.ToMetaEntry();
        _meta = m;
        _meta[Application.EntryKey] = Application.DefaultEntryFunction;
        _meta[Application.ImportKey] = Application.DefaultImport;

        _functions.Add(FunctionDeclaration._Create(Application.InitializationId,this));
    }

    public override CentralCache Cache
    {
        get => _cache;
        internal set => _cache = value ?? throw new ArgumentNullException(nameof(value));
    }

    readonly FunctionTable _functions = new();
    readonly MetaTable _meta; // must be assigned in the constructor

    public override ModuleName Name { get; }

    public override MetaTable Meta => _meta;

    public override FunctionTable Functions => _functions;

    public override VariableTable Variables { get; } = new();
}