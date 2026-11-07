using UnityEngine;
using UnityEngine.UI;
using XRL;
using XRL.Core;
using XRL.UI;

namespace Qud.UI;

[ExecuteAlways]
[HasGameBasedStaticCache]
[UIView("MessageLog", false, false, false, null, null, false, 0, false, NavCategory = "Adventure", UICanvas = "MessageLog", UICanvasHost = 1)]
public class MessageLogWindow : MovableSceneFrameWindowBase<MessageLogWindow>
{
	public MessageLogPooledScrollRect messageLog;

	public MinimapWindow Minimap;

	public Scrollbar ScrollBar;

	public static int _needsScrollToBottom = 0;

	public static int lastMessageLogSize = -1;

	public static bool Shown = true;

	public void LateUpdate()
	{
		if (lastMessageLogSize != Options.MessageLogLineSizeAdjustment)
		{
			lastMessageLogSize = Options.MessageLogLineSizeAdjustment;
			Reflow();
			_needsScrollToBottom = 2;
		}
		if (_needsScrollToBottom > 0)
		{
			messageLog?.ScrollToBot();
			_needsScrollToBottom--;
		}
	}

	public void ShowIfEnabled()
	{
		if (Shown)
		{
			Show();
		}
	}

	public new void Update()
	{
		if (Application.isPlaying && canvas.enabled)
		{
			if (ControlManager.isCommandDown("CmdShowSidebar"))
			{
				Shown = !Shown;
			}
			if (GameManager.Instance.DockMovable <= 0)
			{
				toggle.SetActive(Shown);
			}
			bool docked = Docked;
			if (GameManager.Instance.DockMovable > 0 && (bool)GameManager.MainCameraLetterbox)
			{
				Docked = true;
			}
			else
			{
				Docked = false;
			}
			if (Docked != docked)
			{
				messageLog.Reflow();
			}
			base.Update();
		}
	}

	[GameBasedCacheInit]
	public static void GameInit()
	{
		SingletonWindowBase<MessageLogWindow>.instance?.ClearMessageLog();
	}

	public void Reflow()
	{
		messageLog?.Reflow();
	}

	public void ClearMessageLog()
	{
		messageLog?.Clear();
	}

	public override void Init()
	{
		base.Init();
		XRLCore.RegisterNewMessageLogEntryCallback(AddMessage);
	}

	public void AddMessage(string log)
	{
		string l = ":: " + log;
		GameManager.Instance?.uiQueue?.queueTask(() =>
		{
			_AddMessage(l);
		});
	}

	private void _AddMessage(string log)
	{
		messageLog?.Add(RTF.FormatToRTF(log));
	}

	public override bool AllowPassthroughInput()
	{
		return true;
	}
}
