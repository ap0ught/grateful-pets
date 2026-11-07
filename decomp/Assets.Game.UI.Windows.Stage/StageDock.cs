using System;
using ConsoleLib.Console;
using Qud.UI;
using UnityEngine;
using UnityEngine.UI;
using XRL;
using XRL.UI;

namespace Assets.Game.UI.Windows.Stage;

public class StageDock : MonoBehaviour
{
	public Canvas content;

	public NearbyItemsWindow itemsPane;

	public MinimapWindow minimapPane;

	public RectTransform minimapManager;

	public RectTransform nearbyItemsManager;

	public GameObject messagelogPane;

	public GameObject horizontalDragHandle;

	public Image horizontalDragHandleBackground;

	public RectTransform safeArea;

	public RectTransform stage;

	public Image backgroundImage;

	public ContentSizeFitter nearbyItemsContent;

	public float minWidth = 300f;

	public float preferredWidth = 500f;

	public float maxWidth = 633.6f;

	public RectTransform ours;

	private static StageDock _instance;

	private float _lastNearbyItemsHeight;

	private bool LastShown;

	public float calculatedWidth => Math.Min(maxWidth, Math.Max(preferredWidth, minWidth));

	public float topPadding => 90f;

	public float bottomPadding => 90f;

	public static StageDock instance => _instance;

	public void Awake()
	{
		_instance = this;
		preferredWidth = PlayerPrefs.GetFloat("DockPreferredWith", preferredWidth);
	}

	private float CalculateNearbyItemsHeight(float totalDockSize, float alreadyUsed)
	{
		float height = (nearbyItemsContent.transform as RectTransform).rect.height;
		height = Mathf.Max(height, 0f);
		float num = Mathf.Max(totalDockSize / 3f, 200f);
		height = Mathf.Min(totalDockSize - alreadyUsed - num, height);
		if (height != _lastNearbyItemsHeight)
		{
			MessageLogWindow._needsScrollToBottom = 2;
			_lastNearbyItemsHeight = height;
		}
		return height;
	}

	public void LateUpdate()
	{
		bool flag = false;
		if (Options.ShiftHidesSidebar && Keyboard.bShift && GameManager.Instance?.CurrentGameView == "Stage")
		{
			flag = true;
		}
		if (ours == null)
		{
			ours = GetComponent<RectTransform>();
		}
		if (!backgroundImage.isActiveAndEnabled && MessageLogWindow.Shown)
		{
			backgroundImage.enabled = true;
		}
		if (backgroundImage.color.a != Options.DockOpacity)
		{
			backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, Options.DockOpacity);
			horizontalDragHandleBackground.color = new Color(horizontalDragHandleBackground.color.r, horizontalDragHandleBackground.color.g, horizontalDragHandleBackground.color.b, Options.DockOpacity);
		}
		bool flag2 = GameManager.Instance.DockMovable == 1;
		if (GameManager.Instance.DockMovable == 3)
		{
			flag2 = GameManager.Instance.currentSidebarPosition == GameManager.PreferredSidebarPosition.Left;
		}
		float num = 16f;
		float num2 = 0f;
		float num3 = ours.rect.width - num + num2;
		float x = 0f;
		if (GameManager.Instance.DockMovable > 0 && (bool)GameManager.MainCameraLetterbox && MessageLogWindow.Shown && !flag)
		{
			if (!flag2)
			{
				x = num + num2;
			}
			if (!content.isActiveAndEnabled)
			{
				content.enabled = true;
			}
			if (ours.sizeDelta.x != calculatedWidth)
			{
				ours.sizeDelta = new Vector2(calculatedWidth, ours.sizeDelta.y);
			}
			_ = ours.rect.height;
			float num4 = 0f;
			if (Options.OverlayMinimap)
			{
				float num5 = 128f;
				if (ours.rect.height < 500f)
				{
					num5 = 64f;
				}
				float x2 = num3;
				RectTransform obj = minimapPane.content.transform as RectTransform;
				obj.sizeDelta = new Vector2(x2, num5);
				obj.anchoredPosition = new Vector2(x, 0f - num4);
				obj.anchorMin = new Vector2(0f, 1f);
				obj.anchorMax = new Vector2(0f, 1f);
				num4 += num5;
				minimapPane.contentCanvas.enabled = true;
				(minimapManager.transform as RectTransform).CopyFrom(minimapPane.content.transform as RectTransform);
			}
			else if (minimapPane.contentCanvas.enabled)
			{
				minimapPane.contentCanvas.enabled = false;
			}
			if (Options.OverlayNearbyObjects)
			{
				float num6 = CalculateNearbyItemsHeight(ours.rect.height, num4) + 24f;
				RectTransform obj2 = itemsPane.content.transform as RectTransform;
				obj2.sizeDelta = new Vector2(num3, num6);
				obj2.anchoredPosition = new Vector2(x, 0f - num4);
				obj2.anchorMin = new Vector2(0f, 1f);
				obj2.anchorMax = new Vector2(0f, 1f);
				num4 += num6;
				itemsPane.contentCanvas.enabled = true;
				(nearbyItemsManager.transform as RectTransform).CopyFrom(itemsPane.content.transform as RectTransform);
			}
			else if (itemsPane.contentCanvas.enabled)
			{
				itemsPane.contentCanvas.enabled = false;
			}
			messagelogPane.GetComponent<RectTransform>().sizeDelta = new Vector2(num3, ours.rect.height - num4);
			messagelogPane.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, 0f - num4);
			if (flag2)
			{
				ours.anchorMin = new Vector2(0f, 0f);
				ours.anchorMax = new Vector2(0f, 1f);
				ours.pivot = new Vector2(0f, 0f);
				(safeArea.transform as RectTransform).anchoredPosition = new Vector2(ours.rect.width, 0f - topPadding);
				(safeArea.transform as RectTransform).sizeDelta = new Vector2(stage.rect.width - ours.rect.width, stage.rect.height - topPadding - bottomPadding);
				(horizontalDragHandle.transform as RectTransform).anchoredPosition = new Vector2(ours.rect.width - num - num2, 0f);
				(horizontalDragHandle.transform as RectTransform).sizeDelta = new Vector2(num, ours.rect.height);
			}
			else
			{
				ours.anchorMin = new Vector2(1f, 0f);
				ours.anchorMax = new Vector2(1f, 1f);
				ours.pivot = new Vector2(1f, 0f);
				(safeArea.transform as RectTransform).anchoredPosition = new Vector2(0f, 0f - topPadding);
				(safeArea.transform as RectTransform).sizeDelta = new Vector2(stage.rect.width - ours.rect.width, stage.rect.height - topPadding - bottomPadding);
				(horizontalDragHandle.transform as RectTransform).anchoredPosition = new Vector2(num2, 0f);
				(horizontalDragHandle.transform as RectTransform).sizeDelta = new Vector2(num, ours.rect.height);
			}
			if (GameManager.Instance.DockMovable == 3)
			{
				(safeArea.transform as RectTransform).anchoredPosition = new Vector2(0f, 0f - topPadding);
				(safeArea.transform as RectTransform).sizeDelta = new Vector2(stage.rect.width, stage.rect.height - topPadding - bottomPadding);
			}
			if (!LastShown)
			{
				GameManager.Instance.RefreshLayout();
				GameManager.MainCameraLetterbox.OnUpdate();
			}
			LastShown = true;
		}
		else
		{
			(safeArea.transform as RectTransform).anchoredPosition = new Vector2(0f, 0f - topPadding);
			(safeArea.transform as RectTransform).sizeDelta = new Vector2(stage.rect.width, stage.rect.height - topPadding - bottomPadding);
			if (content.isActiveAndEnabled)
			{
				content.enabled = false;
			}
			if (backgroundImage.isActiveAndEnabled)
			{
				backgroundImage.enabled = false;
			}
			if (LastShown)
			{
				GameManager.Instance.RefreshLayout();
				GameManager.MainCameraLetterbox.OnUpdate();
			}
			LastShown = false;
		}
	}
}
