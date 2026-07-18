using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace task17
{
    public interface IScheduler
    {
        bool HasCommand();
        ICommand Select();
        void Add(ICommand cmd);
    }

    public class RoundRobinScheduler : IScheduler
    {
        private readonly ConcurrentQueue<ICommand> _pendingCommands = new ConcurrentQueue<ICommand>();
        private readonly LinkedList<ILongCommand> _activeOperations = new LinkedList<ILongCommand>();
        private LinkedListNode<ILongCommand> _currentOperation;
        private readonly object _syncRoot = new object();
        private volatile int _totalCount = 0;

        public bool HasCommand()
        {
            return _totalCount > 0;
        }

        public void Add(ICommand cmd)
        {
            if (cmd == null)
                throw new ArgumentNullException(nameof(cmd));

            if (cmd is ILongCommand longCmd)
            {
                lock (_syncRoot)
                {
                    _activeOperations.AddLast(longCmd);
                }
            }
            else
            {
                _pendingCommands.Enqueue(cmd);
            }

            Interlocked.Increment(ref _totalCount);
        }

        public ICommand Select()
        {
            if (_pendingCommands.TryDequeue(out ICommand pendingCmd))
            {
                Interlocked.Decrement(ref _totalCount);
                return pendingCmd;
            }

            lock (_syncRoot)
            {
                if (_activeOperations.Count == 0)
                    return null;

                if (_currentOperation == null || _currentOperation.Next == null)
                    _currentOperation = _activeOperations.First;
                else
                    _currentOperation = _currentOperation.Next;

                return _currentOperation.Value;
            }
        }

        public void Reschedule(ILongCommand operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            if (!operation.IsCompleted)
            {
                return;
            }

            lock (_syncRoot)
            {
                var node = FindNode(operation);
                if (node != null)
                {
                    if (_currentOperation == node)
                    {
                        _currentOperation = node.Next ?? _activeOperations.First;
                    }
                    _activeOperations.Remove(node);
                }
            }

            Interlocked.Decrement(ref _totalCount);
        }

        private LinkedListNode<ILongCommand> FindNode(ILongCommand operation)
        {
            var current = _activeOperations.First;
            while (current != null)
            {
                if (ReferenceEquals(current.Value, operation))
                    return current;
                current = current.Next;
            }
            return null;
        }
    }
}