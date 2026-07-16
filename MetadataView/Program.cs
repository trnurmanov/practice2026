using System;
using System.IO;
using System.Reflection;
using task07;

namespace task09
{
    [DisplayName("Тест")]
    [Version(2, 6)]
    public class SampleTestClass
    {
        public string Data { get; set; }

        public SampleTestClass(string initialData, int count)
        {
            Data = initialData;
        }

        [DisplayName("Метод обработки файлов")]
        public bool ProcessFile(string path, int mode)
        {
            return true;
        }
    }

    class Program
    {
        static void PrintParameters(ParameterInfo[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                Console.Write($"{parameters[i].ParameterType.Name} {parameters[i].Name}");
                if (i < parameters.Length - 1)
                    Console.Write(", ");
            }
        }

        static void Main(string[] args)
        {
            string dllPath = args.Length > 0 ? args[0] : Assembly.GetExecutingAssembly().Location;

            if (!File.Exists(dllPath))
            {
                Console.WriteLine($"Ошибка: файл не найден по пути {dllPath}");
                return;
            }

            try
            {
                Assembly assembly = Assembly.LoadFrom(dllPath);
                Type[] types = assembly.GetTypes();

                foreach (Type type in types)
                {
                    if (!type.IsClass || type.IsAbstract)
                        continue;

                    Console.WriteLine($"\nКласс: {type.FullName}");

                    var classAttributes = type.GetCustomAttributes();
                    bool hasAttributes = false;
                    foreach (var attr in classAttributes)
                    {
                        if (attr.GetType().Name.StartsWith("Nullable"))
                            continue;

                        if (!hasAttributes)
                        {
                            Console.WriteLine("  Атрибуты класса:");
                            hasAttributes = true;
                        }

                        if (attr is DisplayNameAttribute dna)
                            Console.WriteLine($"    DisplayName: \"{dna.DisplayName}\"");
                        else if (attr is VersionAttribute va)
                            Console.WriteLine($"    Version: {va.Major}.{va.Minor}");
                        else
                            Console.WriteLine($"    {attr.GetType().Name}");
                    }

                    ConstructorInfo[] constructors = type.GetConstructors();
                    if (constructors.Length > 0)
                    {
                        Console.WriteLine("  Конструкторы:");
                        foreach (var ctor in constructors)
                        {
                            Console.Write($"    - {type.Name}(");
                            ParameterInfo[] parameters = ctor.GetParameters();
                            PrintParameters(parameters);
                            Console.WriteLine(")");
                        }
                    }

                    MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                    bool hasMethods = false;
                    foreach (var method in methods)
                    {
                        if (method.IsSpecialName)
                            continue;

                        if (!hasMethods)
                        {
                            Console.WriteLine("  Методы:");
                            hasMethods = true;
                        }

                        Console.Write($"    - {method.ReturnType.Name} {method.Name}(");
                        ParameterInfo[] parameters = method.GetParameters();
                        PrintParameters(parameters);
                        Console.WriteLine(")");

                        var methodAttrs = method.GetCustomAttributes();
                        foreach (var attr in methodAttrs)
                        {
                            if (attr is DisplayNameAttribute dna)
                                Console.WriteLine($"      DisplayName: \"{dna.DisplayName}\"");
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.WriteLine($"Ошибка загрузки типов: {ex.Message}");
                if (ex.LoaderExceptions != null)
                {
                    foreach (var loaderEx in ex.LoaderExceptions)
                    {
                        if (loaderEx != null)
                            Console.WriteLine($"  - {loaderEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }
}
