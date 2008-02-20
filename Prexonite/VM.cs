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
using System.Collections.Generic;
using System.Diagnostics;
using Prexonite.Types;
#if Verbose

#endif

namespace Prexonite
{
    /// <summary>
    /// Prexonite virtual machines. Engines manage available <see cref="PType">PTypes</see>, assemblies accessible by the virtual machine as well as available <see cref="Commands" />.
    /// </summary>
    public partial class Engine
    {
        #region Stack management

        private LinkedList<StackContext> _stack = new LinkedList<StackContext>();

        /// <summary>
        /// Provides access to the virtual machine's call stack.
        /// </summary>
        public LinkedList<StackContext> Stack
        {
            [DebuggerStepThrough]
            get { return _stack; }
        }

        #endregion

        #region Processor

        /// <summary>
        /// Executes the context on top of the stack.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     This function executes the context on top of the stack until that context gets popped, 
        ///     regardless of any other contexts that have been pushed by the code.
        /// </para>
        /// <para>
        ///     If the stack is empty, this method returns immediately.
        /// </para></remarks>
        /// <exception cref="ExecutionProhibitedException">The engine does is not permitted to execute code.</exception>
        /// <seealso cref="ExecutionProhibited"/>
        /// <seealso cref="StackContext"/>
        public PValue Process()
        {
            if (_executionProhibited)
                throw new ExecutionProhibitedException("The engine is not permitted to run code.");

            int level = _stack.Count;
            if (level < 1)
                throw new PrexoniteException("The VM stack is empty. Return value cannot be computed.") ;

            PrexoniteRuntimeException currentException = null;
            StackContext lastContext = null;

            while (_stack.Count >= level)
            {
                StackContext sctx = _stack.Last.Value;

                bool keepOnStack = false;
                if (currentException == null)
                {
                    //Execute code
                    try
                    {
                        keepOnStack = sctx.NextCylce(lastContext);
                        lastContext = sctx;
                    }
                    catch (PrexoniteRuntimeException exc)
                    {
                        currentException = PrexoniteRuntimeException.UnpackException(exc);
                        continue;
                    }
                    catch (Exception exc)
                    {
                        currentException = PrexoniteRuntimeException.UnpackException(
                            PrexoniteRuntimeException.CreateRuntimeException(sctx, exc));
                        continue;
                    }
                }
                else //Handle exception
                {
                    try
                    {
                        keepOnStack = sctx.TryHandleException(currentException);
                        if (keepOnStack) //Exception has been handled
                            currentException = null;
                    }
                    catch (PrexoniteRuntimeException exc)
                    {
                        currentException = PrexoniteRuntimeException.UnpackException(exc);
                    }
                    catch (Exception exc)
                    {
                        //Original exception can no longer be handled
                        currentException =
                            PrexoniteRuntimeException.UnpackException(
                                PrexoniteRuntimeException.CreateRuntimeException(sctx, exc));
                        continue;
                    }
                }

                if (!keepOnStack)
                {
#if Verbose
                    Console.WriteLine("#POP: " + _stack.Last.Value + "=" + FunctionContext.toDebug(_stack.Last.Value.ReturnValue));
#endif
                    if (ReferenceEquals(_stack.Last.Value, sctx))
                        _stack.RemoveLast();
                    //else the context has already been removed.
                }
            }

            if (currentException != null)
                throw currentException;

            if (lastContext == null)
                throw new PrexoniteException("Cannot identify last stack context.");
            else
                return lastContext.ReturnValue ?? PType.Null;
        }

        /// <summary>
        /// Executes a give stack context.
        /// </summary>
        /// <param name="sctx">Any stack context.</param>
        /// <exception cref="ArgumentNullException"><paramref name="sctx"/> is null.</exception>
        public PValue Process(StackContext sctx)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            _stack.AddLast(sctx);
#if Verbose
                    Console.WriteLine("\n#PSH: " + sctx + "(?)");
#endif
            sctx.ReturnMode = ReturnModes.Exit;
            return Process();
        }

        #endregion

        #region Runtime Settings

        private bool _cacheFunctions = false;

        /// <summary>
        /// Controls whether function references are cached to skip the string comparison based lookup.
        /// </summary>
        /// <remarks>If you enabled caching and replace a function at runtime, instructions that 
        /// maintain a cached reference will still call the original function.</remarks>
        public bool CacheFunctions
        {
            get { return _cacheFunctions; }
            set { _cacheFunctions = value; }
        }

        private bool _cacheCommands = false;

        /// <summary>
        /// Controls wether command references are cached to skip the string comparison based lookup.
        /// </summary>
        /// <remarks>If you enabled caching and replace a commanc at runtime, instructions that 
        /// maintain a cached reference will still call the original command.</remarks>
        public bool CacheCommands
        {
            get { return _cacheCommands; }
            set { _cacheCommands = value; }
        }

        private bool _executionProhibited = false;

        /// <summary>
        /// You can prevent an engine from accidentially executing code by setting this property to true.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     Only calls to <see cref="Process()"/> are blocked. 
        ///     More specifically they will result in a <see cref="ExecutionProhibitedException"/> if called.
        /// </para>
        /// <para>
        ///     The engine can still be used to dispatch calls in the type system.
        /// </para>
        /// <para>
        ///     This can be usefull if you want to prevent a script from executing it's build block upon loading.<br />
        /// </para>
        /// </remarks>
        public bool ExecutionProhibited
        {
            get { return _executionProhibited; }
            set { _executionProhibited = value; }
        }

        #endregion
    }
}