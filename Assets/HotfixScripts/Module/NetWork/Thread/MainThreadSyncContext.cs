using System;
using System.Collections.Concurrent;
using System.Threading;

public sealed class MainThreadSyncContext : SynchronizationContext
{
    private readonly ConcurrentQueue<Action> safeQueue = new ConcurrentQueue<Action>();

    public void Update()
    {
        while (true)
        {
            if (safeQueue.TryDequeue(out Action action) == false)
                return;
            action.Invoke();
        }
    }

    public override void Post(SendOrPostCallback d, object state)
    {
        Action action = new Action(() => { d(state); });
        safeQueue.Enqueue(action);
    }
}
