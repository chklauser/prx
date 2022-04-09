#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Prexonite.Commands;

namespace Prexonite;

public class CommandTable : SymbolTable<PCommand>
{
    private readonly SymbolTable<ICommandInfo> _fallbackCommandInfos = new();
    private readonly SymbolTable<PCommandGroups> _commandGroups = new();

    /// <summary>
    ///     Returns information about the specified command's capabilities. The command might not be installed
    ///     in the symbol table.
    /// </summary>
    /// <param name = "id">The id of the command to get information about.</param>
    /// <param name = "commandInfo">Contains the command info on success. Undefined on failure.</param>
    /// <returns>True on success; false on failure.</returns>
    public bool TryGetInfo(string id, [NotNullWhen(true)] out ICommandInfo? commandInfo)
    {
        if (TryGetValue(id, out var command))
        {
            commandInfo = command.ToCommandInfo();
            return true;
        }
        else if (_fallbackCommandInfos.TryGetValue(id, out commandInfo))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Adds a fallback command info to the command table. This will be returned by <see cref = "TryGetInfo" /> if 
    ///     no corresponding command is stored in the table at the moment.
    /// </summary>
    /// <param name = "id">The id of the command to provide fallback info for.</param>
    /// <param name = "commandInfo">The fallback command info.</param>
    public void ProvideFallbackInfo(string id, ICommandInfo commandInfo)
    {
        if (id == null)
            throw new ArgumentNullException(nameof(id));

        _fallbackCommandInfos[id] = commandInfo ?? throw new ArgumentNullException(nameof(commandInfo));
    }

    /// <summary>
    ///     Removes an <see cref = "ICommandInfo" /> from the fallback command info table.
    /// </summary>
    /// <param name = "id">The id of the command to remove fallback info from.</param>
    /// <returns>True if a fallback command info was removed. False otherwise.</returns>
    public bool RemoveFallbackInfo(string id)
    {
        if (id == null)
            throw new ArgumentNullException(nameof(id));
        return _fallbackCommandInfos.Remove(id);
    }

    /// <summary>
    ///     Determines whether a particular name is registered for a command.
    /// </summary>
    /// <param name = "name">A name.</param>
    /// <returns>True if a command with the supplied name exists; false otherwise.</returns>
    public bool Contains(string name)
    {
        return ContainsKey(name);
    }

    private void addToGroup(string id, PCommandGroups group)
    {
        if (_commandGroups.TryGetValue(id, out var existing))
        {
            _commandGroups[id] = existing | group;
        }
        else
        {
            _commandGroups[id] = group;
        }
    }

    private (bool removed, PCommandGroups remaining) removeFromGroup(string id, PCommandGroups group)
    {
        if (_commandGroups.TryGetValue(id, out var existing))
        {
            var newValue = existing & ~group;
            if (newValue == existing)
            {
                return (false, existing);
            }

            if (newValue == PCommandGroups.None)
            {
                _commandGroups.Remove(id);
                return (true, PCommandGroups.None);
            }
            else
            {
                _commandGroups[id] = newValue;
                return (true, newValue);
            }
        }
        else
        {
            return (false, PCommandGroups.None);
        }
    }

    /// <summary>
    ///     Index based access to the dictionary of available commands.
    /// </summary>
    /// <param name = "key">The name of the command to retrieve.</param>
    /// <returns>The command registered with the supplied name.</returns>
    public override PCommand this[string key]
    {
        get => TryGetValue(key, out var cmd) 
            ? cmd 
            : throw new PrexoniteException($"Cannot find command {key}.");
        set
        {
            if (value != null && !_commandGroups.ContainsKey(key))
                addToGroup(key, PCommandGroups.User);

            if (ContainsKey(key))
            {
                if (value == null)
                    Remove(key);
                else
                    base[key] = value;
            }
            else if (value == null)
                throw new ArgumentNullException(nameof(value));
            else
                base.Add(key, value);
        }
    }

    /// <summary>
    ///     Adds a new command in the user space.
    /// </summary>
    /// <param name = "alias">The alias that shall refer to the supplied command.</param>
    /// <param name = "command">A command instance.</param>
    /// <exception cref = "ArgumentNullException">If either <paramref name = "alias" /> 
    ///     or <paramref name = "command" /> is null.</exception>
    public void AddUserCommand(string alias, PCommand command)
    {
        if (alias == null)
            throw new ArgumentNullException(nameof(alias));
        if (command == null)
            throw new ArgumentNullException(nameof(command));
        addToGroup(alias, PCommandGroups.User);
        this[alias] = command;
    }

    /// <summary>
    ///     Adds a new command in the user space.
    /// </summary>
    /// <param name = "alias">The alias that shall refer to the supplied command.</param>
    /// <param name = "action">An action to be turned into a command.</param>
    /// <exception cref = "ArgumentNullException">If either <paramref name = "alias" /> 
    ///     or <paramref name = "action" /> is null.</exception>
    public void AddUserCommand(string alias, PCommandAction action)
    {
        if (alias == null)
            throw new ArgumentNullException(nameof(alias));
        if (action == null)
            throw new ArgumentNullException(nameof(action));
        AddUserCommand(alias, new DelegatePCommand(action));
    }

    /// <summary>
    ///     Adds a new command in the user space.
    /// </summary>
    /// <param name = "alias">The alias that shall refer to the supplied command.</param>
    /// <param name = "action">An action to be turned into a command.</param>
    /// <exception cref = "ArgumentNullException">If either <paramref name = "alias" /> 
    ///     or <paramref name = "action" /> is null.</exception>
    public void AddUserCommand(string alias, ICommand action)
    {
        if (alias == null)
            throw new ArgumentNullException(nameof(alias));
        if (action == null)
            throw new ArgumentNullException(nameof(action));
        AddUserCommand(alias, new NestedPCommand(action));
    }

    internal void AddEngineCommand(string alias, PCommand command)
    {
        if (alias == null)
            throw new ArgumentNullException(nameof(alias));
        if (command == null)
            throw new ArgumentNullException(nameof(command));
        addToGroup(alias, PCommandGroups.Engine);
        this[alias] = command;
    }

    internal void AddEngineCommand(string alias, PCommandAction action)
    {
        if (alias == null)
            throw new ArgumentNullException(nameof(alias));
        if (action == null)
            throw new ArgumentNullException(nameof(action));
        AddEngineCommand(alias, new DelegatePCommand(action));
    }

    internal void AddEngineCommand(string alias, ICommand action)
    {
        if (alias == null)
            throw new ArgumentNullException(nameof(alias));
        if (action == null)
            throw new ArgumentNullException(nameof(action));
        AddEngineCommand(alias, new NestedPCommand(action));
    }

    internal void AddCompilerCommand(string alias, PCommand command)
    {
        if (alias == null)
            throw new ArgumentNullException(nameof(alias));
        if (command == null)
            throw new ArgumentNullException(nameof(command));
        addToGroup(alias, PCommandGroups.Compiler);
        this[alias] = command;
    }

    internal void AddCompilerCommand(string alias, PCommandAction action)
    {
        if (alias == null)
            throw new ArgumentNullException(nameof(alias));
        if (action == null)
            throw new ArgumentNullException(nameof(action));
        AddCompilerCommand(alias, new DelegatePCommand(action));
    }

    internal void AddCompilerCommand(string alias, ICommand action)
    {
        if (alias == null)
            throw new ArgumentNullException(nameof(alias));
        if (action == null)
            throw new ArgumentNullException(nameof(action));
        AddCompilerCommand(alias, new NestedPCommand(action));
    }

    /// <summary>
    ///     Adds a new command in the host space.
    /// </summary>
    /// <param name = "alias">The alias that shall refer to the supplied command.</param>
    /// <param name = "command">A command instance.</param>
    /// <exception cref = "ArgumentNullException">If either <paramref name = "alias" /> 
    ///     or <paramref name = "command" /> is null.</exception>
    public void AddHostCommand(string alias, PCommand command)
    {
        if (alias == null)
            throw new ArgumentNullException(nameof(alias));
        if (command == null)
            throw new ArgumentNullException(nameof(command));
        addToGroup(alias, PCommandGroups.Host);
        this[alias] = command;
    }

    /// <summary>
    ///     Adds a new command in the host space.
    /// </summary>
    /// <param name = "alias">The alias that shall refer to the supplied command.</param>
    /// <param name = "action">An action to be turned into a command.</param>
    /// <exception cref = "ArgumentNullException">If either <paramref name = "alias" /> 
    ///     or <paramref name = "action" /> is null.</exception>
    public void AddHostCommand(string alias, PCommandAction action)
    {
        if (alias == null)
            throw new ArgumentNullException(nameof(alias));
        if (action == null)
            throw new ArgumentNullException(nameof(action));
        AddHostCommand(alias, new DelegatePCommand(action));
    }

    /// <summary>
    ///     Adds a new command in the host space.
    /// </summary>
    /// <param name = "alias">The alias that shall refer to the supplied command.</param>
    /// <param name = "action">An action to be turned into a command.</param>
    /// <exception cref = "ArgumentNullException">If either <paramref name = "alias" /> 
    ///     or <paramref name = "action" /> is null.</exception>
    public void AddHostCommand(string alias, ICommand action)
    {
        if (alias == null)
            throw new ArgumentNullException(nameof(alias));
        if (action == null)
            throw new ArgumentNullException(nameof(action));
        AddHostCommand(alias, new NestedPCommand(action));
    }

    /// <summary>
    ///     Removes all command previously added to the user space.
    /// </summary>
    public void RemoveUserCommands()
    {
        _remove_commands(PCommandGroups.User);
    }

    /// <summary>
    ///     Removes all commands previously added to the host space.
    /// </summary>
    public void RemoveHostCommands()
    {
        _remove_commands(PCommandGroups.Host);
    }

    internal void RemoveCompilerCommands()
    {
        _remove_commands(PCommandGroups.Compiler);
    }

    internal void RemoveEngineCommands()
    {
        _remove_commands(PCommandGroups.Engine);
    }

    private void _remove_commands(PCommandGroups groups)
    {
        var commands = new KeyValuePair<string, PCommand>[Count];
        CopyTo(commands, 0);
        foreach (var (alias, _) in commands)
        {
            if(removeFromGroup(alias, groups) is (true, PCommandGroups.None))
                Remove(alias);
        }
    }

    public IEnumerable<KeyValuePair<string, PCommand>> CommandsInGroup(PCommandGroups group)
    {
        return _commandGroups
            .Where(kvp => (kvp.Value & group) != 0)
            .SelectMaybe(kvp => this[kvp.Key] is { } cmd 
                ? (KeyValuePair<string, PCommand>?)new KeyValuePair<string, PCommand>(kvp.Key, cmd)
                : null);
    }
}