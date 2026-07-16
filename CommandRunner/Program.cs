using CommandLib;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace CommandRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            const string dllName = "FileSystemCommands.dll";
            string dllPath = LocateLibrary(dllName);

            if (string.IsNullOrEmpty(dllPath))
            {
                Console.WriteLine($"Не удалось найти библиотеку: {dllName}");
                return;
            }

            try
            {
                RunAllCommands(dllPath);
            }
            catch (Exception error)
            {
                Console.WriteLine($"Ошибка выполнения: {error.Message}");
            }
        }

        static string LocateLibrary(string fileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string directPath = Path.Combine(baseDirectory, fileName);
            if (File.Exists(directPath))
                return directPath;

            string relativePath = Path.Combine(baseDirectory, "..", "..", "..", "..",
                "FileSystemCommands", "bin", "Debug", "net10.0", fileName);
            if (File.Exists(relativePath))
                return relativePath;

            return null;
        }

        static void RunAllCommands(string libraryPath)
        {
            Assembly loadedAssembly = Assembly.LoadFrom(libraryPath);
            string testDirectory = BuildTestDirectory();
            PopulateTestFiles(testDirectory);

            var availableCommands = FindCommands(loadedAssembly);
            Console.WriteLine($"Обнаружено команд: {availableCommands.Count}");

            foreach (var commandType in availableCommands)
            {
                ExecuteCommand(commandType, testDirectory);
            }

            CleanupTestDirectory(testDirectory);
        }

        static string BuildTestDirectory()
        {
            string directory = Path.Combine(Path.GetTempPath(), $"RunnerTestDir_{Guid.NewGuid()}");
            Directory.CreateDirectory(directory);
            Console.WriteLine($"Тестовая директория: {directory}");
            return directory;
        }

        static void PopulateTestFiles(string directory)
        {
            File.WriteAllText(Path.Combine(directory, "document.txt"), "sample content");
            File.WriteAllText(Path.Combine(directory, "text.txt"), "text file content");
            Console.WriteLine("Созданы файлы: document.txt, text.txt\n");
        }

        static List<Type> FindCommands(Assembly assembly)
        {
            return assembly.GetTypes()
                .Where(type => typeof(CommandLib.ICommand).IsAssignableFrom(type)
                    && !type.IsInterface
                    && !type.IsAbstract)
                .ToList();
        }

        static void ExecuteCommand(Type commandType, string workingDirectory)
        {
            Console.WriteLine(commandType.Name);
            CommandLib.ICommand commandInstance = CreateCommand(commandType, workingDirectory);

            if (commandInstance is not null)
            {
                ExecuteAndDisplay(commandInstance);
            }

            Console.WriteLine();
        }

        static CommandLib.ICommand CreateCommand(Type commandType, string directory)
        {
            try
            {
                if (commandType.Name == "DirectorySizeCommand")
                {
                    var instance = (CommandLib.ICommand)Activator.CreateInstance(commandType, new object[] { directory });
                    Console.WriteLine($"Создан экземпляр DirectorySizeCommand с директорией: {directory}");
                    return instance;
                }

                if (commandType.Name == "FindFilesCommand")
                {
                    var instance = (CommandLib.ICommand)Activator.CreateInstance(commandType, new object[] { directory, "*.txt" });
                    Console.WriteLine($"Создан экземпляр FindFilesCommand с директорией: {directory} и маской: *.txt");
                    return instance;
                }

                return (CommandLib.ICommand)Activator.CreateInstance(commandType);
            }
            catch (Exception error)
            {
                Console.WriteLine($"Ошибка создания экземпляра: {error.Message}");
                return null;
            }
        }

        static void ExecuteAndDisplay(CommandLib.ICommand command)
        {
            try
            {
                Console.WriteLine("Выполнение команды:");
                command.Execute();
                Console.WriteLine("Команда успешно выполнена.");
            }
            catch (Exception error)
            {
                Console.WriteLine($"Ошибка выполнения: {error.Message}");
            }
        }

        static void CleanupTestDirectory(string directory)
        {
            try
            {
                Directory.Delete(directory, true);
                Console.WriteLine($"Директория удалена: {directory}");
            }
            catch (Exception error)
            {
                Console.WriteLine($"Ошибка удаления: {error.Message}");
            }
        }
    }
}