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

using System.Diagnostics;
using JetBrains.Annotations;
using Prexonite.Commands.Core;
#if Verbose

#endif

namespace Prexonite
{
    /// <summary>
    ///     Prexonite virtual machines. Engines manage available <see cref = "PType">PTypes</see>, assemblies accessible by the virtual machine as well as available <see
    ///      cref = "Commands" />.
    /// </summary>
    public partial class Engine
    {
        #region Stack management

        //private readonly LinkedList<StackContext> _stack = new LinkedList<StackContext>();

        /// <summary>
        ///     Provides access to the virtual machine's call stack.
        /// </summary>
        public LinkedList<StackContext> Stack
        {
            [DebuggerStepThrough]
            get
            {
                if (Thread.GetData(_stackSlot) is not LinkedList<StackContext> stack)
                    Thread.SetData(_stackSlot, stack = new());
                return stack;
            }
        }

        #endregion

        #region Processor

        /// <summary>
        ///     Executes the context on top of the stack.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This function executes the context on top of the stack until that context gets popped, 
        ///         regardless of any other contexts that have been pushed by the code.
        ///     </para>
        ///     <para>
        ///         If the stack is empty, this method returns immediately.
        ///     </para>
        /// </remarks>
        /// <exception cref = "ExecutionProhibitedException">The engine does is not permitted to execute code.</exception>
        /// <seealso cref = "ExecutionProhibited" />
        /// <seealso cref = "StackContext" />
        public PValue Process()
        {
            if (ExecutionProhibited)
                throw new ExecutionProhibitedException("The engine is not permitted to run code.");

            var localStack = Stack;
            var level = localStack.Count;
            if (level < 1)
                throw new PrexoniteException(
                    "The VM stack is empty. Return value cannot be computed.");

            PrexoniteRuntimeException? currentException = null;
            StackContext? lastContext = null;

            while (localStack.Count >= level)
            {
                var sctx = localStack.Last!.Value;

                var keepOnStack = false;
                if (currentException == null)
                {
                    //Execute code
                    try
                    {
                        keepOnStack = sctx._NextCycle(lastContext);
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
                    Console.WriteLine("#POP: " + Stack.Last.Value + "=" + PValue.ToDebugString(Stack.Last.Value.ReturnValue));
#endif
                    if (ReferenceEquals(localStack.Last.Value, sctx))
                        localStack.RemoveLast();
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
        ///     Executes a give stack context.
        /// </summary>
        /// <param name = "sctx">Any stack context.</param>
        /// <exception cref = "ArgumentNullException"><paramref name = "sctx" /> is null.</exception>
        public PValue Process(StackContext sctx)
        {
            if (sctx == null)
                throw new ArgumentNullException(nameof(sctx));
            Stack.AddLast(sctx);
#if Verbose
                    Console.WriteLine("\n#PSH: " + sctx + "(?)");
#endif
            sctx.ReturnMode = ReturnMode.Exit;
            return Process();
        }

        #endregion

        #region Runtime Settings

        /// <summary>
        ///     Controls whether function references are cached to skip the string comparison based lookup.
        /// </summary>
        /// <remarks>
        ///     If you enabled caching and replace a function at runtime, instructions that 
        ///     maintain a cached reference will still call the original function.
        /// </remarks>
        [PublicAPI]
        public bool CacheFunctions { get; set; }

        /// <summary>
        ///     Controls whether command references are cached to skip the string comparison based lookup.
        /// </summary>
        /// <remarks>
        ///     If you enabled caching and replace a commanc at runtime, instructions that 
        ///     maintain a cached reference will still call the original command.
        /// </remarks>
        [PublicAPI]
        public bool CacheCommands { get; set; }

        /// <summary>
        ///     You can prevent an engine from accidentially executing code by setting this property to true.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Only calls to <see cref = "Process()" /> are blocked. 
        ///         More specifically they will result in a <see cref = "ExecutionProhibitedException" /> if called.
        ///     </para>
        ///     <para>
        ///         The engine can still be used to dispatch calls in the type system.
        ///     </para>
        ///     <para>
        ///         This can be usefull if you want to prevent a script from executing it's build block upon loading.<br />
        ///     </para>
        /// </remarks>
        public bool ExecutionProhibited { get; set; }

        /// <summary>
        ///     Indicates whether the <see cref = "CompileToCil" /> command is allowed to link statically. 
        ///     This setting overrides any arguments passed to CompileToCil.
        /// </summary>
        [PublicAPI]
        public bool StaticLinkingAllowed { get; set; }

        #endregion
    }
}