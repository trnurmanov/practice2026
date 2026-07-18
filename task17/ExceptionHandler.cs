using System;

namespace task17
{
    public static class ExceptionHandler
    {
        public static ICommand FailedCommand { get; set; }
        public static Exception CapturedError { get; set; }

        public static void Handle(ICommand command, Exception error)
        {
            FailedCommand = command;
            CapturedError = error;
            Console.WriteLine($"Error: {CapturedError.Message} | Command: {FailedCommand.GetType().Name}");
        }

        public static void Clear()
        {
            FailedCommand = null;
            CapturedError = null;
        }
    }
}
