using System;
using System.Collections.Generic;
using System.Text;
using Prexonite.Commands;

namespace Prexonite.Commands
{
    public class CommandTable : SymbolTable<PCommand>
    {

        /// <summary>
        /// Determines whether a particular name is registered for a command.
        /// </summary>
        /// <param name="name">A name.</param>
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
        /// Index based access to the dictionary of available commands.
        /// </summary>
        /// <param name="name">The name of the command to retrieve.</param>
        /// <returns>The command registered with the supplied name.</returns>
        public override PCommand this[string name]
        {
            get
            {
                if (ContainsKey(name))
                    return base[name];
                else
                    return null;
            }
            set
            {
                if (value != null && (!value.BelongsToAGroup))
                    value.AddToGroup(PCommandGroups.User);

                if (ContainsKey(name))
                {
                    if (value == null)
                        Remove(name);
                    else
                        base[name] = value;
                }
                else if (value == null)
                    throw new ArgumentNullException("value");
                else
                    Add(name, value);
            }
        }

        /// <summary>
        /// Adds a new command in the user space.
        /// </summary>
        /// <param name="alias">The alias that shall refer to the supplied command.</param>
        /// <param name="command">A command instance.</param>
        /// <exception cref="ArgumentNullException">If either <paramref name="alias"/> 
        /// or <paramref name="command"/> is null.</exception>
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
        /// Adds a new command in the user space.
        /// </summary>
        /// <param name="alias">The alias that shall refer to the supplied command.</param>
        /// <param name="action">An action to be turned into a command.</param>
        /// <exception cref="ArgumentNullException">If either <paramref name="alias"/> 
        /// or <paramref name="action"/> is null.</exception>
        public void AddUserCommand(string alias, PCommandAction action)
        {
            if (alias == null)
                throw new ArgumentNullException("alias");
            if (action == null)
                throw new ArgumentNullException("action");
            AddUserCommand(alias, new DelegatePCommand(action));
        }

        /// <summary>
        /// Adds a new command in the user space.
        /// </summary>
        /// <param name="alias">The alias that shall refer to the supplied command.</param>
        /// <param name="action">An action to be turned into a command.</param>
        /// <exception cref="ArgumentNullException">If either <paramref name="alias"/> 
        /// or <paramref name="action"/> is null.</exception>
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
        /// Adds a new command in the host space.
        /// </summary>
        /// <param name="alias">The alias that shall refer to the supplied command.</param>
        /// <param name="command">A command instance.</param>
        /// <exception cref="ArgumentNullException">If either <paramref name="alias"/> 
        /// or <paramref name="command"/> is null.</exception>
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
        /// Adds a new command in the host space.
        /// </summary>
        /// <param name="alias">The alias that shall refer to the supplied command.</param>
        /// <param name="action">An action to be turned into a command.</param>
        /// <exception cref="ArgumentNullException">If either <paramref name="alias"/> 
        /// or <paramref name="action"/> is null.</exception>
        public void AddHostCommand(string alias, PCommandAction action)
        {
            if (alias == null)
                throw new ArgumentNullException("alias");
            if (action == null)
                throw new ArgumentNullException("action");
            AddHostCommand(alias, new DelegatePCommand(action));
        }

        /// <summary>
        /// Adds a new command in the host space.
        /// </summary>
        /// <param name="alias">The alias that shall refer to the supplied command.</param>
        /// <param name="action">An action to be turned into a command.</param>
        /// <exception cref="ArgumentNullException">If either <paramref name="alias"/> 
        /// or <paramref name="action"/> is null.</exception>
        public void AddHostCommand(string alias, ICommand action)
        {
            if (alias == null)
                throw new ArgumentNullException("alias");
            if (action == null)
                throw new ArgumentNullException("action");
            AddHostCommand(alias, new NestedPCommand(action));
        }

        /// <summary>
        /// Removes all command previously added to the user space.
        /// </summary>
        public void RemoveUserCommands()
        {
            _remove_commands(PCommandGroups.User);
        }

        /// <summary>
        /// Removes all commands previously added to the host space.
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
            KeyValuePair<string, PCommand>[] commands =
                new KeyValuePair<string, PCommand>[Count];
            CopyTo(commands, 0);
            foreach (KeyValuePair<string, PCommand> kvp in commands)
            {
                PCommand cmd = kvp.Value;
                cmd.RemoveFromGroup(groups);
                if (!cmd.BelongsToAGroup)
                    Remove(kvp.Key);
            }
        }

    }
}
