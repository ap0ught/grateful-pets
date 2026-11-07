using System;
using System.Threading;
using LaundryBear;
using LaundryBear.PlatformServices;
using UnityEngine;

namespace Platform.Performance;

public struct CPUBoost : IDisposable
{
	private static int requestCount;

	private bool endWasCalled;

	private bool beginWasCalled;

	private bool forceMainThread;

	private static int mainManagedThreadId;

	private static SynchronizationContext mainSyncContext;

	public static void InitOnMainThread()
	{
		mainManagedThreadId = Thread.CurrentThread.ManagedThreadId;
		mainSyncContext = SynchronizationContext.Current;
	}

	public static CPUBoost AutoScopeForceMainThread()
	{
		CPUBoost result = default;
		result.forceMainThread = true;
		result.Begin();
		return result;
	}

	public static CPUBoost AutoScope()
	{
		CPUBoost result = default;
		result.Begin();
		return result;
	}

	public void Dispose()
	{
		End();
	}

	public void Begin()
	{
		if (!forceMainThread && mainManagedThreadId != Thread.CurrentThread.ManagedThreadId)
		{
			Debug.LogError("CPUBoost was called off the main UI thread which is invalid usage.");
		}
		else
		{
			if (beginWasCalled)
			{
				return;
			}
			Debug.Log("CPUBoost:: FastLoad");
			Interlocked.Increment(ref requestCount);
			beginWasCalled = true;
			if (requestCount > 0)
			{
				if (forceMainThread)
				{
					RawBeginForceMainThread();
				}
				else
				{
					RawBegin();
				}
			}
		}
	}

	public void End()
	{
		if (!forceMainThread && mainManagedThreadId != Thread.CurrentThread.ManagedThreadId)
		{
			Debug.LogError("CPUBoost:: Dispose was called off the main UI thread which is invalid usage. Will continue anyways...");
		}
		else if (!beginWasCalled)
		{
			Debug.LogWarning("CPUBoost:: non-symetric Begin-End calls. Prefer using AutoScope over manualy begin() end() calls");
		}
		else
		{
			if (endWasCalled)
			{
				return;
			}
			Debug.Log("CPUBoost:: Normal");
			endWasCalled = true;
			Interlocked.Decrement(ref requestCount);
			if (requestCount == 0)
			{
				if (forceMainThread)
				{
					RawEndForceMainThread();
				}
				else
				{
					RawEnd();
				}
			}
		}
	}

	public static void RawBeginForceMainThread()
	{
		mainSyncContext.Send((object _) =>
		{
			RawBegin();
		}, null);
	}

	public static void RawEndForceMainThread()
	{
		mainSyncContext.Send((object _) =>
		{
			RawEnd();
		}, null);
	}

	private static void RawBegin()
	{
		if (ServiceLocator.TryGetService<IPerformanceService>(out var service))
		{
			service.BeginCpuCritical();
		}
	}

	private static void RawEnd()
	{
		if (ServiceLocator.TryGetService<IPerformanceService>(out var service))
		{
			service.EndCpuCritical();
		}
	}
}
