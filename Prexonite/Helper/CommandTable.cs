// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;

namespace Prexonite.Commands
{
    public class CommandTable : SymbolTable<PCommand>
    {
        private readonly SymbolTable<ICommandInfo> _fallbackCommandInfos =
            new SymbolTable<ICommandInfo>();

        /// <summary>
        ///     Returns information about the specified command's capabilities. The command might not be installed
        ///     in the symbol table.
        /// </summary>
        /// <param name = "id">The id of the command to get information about.</param>
        /// <param name = "commandInfo">Contains the command info on success. Undefined on failure.</param>
        /// <returns>True on success; false on failure.</returns>
        public bool TryGetInfo(string id, out ICommandInfo commandInfo)
        {
            PCommand command;
            if (TryGetValue(id, out command))
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
                throw new ArgumentNullException("id");
            if (commandInfo == null)
                throw new ArgumentNullException("commandInfo");

            _fallbackCommandInfos[id] = commandInfo;
        }

        /// <summary>
        ///     Removes an <see cref = "ICommandInfo" /> from the fallback command info table.
        /// </summary>
        /// <param name = "id">The id of the command to remove fallback info from.</param>
        /// <returns>True if a fallback command info was removed. False otherwise.</returns>
        public bool RemoveFallbackInfo(string id)
        {
            if (id == null)
                throw new ArgumentNullException("id");
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

        public override void Add(string key, PCommand value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            base.Add(key, value);
        }

        /// <summary>
        ///     Index based access to the dictionary of available commands.
        /// </summary>
        /// <param name = "key">The name of the command to retrieve.</param>
        /// <returns>The command registered with the supplied name.</returns>
        public override PCommand this[string key]
        {
            get
            {
                if (ContainsKey(key))
                    return base[key];
                else
                    return null;
            }
            set
            {
                if (value != null && (!value.BelongsToAGroup))
                    value.AddToGroup(PCommandGroups.User);

                if (ContainsKey(key))
                {
                    if (value == null)
                        Remove(key);
                    else
                        base[key] = value;
                }
                else if (value == null)
                    throw new ArgumentNullException("value");
                else
                    Add(key, value);
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
                throw new ArgumentNullException("alias");
            if (command == null)
                throw new ArgumentNullException("command");
            command.AddToGroup(PCommandGroups.User);
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
                throw new ArgumentNullException("alias");
            if (action == null)
                throw new ArgumentNullException("action");
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
                throw new ArgumentNullException("alias");
            if (action == null)
                throw new ArgumentNullException("action");
            AddUserCommand(alias, new NestedPCommand(action));
        }

        internal void AddEngineCommand(string alias, PCommand command)
        {
            if (alias == null)
                throw new ArgumentNullException("alias");
            if (command == null)
                throw new ArgumentNullException("command");
            command.AddToGroup(PCommandGroups.Engine);
            this[alias] = command;
        }

        internal void AddEngineCommand(string alias, PCommandAction action)
        {
            if (alias == null)
                throw new ArgumentNullException("alias");
            if (action == null)
                throw new ArgumentNullException("action");
            AddEngineCommand(alias, new DelegatePCommand(action));
        }

        internal void AddEngineCommand(string alias, ICommand action)
        {
            if (alias == null)
                throw new ArgumentNullException("alias");
            if (action == null)
                throw new ArgumentNullException("action");
            AddEngineCommand(alias, new NestedPCommand(action));
        }

        internal void AddCompilerCommand(string alias, PCommand command)
        {
            if (alias == null)
                throw new ArgumentNullException("alias");
            if (command == null)
                throw new ArgumentNullException("command");
            command.AddToGroup(PCommandGroups.Compiler);
            this[alias] = command;
        }

        internal void AddCompilerCommand(string alias, PCommandAction action)
        {
            if (alias == null)
                throw new ArgumentNullException("alias");
            if (action == null)
                throw new ArgumentNullException("action");
            AddCompilerCommand(alias, new DelegatePCommand(action));
        }

        internal void AddCompilerCommand(string alias, ICommand action)
        {
            if (alias == null)
                throw new ArgumentNullException("alias");
            if (action == null)
                throw new ArgumentNullException("action");
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
                throw new ArgumentNullException("alias");
            if (command == null)
                throw new ArgumentNullException("command");
            command.AddToGroup(PCommandGroups.Host);
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
                throw new ArgumentNullException("alias");
            if (action == null)
                throw new ArgumentNullException("action");
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
                throw new ArgumentNullException("alias");
            if (action == null)
                throw new ArgumentNullException("action");
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
            var commands =
                new KeyValuePair<string, PCommand>[Count];
            CopyTo(commands, 0);
            foreach (var kvp in commands)
            {
                var cmd = kvp.Value;
                cmd.RemoveFromGroup(groups);
                if (!cmd.BelongsToAGroup)
                    Remove(kvp.Key);
            }
        }
    }
}