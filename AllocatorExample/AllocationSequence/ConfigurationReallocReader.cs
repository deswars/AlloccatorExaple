using AllocatorInterface;
using Allocators.AllocationSequence.Actions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Allocators.AllocationSequence
{
    public static class ConfigurationReallocReader
    {
        public static Action<IAllocatorReallocable, IList<uint>> Configure(IEnumerable<string> actions)
        {
            Action<IAllocatorReallocable, IList<uint>> result = StaticActions.ActionReallocableNull;
            result = actions.Select(ToDelegate).Aggregate(result, AggregateActions);
            return result;
        }

        public static Action<IAllocatorReallocable, IList<uint>> ConfigureFromFile(string file)
        {
            string[] input = File.ReadAllLines(file);
            return Configure(input);
        }

        private static Action<IAllocatorReallocable, IList<uint>> AggregateActions(Action<IAllocatorReallocable, IList<uint>> sum, Action<IAllocatorReallocable, IList<uint>> curr)
        {
            if (curr != null)
            {
                sum += curr;
            }
            return sum;
        }

        private static Action<IAllocatorReallocable, IList<uint>> ToDelegate(string data)
        {
            var arguments = data.Split(" ");
            switch (arguments[0])
            {
                case "alloc":
                    if (arguments.Length == 2)
                    {
                        if (uint.TryParse(arguments[1], out uint size))
                        {
                            var action = new ActionAlloc(size);
                            return action.ActionReallocable;
                        }
                    }
                    break;
                case "free":
                    if (arguments.Length == 2)
                    {
                        if (int.TryParse(arguments[1], out int index))
                        {
                            var action = new ActionFree(index);
                            return action.ActionReallocable;
                        }
                    }
                    break;
                case "realloc":
                    if (arguments.Length == 3)
                    {
                        if (int.TryParse(arguments[1], out int index))
                        {
                            if (uint.TryParse(arguments[2], out uint size))
                            {
                                var action = new ActionRealloc(index, size);
                                return action.ActionReallocable;
                            }
                        }
                    }
                    break;
                case "freeall":
                    return StaticActions.ActionReallocableFreeAll;
                case "null":
                    return StaticActions.ActionReallocableNull;
                default:
                    break;
            }
            return null;
        }
    }
}
