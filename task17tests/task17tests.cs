using System;
using System.Threading;
using Xunit;
using task17;

namespace task17tests
{
    public class ServerThreadTests : IDisposable
    {
        private readonly ServerThread _executor;

        public ServerThreadTests()
        {
            _executor = new ServerThread();
            ExceptionHandler.Clear();
        }

        public void Dispose()
        {
            _executor.HardStop();
            _executor.Join();
        }

        [Fact]
        public void ForceStop_TerminatesExecution_AndClearsPendingWork()
        {
            var wasExecuted = false;
            var operation = new ActionCommand(() => wasExecuted = true);

            _executor.Start();
            _executor.Add(operation);
            Thread.Sleep(200);

            _executor.HardStop();
            _executor.Join();

            Assert.True(wasExecuted);
            Assert.False(_executor.Thread.IsAlive);
        }

        [Fact]
        public void GracefulStop_FinishesAllQueuedWork_ThenShutsDown()
        {
            var completedOperations = 0;
            var operations = new[]
            {
                new ActionCommand(() => Interlocked.Increment(ref completedOperations)),
                new ActionCommand(() => Interlocked.Increment(ref completedOperations)),
                new ActionCommand(() => Interlocked.Increment(ref completedOperations))
            };

            _executor.Start();
            foreach (var op in operations)
            {
                _executor.Add(op);
            }

            _executor.SoftStop();
            _executor.Join();

            Assert.Equal(3, completedOperations);
            Assert.False(_executor.Thread.IsAlive);
        }

        [Fact]
        public void StopCommands_MustFail_WhenCalledFromOutsideWorkerThread()
        {
            _executor.Start();
            var forceStopCmd = new HardStopCommand(_executor);
            var gracefulStopCmd = new SoftStopCommand(_executor);

            Assert.Throws<InvalidOperationException>(() => forceStopCmd.Execute());
            Assert.Throws<InvalidOperationException>(() => gracefulStopCmd.Execute());
        }

        private class ActionCommand : ICommand
        {
            private readonly Action _callback;

            public ActionCommand(Action callback)
            {
                _callback = callback;
            }

            public void Execute()
            {
                _callback?.Invoke();
            }
        }
    }
}
