using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;
using task17;

namespace task17tests
{
    public class SchedulerTests : IDisposable
    {
        private RoundRobinScheduler _planner;
        private ServerThread _worker;

        public SchedulerTests()
        {
            _planner = new RoundRobinScheduler();
            _worker = new ServerThread(_planner);
            ExceptionHandler.Clear();
        }

        public void Dispose()
        {
            _worker.HardStop();
            _worker.Join();
        }

        [Fact]
        public void LongOperation_ExecutesMultipleTimes_UntilCompleted()
        {
            var executionLog = new List<int>();
            var longOp = new StepOperation(3, () => executionLog.Add(1));

            _planner.Add(longOp);
            _worker.Start();

            while (!longOp.IsCompleted)
                Thread.Sleep(10);

            Thread.Sleep(100);
            _worker.HardStop();
            _worker.Join();

            Assert.Equal(3, executionLog.Count);
        }

        [Fact]
        public void RoundRobin_AlternatesBetweenMultipleLongOperations()
        {
            var log = new List<string>();
            var opA = new StepOperation(3, () => log.Add("A"));
            var opB = new StepOperation(3, () => log.Add("B"));
            var opC = new StepOperation(3, () => log.Add("C"));

            _planner.Add(opA);
            _planner.Add(opB);
            _planner.Add(opC);
            _worker.Start();

            while (!opA.IsCompleted || !opB.IsCompleted || !opC.IsCompleted)
                Thread.Sleep(10);

            Thread.Sleep(100);
            _worker.HardStop();
            _worker.Join();

            Assert.Equal(3, log.FindAll(x => x == "A").Count);
            Assert.Equal(3, log.FindAll(x => x == "B").Count);
            Assert.Equal(3, log.FindAll(x => x == "C").Count);

            Assert.Equal("A", log[0]);
            Assert.Equal("B", log[1]);
            Assert.Equal("C", log[2]);
        }


        private class StepOperation : ILongCommand
        {
            private readonly int _maxSteps;
            private readonly Action _action;
            private int _currentStep;

            public bool IsCompleted => _currentStep >= _maxSteps;

            public StepOperation(int maxSteps, Action action)
            {
                _maxSteps = maxSteps;
                _action = action;
            }

            public void Execute()
            {
                if (!IsCompleted)
                {
                    _action?.Invoke();
                    _currentStep++;
                }
            }
        }
    }
}