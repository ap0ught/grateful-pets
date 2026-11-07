using UnityEngine;

public class exHotPoint
{
	public int id = -1;

	public bool active;

	public bool pressDown;

	public bool pressUp;

	public Vector2 pos = Vector2.zero;

	public Vector2 delta = Vector2.zero;

	public Vector3 worldPos = Vector3.zero;

	public Vector3 worldDelta = Vector3.zero;

	public exUIControl hover;

	public exUIControl pressed;

	public bool isMouse;

	public bool isTouch => !isMouse;

	public bool GetMouseButton(int _id)
	{
		if (isMouse)
		{
			return id == _id;
		}
		return false;
	}

	public void Reset()
	{
		active = false;
		pressDown = false;
		pressUp = false;
		pos = Vector2.zero;
		delta = Vector2.zero;
		worldPos = Vector3.zero;
		worldDelta = Vector3.zero;
		hover = null;
		pressed = null;
	}
}
