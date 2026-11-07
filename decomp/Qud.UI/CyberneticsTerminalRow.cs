using UnityEngine;
using UnityEngine.UI;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

[ExecuteInEditMode]
public class CyberneticsTerminalRow : MonoBehaviour, IFrameworkControl
{
	public bool cursorDone;

	public bool currentCursor;

	public float cursorTimer;

	public int cursor;

	public Image background;

	public CyberneticsTerminalLineData data;

	public UITextSkin description;

	public FrameworkContext frameworkContext;

	public bool selectedMode;

	private ScrollViewCalcs _calcs = new ScrollViewCalcs();

	private CyberneticsTerminalScreen screen;

	private bool? wasSelected;

	public void HandleUpDown()
	{
		XRL.UI.Framework.Event currentEvent = NavigationController.currentEvent;
		if (currentEvent.axisValue > 0)
		{
			if (ScrollViewCalcs.GetScrollViewCalcs(transform as RectTransform, _calcs).isAnyBelowView)
			{
				_calcs.ScrollPageDown();
				currentEvent.Handle();
			}
		}
		else if (currentEvent.axisValue < 0 && ScrollViewCalcs.GetScrollViewCalcs(transform as RectTransform, _calcs).isAnyAboveView)
		{
			_calcs.ScrollPageUp();
			currentEvent.Handle();
		}
	}

	public NavigationContext GetNavigationContext()
	{
		return frameworkContext.context;
	}

	public void setData(FrameworkDataElement data)
	{
		if (data is CyberneticsTerminalLineData cyberneticsTerminalLineData)
		{
			cyberneticsTerminalLineData.row = this;
			screen = cyberneticsTerminalLineData.screen;
			this.data = cyberneticsTerminalLineData;
			description.SetText("");
			cursor = 0;
			cursorTimer = 0f;
			currentCursor = false;
			cursorDone = false;
		}
	}

	public void Update()
	{
		if (!CyberneticsTerminalScreen.initReady || NavigationController.instance?.activeContext == NavigationController.instance?.suspensionContext)
		{
			return;
		}
		if (!cursorDone)
		{
			if (ControlManager.currentFrameCommands.Count > 0)
			{
				description.SetText(data.Text);
				currentCursor = false;
				cursorDone = true;
				if (data.nextCursorData == null)
				{
					screen?.HandleTextComplete();
				}
			}
			else if (currentCursor)
			{
				cursorTimer += Time.deltaTime;
				if (cursorTimer > 0.015f)
				{
					int num = (int)(cursorTimer / 0.015f);
					cursorTimer -= 0.015f * (float)num;
					cursor += num;
					if (cursor >= data.Text.Length)
					{
						description.SetText(data.Text);
						currentCursor = false;
						cursorDone = true;
						if (data.nextCursorData != null)
						{
							data.nextCursorData.row.currentCursor = true;
						}
						else if (data.nextCursorData == null)
						{
							screen?.HandleTextComplete();
						}
					}
					else
					{
						description.SetText(data.Text.Substring(0, cursor) + "_");
					}
				}
			}
		}
		bool? flag = GetNavigationContext()?.IsActive();
		if (wasSelected != flag)
		{
			wasSelected = flag;
			bool flag2 = (background.enabled = flag == true);
			selectedMode = flag2;
		}
	}
}
