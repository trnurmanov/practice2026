using System;
using System.Collections.Concurrent;
using System.Threading;

namespace task17
{
    public class ServerThread
    {
        private readonly BlockingCollection<ICommand> _commandBuffer = new BlockingCollection<ICommand>();
        private readonly Thread _executionThread;
        private Action _processingStrategy;
        private volatile bool _terminationRequested = false;

        public Thread Thread => _executionThread;

        public ServerThread()
        {
            _processingStrategy = DefaultBehavior;
            _executionThread = new Thread(Run);
        }

        public void Start()
        {
            _executionThread.Start();
        }

        public void Join()
        {
            _executionThread.Join();
        }

        public void Add(ICommand cmd)
        {
            try
            {
                _commandBuffer.Add(cmd);
            }
            catch (InvalidOperationException)
            {
            }
        }

        public void HardStop()
        {
            _terminationRequested = true;
            _commandBuffer.CompleteAdding();
        }

        public void SoftStop()
        {
            _commandBuffer.CompleteAdding();
            UpdateBehavior(() =>
            {
                if (_commandBuffer.TryTake(out ICommand cmd))
                {
                    ExecuteCommand(cmd);
                    return;
                }
                HardStop();
            });
        }

        public void UpdateBehavior(Action nextBehavior)
        {
            _processingStrategy = nextBehavior ?? throw new ArgumentNullException(nameof(nextBehavior));
        }

        private void Run()
        {
            while (!_terminationRequested)
            {
                _processingStrategy();
            }
        }

        private void DefaultBehavior()
        {
            try
            {
                if (_commandBuffer.TryTake(out ICommand cmd, 100))
                {
                    ExecuteCommand(cmd);
                }
            }
            catch (InvalidOperationException)
            {
                HardStop();
            }
        }

        private void ExecuteCommand(ICommand cmd)
        {
            try
            {
                cmd.Execute();
            }
            catch (Exception ex)
            {
                ExceptionHandler.Handle(cmd, ex);
            }
        }
    }
}
