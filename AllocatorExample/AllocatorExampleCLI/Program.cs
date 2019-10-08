using AllocatorInterface;
using MemoryModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AllocatorExampleCLI
{
    class Program
    {
        const string mainMenu = "\n==MAIN MENU==\n1. Create Allocator\n2. List all builders\n3. Load assembly\n4. Load all from directory and subdirectories\n5. Exit\n";

        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine(mainMenu);
                string input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        CreateAllocator();
                        break;
                    case "2":
                        OutputAllBuilders();
                        break;
                    case "3":
                        LoadAssembly();
                        break;
                    case "4":
                        LoadAll();
                        break;
                    case "5":
                        return;
                    default:
                        break;
                }
            }
        }

        static void OutputAllBuilders()
        {
            Console.WriteLine();
            var builders = GetBuilderList();
            if (builders.Count() == 0)
            {
                Console.WriteLine("Please load any valid classes");
            }
            else
            {
                Console.WriteLine("==Valid classes==");
            }
            foreach(var builder in builders)
            {
                Console.WriteLine(builder.FullName);
            }
            Console.WriteLine();
        }
        
        static IEnumerable<Type> GetBuilderList()
        {
            var type = typeof(IAllocatorBuilder);
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => type.IsAssignableFrom(p) && p.IsClass);
        }

        static void LoadAssembly()
        {
            Console.WriteLine("\nPlease enter file path");
            string file = Console.ReadLine();
            try
            {
                Assembly assembly = Assembly.LoadFile(file);
            }
            catch
            {
                Console.WriteLine("Incorrect path");
            }
        }

        static void LoadAll()
        {
            Console.WriteLine("\nPlease enter root directory");
            string dir = Console.ReadLine();
            LoadFiles(dir);
        }
        static void LoadFiles(string root)
        {
            var subDir = Directory.EnumerateDirectories(root);
            foreach(var dir in subDir)
            {
                var dirIndo = new DirectoryInfo(dir);
                if ((dirIndo.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    LoadFiles(dir);
                }
            }
            var files = Directory.EnumerateFiles(root);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.Extension == ".dll")
                {
                    try
                    {
                        Assembly assembly = Assembly.LoadFile(file);
                    }
                    catch
                    {
                        Console.WriteLine("Incorrect path");
                    }
                }
            }
        }

        static void CreateAllocator()
        {
            var builders = GetBuilderList();
            Console.WriteLine("\n==Select builder==");
            int i = 0;
            foreach (var builder in builders)
            {
                i++;
                Console.WriteLine(i+". " + builder.FullName);
            }
            string input = Console.ReadLine();
            bool isValid = int.TryParse(input, out int value);
            if (isValid)
            {
                i = 0;
                foreach (var builder in builders)
                {
                    i++;
                    if (i == value)
                    {
                        IAllocatorBuilder selectedBuilder = (IAllocatorBuilder)Activator.CreateInstance(builder);
                        RunExample(selectedBuilder);
                    }
                }
            }
        }

        static void RunExample(IAllocatorBuilder builder)
        {
            bool isValid = false;
            uint value = 0;
            while (!isValid)
            {
                Console.WriteLine("\nInput memory size");
                string input = Console.ReadLine();
                isValid = uint.TryParse(input, out value);
            }

            Memory memory = new Memory(value);
            builder.SetMemory(memory);
            IAllocator allocator = builder.Build();
            IAllocatorAnalizer analizer = builder.BuildAnalizer();

            // TODO
            string menu = "\n==Select action==\n1. Allocate memory\n2. Free memory\n3. List allocated adresses\n4. Show memory status\n5. Exit";
            List<uint> allocated = new List<uint>();
            while (true)
            {
                Console.WriteLine(menu);
                string input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        Console.WriteLine("\nInput allocation size");
                        input = Console.ReadLine();
                        isValid = uint.TryParse(input, out value);
                        if (isValid)
                        {
                            uint address = allocator.Alloc(value);
                            if (address != allocator.Null)
                            {
                                allocated.Add(address);
                                Console.WriteLine("Allocated with adress: " + address);
                            }
                            else
                            {
                                Console.WriteLine("Cannot allocate");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Incorrect number");
                        }
                        break;
                    case "2":
                        Console.WriteLine("\nSelect address from list");
                        for(int i = 0; i < allocated.Count; i++)
                        {
                            Console.WriteLine(i+". "+ allocated[i]);
                        }
                        input = Console.ReadLine();
                        isValid = uint.TryParse(input, out value);
                        if (isValid)
                        {
                            if (value < allocated.Count)
                            {
                                uint address = allocated[(int)value];
                                allocator.Free(address);
                                allocated.RemoveAt((int)value);
                                Console.WriteLine("Block free");
                            }
                            else
                            {
                                Console.WriteLine("Incorrect index");
                            }
                        }
                        break;
                    case "3":
                        Console.WriteLine("\n==Allocated addresses==");
                        for (int i = 0; i < allocated.Count; i++)
                        {
                            Console.WriteLine(i + ". " + allocated[i]);
                        }
                        break;
                    case "4":
                        var status = analizer.AnalizeMemory();
                        Console.WriteLine("==Possible status list==");
                        var names = Enum.GetNames(typeof(MemoryAnalizerStatus)).ToArray();
                        var values = Enum.GetValues(typeof(MemoryAnalizerStatus)).Cast<int>().ToArray();
                        for (int i = 0; i < names.Length; i++)
                        {
                            Console.WriteLine(values[i]+ " - " + names[i]);
                        }
                        Console.WriteLine("==Memory Status==");
                        for (int i = 0; i < status.Length; i++)
                        {
                            if (i % 64 == 0)
                            {
                                Console.WriteLine();
                            }
                            Console.Write((byte)status[i]);
                        }
                        break;
                    case "5":
                        return;
                    default:
                        break;
                }
            }
        }
    }
}
