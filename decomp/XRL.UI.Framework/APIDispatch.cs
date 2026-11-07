using System;
using System.Threading.Tasks;
using Qud.UI;
using XRL.World;

namespace XRL.UI.Framework;

public static class APIDispatch
{
	public static async Task<T> RunAndWaitAsync<T>(Func<T> A)
	{
		string oldView = GameManager.Instance.CurrentGameView;
		UIManager.pushSaveStack();
		try
		{
			return await NavigationController.instance.SuspendContextWhile(async () =>
			{
				T result = default;
				await Task.Run(() =>
				{
					try
					{
						result = A();
					}
					catch (Exception x2)
					{
						MetricsManager.LogException("APIDispatch", x2);
					}
				});
				return result;
			});
		}
		catch (Exception x)
		{
			MetricsManager.LogException("APIDispatch (outer)", x);
		}
		finally
		{
			UIManager.popSaveStack();
			ControlManager.ConsumeAllInput();
			GameManager.EnsureGameView(oldView);
		}
		return default;
	}

	public static async Task RunAndWaitAsync(Action A)
	{
		string oldView = GameManager.Instance.CurrentGameView;
		bool wasHeld = MinEvent.UIHold;
		MinEvent.UIHold = false;
		UIManager.pushSaveStack();
		try
		{
			await NavigationController.instance.SuspendContextWhile(async () =>
			{
				await Task.Run(() =>
				{
					try
					{
						A();
					}
					catch (Exception x2)
					{
						MetricsManager.LogException("APIDispatch", x2);
					}
				});
			});
		}
		catch (Exception x)
		{
			MetricsManager.LogException("APIDispatch (outer)", x);
		}
		finally
		{
			MinEvent.UIHold = wasHeld;
			UIManager.popSaveStack();
			ControlManager.ConsumeAllInput();
			GameManager.EnsureGameView(oldView);
		}
	}
}
