using System.Collections.Generic;
using UnityEngine;
using XRL.UI;

namespace Qud.UI;

public class CursorManager : MonoBehaviour
{
	public enum Style
	{
		Pointer,
		ResizeNorthSouth,
		ResizeWestEast
	}

	public static CursorManager instance;

	public List<Texture2D> hiddenCursor;

	public List<Texture2D> normalCursor;

	public Vector2 resizeHotSpot;

	public List<Texture2D> resizeNorthSouthCursor;

	public List<Texture2D> resizeWestEastCursor;

	public float cursorHideDelay = 3f;

	private bool? _cursorAutoHideEnabled;

	private Style _currentStyle;

	private bool _cursorHidden;

	public bool forceCursorHidden;

	private Vector3 lastMousePosition;

	private bool syncCursor = true;

	private float cursorHideTime;

	public int cursorThemeIndex => Options.MouseCursor;

	public bool cursorAutoHideEnabled
	{
		get
		{
			bool valueOrDefault = _cursorAutoHideEnabled == true;
			if (!_cursorAutoHideEnabled.HasValue)
			{
				valueOrDefault = Options.GetOptionBool("OptionMouseAutoHideCursorEnabled");
				_cursorAutoHideEnabled = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public Style currentStyle
	{
		get
		{
			return _currentStyle;
		}
		set
		{
			if (_currentStyle != value)
			{
				syncCursor = true;
				_currentStyle = value;
			}
		}
	}

	public bool cursorHidden
	{
		get
		{
			return _cursorHidden;
		}
		set
		{
			if (_cursorHidden != value)
			{
				syncCursor = true;
				_cursorHidden = value;
			}
		}
	}

	public void Sync()
	{
		syncCursor = true;
	}

	public void Start()
	{
		instance = this;
	}

	public static void setStyle(Style style)
	{
		if (instance != null)
		{
			instance.currentStyle = style;
		}
	}

	private bool inWindow(Vector2 v)
	{
		if (v.x >= 0f && v.y >= 0f && v.x <= (float)Screen.width)
		{
			return v.y <= (float)Screen.height;
		}
		return false;
	}

	public void Update()
	{
		if (inWindow(Input.mousePosition) && !inWindow(lastMousePosition))
		{
			syncCursor = true;
		}
		if (Vector3.Distance(lastMousePosition, Input.mousePosition) > 1f && !forceCursorHidden)
		{
			cursorHideTime = Time.realtimeSinceStartup + cursorHideDelay;
			cursorHidden = false;
			lastMousePosition = Input.mousePosition;
		}
		if (cursorAutoHideEnabled && !cursorHidden && Application.isFocused && cursorHideTime < Time.realtimeSinceStartup)
		{
			cursorHidden = true;
		}
	}

	public void LateUpdate()
	{
		if (syncCursor)
		{
			syncCursor = false;
			Texture2D texture2D = ((!cursorHidden) ? (currentStyle switch
			{
				Style.Pointer => normalCursor[cursorThemeIndex], 
				Style.ResizeNorthSouth => resizeNorthSouthCursor[cursorThemeIndex], 
				Style.ResizeWestEast => resizeWestEastCursor[cursorThemeIndex], 
				_ => null, 
			}) : hiddenCursor[cursorThemeIndex]);
			Texture2D texture = texture2D;
			Cursor.SetCursor(texture, currentStyle switch
			{
				Style.ResizeNorthSouth => resizeHotSpot, 
				Style.ResizeWestEast => resizeHotSpot, 
				_ => Vector2.zero, 
			}, CursorMode.Auto);
		}
	}

	public void UpdateOptions()
	{
		_cursorAutoHideEnabled = null;
		cursorHidden = false;
		syncCursor = true;
	}
}
