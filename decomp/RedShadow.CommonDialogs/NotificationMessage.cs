using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RedShadow.CommonDialogs;

public class NotificationMessage : DialogBase
{
	public float Duration = 10f;

	public NotificationArea Area = NotificationArea.LowerRight;

	public static int MaxVisible = 5;

	private Image _icon;

	private Text _messageText;

	private static readonly List<NotificationMessage> _activeMessages = new List<NotificationMessage>();

	private float _startTime;

	private float _moveStartTime;

	private Vector3 _startPos;

	private Vector3 _endPos;

	protected override void Awake()
	{
		base.Awake();
		_messageText = transform.Find("Window/MessagePanel/Text").GetComponent<Text>();
		_icon = transform.Find("Window/MessagePanel/Icon").GetComponent<Image>();
	}

	public override void Update()
	{
		base.Update();
		RectTransform component = _mainWindow.GetComponent<RectTransform>();
		if (component.position != _endPos)
		{
			float num = (Time.time - _moveStartTime) / 0.2f;
			if (num > 1f)
			{
				component.position = _endPos;
			}
			else
			{
				component.position = (_endPos - _startPos) * num + _startPos;
			}
		}
		if (IsVisible && _startTime > 0f && Time.time > _startTime + Duration)
		{
			hide();
		}
		while (_activeMessages.Count > MaxVisible)
		{
			_activeMessages[0].hide();
		}
	}

	public void show(string text, Sprite icon)
	{
		setIcon(icon);
		setText(text);
		StartCoroutine(show_co(0.01f));
	}

	public NotificationMessage setText(string text)
	{
		_messageText.text = text;
		if (IsVisible)
		{
			Canvas.ForceUpdateCanvases();
			adjustPositions();
		}
		return this;
	}

	public NotificationMessage setIcon(Sprite icon)
	{
		_icon.sprite = icon;
		_icon.gameObject.SetActive(icon != null);
		if (IsVisible)
		{
			Canvas.ForceUpdateCanvases();
			adjustPositions();
		}
		return this;
	}

	public static void clear()
	{
		while (_activeMessages.Count > 0)
		{
			_activeMessages[0].hide();
		}
	}

	protected override void show()
	{
		base.show();
		DialogBase.VisibleDialogs.Remove(this);
		_mainWindow.transform.eulerAngles = new Vector3(0f, 0f, 0f);
		_startTime = Time.time;
		_activeMessages.Add(this);
		Canvas.ForceUpdateCanvases();
		adjustPositions();
	}

	protected override void hide()
	{
		base.hide();
		_activeMessages.Remove(this);
		adjustPositions();
	}

	private void adjustPositions()
	{
		RectTransform component = GetComponent<RectTransform>();
		Vector3[] array = new Vector3[4];
		component.GetWorldCorners(array);
		float num = 0f;
		for (int num2 = _activeMessages.Count - 1; num2 >= 0; num2--)
		{
			NotificationMessage notificationMessage = _activeMessages[num2];
			RectTransform component2 = notificationMessage._mainWindow.GetComponent<RectTransform>();
			Vector3[] array2 = new Vector3[4];
			component2.GetWorldCorners(array2);
			float num3 = array2[2].x - array2[0].x;
			float num4 = array2[2].y - array2[0].y;
			float x = ((Area != NotificationArea.LowerLeft && Area != NotificationArea.UpperLeft) ? (array[2].x - 5f - num3 / 2f) : (array[0].x + 5f + num3 / 2f));
			float y = ((Area != NotificationArea.LowerLeft && Area != NotificationArea.LowerRight) ? (array[2].y - 5f - num4 / 2f - num) : (array[0].y + 5f + num4 / 2f + num));
			notificationMessage._moveStartTime = Time.time;
			notificationMessage._startPos = component2.position;
			notificationMessage._endPos = new Vector2(x, y);
			num += num4 + 5f;
		}
	}
}
