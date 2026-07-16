using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace task05
{
    public class ClassAnalyzer
    {
        private readonly Type _targetType;

        public ClassAnalyzer(Type targetType)
        {
            _targetType = targetType ?? throw new ArgumentNullException(nameof(targetType), "Тип не может быть null");
        }

        public IEnumerable<string> GetPublicMethods()
        {
            return _targetType.GetMethods().Select(method => method.Name);
        }

        public IEnumerable<string> GetMethodParams(string methodName)
        {
            if (string.IsNullOrWhiteSpace(methodName))
                return Enumerable.Empty<string>();

            MethodInfo method = _targetType.GetMethod(methodName);

            if (method is null)
                return Enumerable.Empty<string>();

            IEnumerable<string> parameters = method.GetParameters()
                .Select(param => $"{param.ParameterType.Name} {param.Name}");

            string signature = $"{method.ReturnType.Name} {methodName}({string.Join(", ", parameters)})";

            return new List<string> { signature };
        }

        public IEnumerable<string> GetAllFields()
        {
            return _targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Select(field => field.Name);
        }

        public IEnumerable<string> GetProperties()
        {
            return _targetType.GetProperties().Select(property => property.Name);
        }

        public bool HasAttribute<T>() where T : Attribute
        {
            return _targetType.IsDefined(typeof(T), true);
        }
    }
}
