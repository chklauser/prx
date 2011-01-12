using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Prexonite.Commands.Core;
using Prexonite.Commands.List;
using Prexonite.Compiler.Cil;
using Prexonite.Concurrency;
using Prexonite.Types;

namespace Prexonite.Commands.Concurrency
{
    public class Select : PCommand, ICilCompilerAware
    {
        #region Singleton pattern

        private Select()
        {
        }

        private static readonly Select _instance = new Select();

        public static Select Instance
        {
            get { return _instance; }
        }

        #endregion

        #region Overrides of PCommand

        [Obsolete]
        public override bool IsPure
        {
            get { return false; }
        }

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            bool performSubCall;
            if (args.Length > 0 && args[0].Type.ToBuiltIn() == PType.BuiltIn.Bool)
                performSubCall = (bool)args[0].Value;
            else
                performSubCall = false;

            var rawCases = new List<PValue>();
            foreach (var arg in args.Skip(performSubCall ? 1 : 0))
            {
                var set = Map._ToEnumerable(sctx, arg);
                if (set == null)
                    continue;
                else
                    rawCases.AddRange(set);
            }

            var appCases = _extract(rawCases.Where(c => isApplicable(sctx, c))).ToArray();

            return RunStatically(sctx, appCases, performSubCall);
        }

        public static PValue RunStatically(StackContext sctx, KeyValuePair<Channel, PValue>[] appCases, bool performSubCall)
        {
            //Check if there data is already available (i.e. if the select can be processed non-blocking)
            foreach (var kvp in appCases)
            {
                var chan = kvp.Key;
                var handler = kvp.Value;

                if (chan != null)
                {
                    PValue datum;
                    if (chan.TryReceive(out datum))
                    {
                        return _invokeHandler(sctx, handler, datum, performSubCall);
                    }
                }
                else
                {
                    return _invokeHandler(sctx, handler, null, performSubCall);
                }
            }

            //We have to wait for one of the channels to become active (there are no default handlers)
            Channel[] channels;
            PValue[] handlers;
            _split(appCases, out channels, out handlers);
            var flags = channels.Select(c => c.DataAvailable).ToArray();

            while (true)
            {
                var selected = WaitHandle.WaitAny(flags);
                PValue datum;
                if (channels[selected].TryReceive(out datum))
                {
                    return _invokeHandler(sctx, handlers[selected], datum, performSubCall);
                }
                //else: someone ninja'd the damn thing before we could get to it, continue waiting
            }
        }

        private static PValue _invokeHandler(StackContext sctx, PValue handler, PValue datum, bool performSubCall)
        {
            var handlerArgv = datum != null ? new[] { datum } : Runtime.EmptyPValueArray;
            return performSubCall 
                ? CallSubPerform.RunStatically(sctx, handler, handlerArgv, useIndirectCallAsFallback: true) 
                : handler.IndirectCall(sctx, handlerArgv);
        }

        private static readonly PType _chanType = PType.Object[typeof (Channel)];

        private static bool isApplicable(StackContext sctx, PValue selectCase)
        {
            if (selectCase.Type == PValueKeyValuePair.ObjectType)
            {
                var key = ((PValueKeyValuePair) selectCase.Value).Key;
                if (key.Type == _chanType)
                    return true;
                else if (key.Type.ToBuiltIn() == PType.BuiltIn.Bool)
                    return (bool) key.Value;
                else if (key.Value == null)
                    return false;
                else
                    return Runtime.ExtractBool(key.IndirectCall(sctx, Runtime.EmptyPValueArray), sctx);
            }
            else
            {
                return true;
            }
        }

        private static IEnumerable<KeyValuePair<Channel, PValue>> _extract(IEnumerable<PValue> cases)
        {
            foreach (var c in cases)
            {
                if (c.Type == PValueKeyValuePair.ObjectType)
                {
                    var kvp = ((PValueKeyValuePair) c.Value);
                    
                    if( kvp.Value.Type == PValueKeyValuePair.ObjectType)
                    {
                        kvp = (PValueKeyValuePair) kvp.Value.Value;
                    }

                    var key = kvp.Key;

                    if (key.Type == _chanType)
                        yield return new KeyValuePair<Channel, PValue>((Channel) kvp.Key.Value, kvp.Value);
                    else
                        throw new PrexoniteException(
                            "Invalid select clause. Syntax: select( [channel:handler] ) or select( [cond:channel:handler] ). Offending value " + c.Value);
                }
                else if (c.Type == _chanType)
                {
                    throw new PrexoniteException(
                        "Missing handler in select clause. Syntax: select( [channel: handler] ) or select( [cond:channel:handler] )");
                }
                else
                {
                    //A default handler or a handler that just doesn't have input (but possibly a condition)
                    yield return new KeyValuePair<Channel, PValue>(null, c);
                }
            }
        }

        private static void _split(IEnumerable<KeyValuePair<Channel, PValue>> cases, out Channel[] channels, out PValue[] handlers)
        {
            var chanCases = cases.Where(kvp => kvp.Key != null).ToArray();
            var count = chanCases.Length;
            channels = new Channel[count];
            handlers = new PValue[count];
            for (var i = 0; i < chanCases.Length; i++)
            {
                channels[i] = chanCases[i].Key;
                handlers[i] = chanCases[i].Value;
            }
        }

        #endregion

        #region Implementation of ICilCompilerAware

        public CompilationFlags CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersRunStatically;
        }

        public void ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}