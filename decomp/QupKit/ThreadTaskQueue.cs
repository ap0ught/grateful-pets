using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace QupKit;

public class ThreadTaskQueue
{
	private Queue<ThreadTaskQueueEntry> taskQueue = new Queue<ThreadTaskQueueEntry>(256);

	private Queue<ThreadTaskQueueEntry> taskPool = new Queue<ThreadTaskQueueEntry>(256);

	public Queue<ThreadTaskQueueEntry> delayedTasks = new Queue<ThreadTaskQueueEntry>();

	public Thread threadContext;

	public void clear()
	{
		taskQueue.Clear();
	}

	public void poolEntry(ThreadTaskQueueEntry e)
	{
		taskPool.Enqueue(e);
	}

	public bool HasTask()
	{
		return taskQueue.Count > 0;
	}

	public void executeTasks()
	{
		if (taskQueue.Count <= 0)
		{
			return;
		}
		int num = taskQueue.Count;
		while (taskQueue.Count > 0 && num > 0)
		{
			num--;
			ThreadTaskQueueEntry threadTaskQueueEntry = null;
			lock (taskQueue)
			{
				if (taskQueue.Count > 0)
				{
					threadTaskQueueEntry = taskQueue.Dequeue();
				}
			}
			if (threadTaskQueueEntry != null)
			{
				if (threadTaskQueueEntry.Delay > 0)
				{
					threadTaskQueueEntry.Delay--;
					delayedTasks.Enqueue(threadTaskQueueEntry);
				}
				else
				{
					threadTaskQueueEntry.Execute(this);
				}
			}
		}
		if (delayedTasks.Count <= 0)
		{
			return;
		}
		lock (taskQueue)
		{
			while (delayedTasks.Count > 0)
			{
				taskQueue.Enqueue(delayedTasks.Dequeue());
			}
		}
	}

	public Task<TResult> executeAsync<TResult>(Func<TResult> a, string TaskID = null, bool OverwritePrevious = false)
	{
		TaskCompletionSource<TResult> promise = new TaskCompletionSource<TResult>();
		Action a2 = () =>
		{
			try
			{
				promise.TrySetResult(a());
			}
			catch (Exception exception)
			{
				promise.TrySetException(exception);
			}
		};
		if (TaskID != null)
		{
			queueSingletonTask(TaskID, a2, OverwritePrevious);
		}
		else
		{
			queueTask(a2);
		}
		return promise.Task;
	}

	public ThreadTaskQueueEntry queueSingletonTask(string TaskID, Action a, bool OverwritePrevious = false)
	{
		lock (taskQueue)
		{
			foreach (ThreadTaskQueueEntry item in taskQueue)
			{
				if (item.TaskID == TaskID)
				{
					if (OverwritePrevious)
					{
						item.SetAction(a);
					}
					return item;
				}
			}
			ThreadTaskQueueEntry threadTaskQueueEntry;
			if (taskPool.Count > 0)
			{
				threadTaskQueueEntry = taskPool.Dequeue();
				threadTaskQueueEntry.SetAction(a);
				threadTaskQueueEntry.Delay = 0;
			}
			else
			{
				threadTaskQueueEntry = new ThreadTaskQueueEntry(a);
				threadTaskQueueEntry.Delay = 0;
			}
			threadTaskQueueEntry.TaskID = TaskID;
			taskQueue.Enqueue(threadTaskQueueEntry);
			return threadTaskQueueEntry;
		}
	}

	public void awaitTask(Action a)
	{
		if (Thread.CurrentThread == threadContext)
		{
			a();
			return;
		}
		ManualResetEvent awaitEvent = new ManualResetEvent(initialState: false);
		queueTask(() =>
		{
			try
			{
				a();
			}
			finally
			{
				awaitEvent.Set();
			}
		});
		awaitEvent.WaitOne();
	}

	public ThreadTaskQueueEntry queueTask(Action a, int delay = 0)
	{
		lock (taskQueue)
		{
			ThreadTaskQueueEntry threadTaskQueueEntry;
			if (taskPool.Count > 0)
			{
				threadTaskQueueEntry = taskPool.Dequeue();
				threadTaskQueueEntry.SetAction(a);
				threadTaskQueueEntry.Delay = delay;
			}
			else
			{
				threadTaskQueueEntry = new ThreadTaskQueueEntry(a);
				threadTaskQueueEntry.Delay = delay;
			}
			taskQueue.Enqueue(threadTaskQueueEntry);
			return threadTaskQueueEntry;
		}
	}
}
