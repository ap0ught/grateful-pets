using System.Collections;
using UnityEngine;

public class SwipeCompass : MonoBehaviour
{
	public bool controlEnabled = true;

	public bool skipAutoSetup;

	public bool allowInput = true;

	public bool clickEdgeToSwitch = true;

	public Vector2 pxDistBetweenValues = new Vector2(200f, 50f);

	private Vector2 partFactor = new Vector2(1f, 1f);

	public Vector2 startValue = Vector2.zero;

	public Vector2 currentValue = Vector2.zero;

	public Vector2 maxValue = new Vector2(10f, 10f);

	public Rect activeArea;

	public Rect leftEdgeRectForClickSwitch;

	public Rect rightEdgeRectForClickSwitch;

	public Matrix4x4 matrix = Matrix4x4.identity;

	private bool touched;

	private int[] fingerStartArea = new int[20];

	private Vector2[] fingerStartPos = new Vector2[20];

	private int mouseStartArea;

	public Vector2 smoothValue = Vector2.zero;

	private Vector2 smoothStartPos = Vector2.zero;

	private Vector2 smoothDragOffset = new Vector2(0.2f, 0.2f);

	private float[] prevSmoothValueX = new float[5];

	private float[] prevSmoothValueY = new float[5];

	private float realtimeStamp;

	private float xVelocity;

	private float yVelocity;

	public Vector2 maxSpeed = new Vector2(20f, 20f);

	private Vector2 mStartPos;

	private Vector3 pos;

	private Vector2 tPos;

	public bool selected;

	public bool debug;

	private IEnumerator Start()
	{
		if (clickEdgeToSwitch && !allowInput)
		{
			Debug.LogWarning("You have enabled clickEdgeToSwitch, but it will not work because allowInput is disabled!", this);
		}
		if (pxDistBetweenValues.x == 0f && pxDistBetweenValues.y == 0f)
		{
			Debug.LogWarning("pxDistBetweenValues is zero - you won't be able to swipe with this setting...", this);
		}
		yield return new WaitForSeconds(0.2f);
		if (!skipAutoSetup)
		{
			Setup();
		}
	}

	public void Setup()
	{
		partFactor.x = 1f / pxDistBetweenValues.x;
		partFactor.y = 1f / pxDistBetweenValues.y;
		smoothValue.x = Mathf.Round(currentValue.x);
		smoothValue.y = Mathf.Round(currentValue.y);
		currentValue = startValue;
		if (activeArea != new Rect(0f, 0f, 0f, 0f))
		{
			SetActiveArea(activeArea);
		}
		if (leftEdgeRectForClickSwitch == new Rect(0f, 0f, 0f, 0f))
		{
			CalculateEdgeRectsFromActiveArea();
		}
		if (matrix == Matrix4x4.zero)
		{
			matrix = Matrix4x4.identity.inverse;
		}
	}

	public void SetActiveArea(Rect myRect)
	{
		activeArea = myRect;
	}

	public void CalculateEdgeRectsFromActiveArea()
	{
		CalculateEdgeRectsFromActiveArea(activeArea);
	}

	public void CalculateEdgeRectsFromActiveArea(Rect myRect)
	{
		leftEdgeRectForClickSwitch.x = myRect.x;
		leftEdgeRectForClickSwitch.y = myRect.y;
		leftEdgeRectForClickSwitch.width = myRect.width * 0.5f;
		leftEdgeRectForClickSwitch.height = myRect.height;
		rightEdgeRectForClickSwitch.x = myRect.x + myRect.width * 0.5f;
		rightEdgeRectForClickSwitch.y = myRect.y;
		rightEdgeRectForClickSwitch.width = myRect.width * 0.5f;
		rightEdgeRectForClickSwitch.height = myRect.height;
	}

	public void SetEdgeRects(Rect leftRect, Rect rightRect)
	{
		leftEdgeRectForClickSwitch = leftRect;
		rightEdgeRectForClickSwitch = rightRect;
	}

	private float GetAvgValue(float[] arr)
	{
		float num = 0f;
		for (int i = 0; i < arr.Length; i++)
		{
			num += arr[i];
		}
		if (arr.Length == 5)
		{
			return num * 0.2f;
		}
		return num / (float)arr.Length;
	}

	private void FillArrayWithValue(float[] arr, float val)
	{
		for (int i = 0; i < arr.Length; i++)
		{
			arr[i] = val;
		}
	}

	private void Update()
	{
		if (controlEnabled)
		{
			touched = false;
			_ = allowInput;
			if (!touched)
			{
				smoothValue.x = Mathf.SmoothDamp(smoothValue.x, currentValue.x, ref xVelocity, 0.3f, maxSpeed.x, Time.realtimeSinceStartup - realtimeStamp);
				smoothValue.y = Mathf.SmoothDamp(smoothValue.y, currentValue.y, ref yVelocity, 0.3f, maxSpeed.y, Time.realtimeSinceStartup - realtimeStamp);
			}
			realtimeStamp = Time.realtimeSinceStartup;
		}
	}

	private void OnGUI()
	{
		if (debug)
		{
			if (Input.touchCount > 0)
			{
				Rect position = new Rect(Input.GetTouch(0).position.x + 15f, (float)Screen.height - Input.GetTouch(0).position.y - 60f, 200f, 100f);
				Vector3 vector = pos;
				string text = vector.ToString();
				Vector2 vector2 = tPos;
				GUI.Label(position, "pos : " + text + "\ntPos: " + vector2.ToString());
			}
			GUI.matrix = matrix;
			GUI.Box(activeArea, GUIContent.none);
		}
	}
}
