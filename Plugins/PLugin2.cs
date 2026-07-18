using CommandLib;
using System;

namespace Plugin
{
    [PluginLoad("TestCommand")]
    public class DependentPlugin : ICommand
    {
        public void Execute()
        {
            Console.WriteLine("Зависимый плагин. Выполняется после TestCommand.");
        }
    }
}
