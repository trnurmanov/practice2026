using System;
using System.Collections.Concurrent;
using System.Threading;

namespace task17
{
    public class ServerThread
    {
        private readonly IScheduler _scheduler;
        private readonly Thread _executionThread;
        private Action _processingStrategy;
        private volatile bool _terminationRequested = false;

        public Thread Thread => _executionThread;

        public ServerThread(IScheduler scheduler)
        {
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
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
            _scheduler.Add(cmd);
        }

        public void HardStop()
        {
            _terminationRequested = true;
        }

        public void SoftStop()
        {
            UpdateBehavior(() =>
            {
                if (_scheduler.HasCommand())
                {
                    var cmd = _scheduler.Select();
                    if (cmd != null)
                    {
                        ExecuteCommand(cmd);
                        return;
                    }
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
            if (_scheduler.HasCommand())
            {
                var cmd = _scheduler.Select();
                if (cmd != null)
                {
                    ExecuteCommand(cmd);
                }
            }
            else
            {
                Thread.Sleep(1);
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
            finally
            {
                if (cmd is ILongCommand longCmd)
                {
                    (_scheduler as RoundRobinScheduler)?.Reschedule(longCmd);
                }
            }
        }
    }
}