using CommandLib;
using System;

namespace Plugin
{
    [PluginLoad("")]
    public class TestCommand : ICommand
    {
        public void Execute()
        {
            Console.WriteLine("Тестовый плагин.");
        }

    }

}