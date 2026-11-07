using System;
using Qud.UI;

namespace XRL.UI;

public static class Loading
{
	public struct Status : IDisposable
	{
		public string Message;

		public string Description;

		public long StartTime;

		public bool Visible;

		public Status(string Message, string Description = null, bool Visible = true)
		{
			this.Message = Message ?? throw new ArgumentNullException("Message");
			this.Description = Description ?? (Description = Message);
			this.Visible = Visible;
			if (Visible)
			{
				LoadStatuses.Push(Message);
				SetLoadingStatus(Message);
			}
			TimeSpan timeSpan = new TimeSpan(GameManager.Time.ElapsedTicks);
			MetricsManager.LogInfo($"Starting '{Description}' task at {timeSpan:hh\\:mm\\:ss\\:fff}");
			StartTime = (GameManager.Time.IsRunning ? GameManager.Time.ElapsedMilliseconds : (-1));
		}

		public void Dispose()
		{
			if (Message != null)
			{
				if (StartTime != -1)
				{
					long num = GameManager.Time.ElapsedMilliseconds - StartTime;
					MetricsManager.LogInfo($"Finished '{Description}' task in {num}ms");
				}
				if (Visible)
				{
					LoadStatuses.Pop();
					SetLoadingStatus(LoadStatuses.Peek());
				}
				Message = null;
			}
		}
	}

	private static CleanStack<string> LoadStatuses = new CleanStack<string>();

	public static Status StartTask(string Message, string Description = null, bool Visible = true)
	{
		return new Status(Message, Description, Visible);
	}

	public static void LoadTask(string description, Action work, bool showToUser = true)
	{
		Status status = StartTask(description, null, showToUser);
		try
		{
			work();
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Failed '" + description + "' task with exception", x);
		}
		finally
		{
			status.Dispose();
		}
	}

	public static void SetLoadingStatus(string description, bool waitForUiUpdate = false)
	{
		SingletonWindowBase<LoadingStatusWindow>.instance?.SetLoadingStatus(description, waitForUiUpdate);
	}

	public static void SetHideLoadStatus(bool hidden)
	{
		if (!(SingletonWindowBase<LoadingStatusWindow>.instance == null))
		{
			SingletonWindowBase<LoadingStatusWindow>.instance.StayHidden = hidden;
		}
	}
}
