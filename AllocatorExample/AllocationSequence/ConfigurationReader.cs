using AllocatorInterface;
using Allocators.AllocationSequence.Actions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Allocators.AllocationSequence
{
    public static class ConfigurationReader
    {
        public static Action<IAllocator, IList<uint>> Configure(IEnumerable<string> actions)
        {
            Action<IAllocator, IList<uint>> result = StaticActions.ActionNull;
            result = actions.Select(ToDelegate).Aggregate(result, Aggregate);
            return result;
        }

        public static Action<IAllocator, IList<uint>> ConfigureFromFile(string file)
        {
            string[] input = File.ReadAllLines(file);
            return Configure(input);
        }

        private static Action<IAllocator, IList<uint>> Aggregate(Action<IAllocator, IList<uint>> sum, Action<IAllocator, IList<uint>> curr)
        {
            if (curr != null)
            {
                sum += curr;
            }
            return sum;
        }

        private static Action<IAllocator, IList<uint>> ToDelegate(string data)
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
                            return action.Action;
                        }
                    }
                    break;
                case "free":
                    if (arguments.Length == 2)
                    {
                        if (int.TryParse(arguments[1], out int index))
                        {
                            var action = new ActionFree(index);
                            return action.Action;
                        }
                    }
                    break;
                case "freeall":
                    return StaticActions.ActionFreeAll;
                case "null":
                    return StaticActions.ActionFreeAll;
                default:
                    break;
            }
            return null;
        }
    }
}
