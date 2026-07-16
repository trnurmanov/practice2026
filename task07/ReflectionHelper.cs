using System;
using System.Reflection;

namespace task07
{
    public static class ReflectionHelper
    {
        public static void PrintTypeInfo(Type targetType)
        {
            if (targetType is null)
                throw new ArgumentNullException(nameof(targetType), "Тип не может быть null");

            var displayNameAttr = targetType.GetCustomAttribute<DisplayNameAttribute>();
            if (displayNameAttr is not null)
                Console.WriteLine($"Отображаемое имя класса: {displayNameAttr.DisplayName}");

            var versionAttr = targetType.GetCustomAttribute<VersionAttribute>();
            if (versionAttr is not null)
                Console.WriteLine($"Версия класса: {versionAttr.Major}.{versionAttr.Minor}");

            Console.WriteLine("\nСвойства:");
            PropertyInfo[] properties = targetType.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                DisplayNameAttribute propertyDisplay = property.GetCustomAttribute<DisplayNameAttribute>();
                if (propertyDisplay is not null)
                    Console.WriteLine($"  {property.Name}: {propertyDisplay.DisplayName}");
            }

            Console.WriteLine("\nМетоды:");
            MethodInfo[] methods = targetType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (MethodInfo method in methods)
            {
                DisplayNameAttribute methodDisplay = method.GetCustomAttribute<DisplayNameAttribute>();
                if (methodDisplay is not null)
                    Console.WriteLine($"  {method.Name}: {methodDisplay.DisplayName}");
            }
        }
    }
}
