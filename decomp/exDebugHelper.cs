using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class exDebugHelper : MonoBehaviour
{
	public enum LogType
	{
		None,
		Normal,
		Warning,
		Error
	}

	public class TextInfo
	{
		public Vector2 screenPos = Vector2.zero;

		public string text;

		public GUIStyle style;

		public TextInfo(Vector2 _screenPos, string _text, GUIStyle _style)
		{
			screenPos = _screenPos;
			text = _text;
			style = _style;
		}
	}

	public class LogInfo
	{
		public string text;

		public GUIStyle style;

		private float speed = 1f;

		private float timer;

		private float lifetime = 5f;

		public float ratio
		{
			get
			{
				if (lifetime == 0f)
				{
					return 0f;
				}
				if (!(timer >= lifetime - instance.logFadeOutDuration))
				{
					return 0f;
				}
				return (timer - (lifetime - instance.logFadeOutDuration)) / instance.logFadeOutDuration;
			}
		}

		public bool canDelete => timer > lifetime;

		public LogInfo(string _text, GUIStyle _style, float _lifetime)
		{
			text = _text;
			style = _style;
			lifetime = _lifetime;
		}

		public void Dead()
		{
			if (lifetime > 0f)
			{
				float num = lifetime - instance.logFadeOutDuration;
				if (timer < num - 1f)
				{
					timer = num - 1f;
				}
			}
		}

		public void Tick()
		{
			if (lifetime > 0f)
			{
				timer += Time.deltaTime * speed;
			}
		}
	}

	public static exDebugHelper instance;

	public Vector2 offset = new Vector2(10f, 10f);

	public GUIStyle printStyle;

	public GUIStyle fpsStyle;

	public GUIStyle logStyle;

	public GUIStyle timeScaleStyle;

	protected string txtPrint = "screen print: ";

	protected string txtFPS = "fps: ";

	protected List<TextInfo> debugTextPool = new List<TextInfo>();

	private float logFadeOutDuration = 0.3f;

	protected List<LogInfo> logs = new List<LogInfo>();

	protected CleanQueue<LogInfo> pendingLogs = new CleanQueue<LogInfo>();

	[SerializeField]
	protected bool showFps_ = true;

	public TextAnchor fpsAnchor;

	[SerializeField]
	protected bool enableTimeScaleDebug_ = true;

	[SerializeField]
	protected bool showScreenPrint_ = true;

	[SerializeField]
	protected bool showScreenLog_ = true;

	public int logCount = 10;

	public bool showScreenDebugText;

	protected int frames;

	protected float fps;

	protected float lastInterval;

	public bool showFps
	{
		get
		{
			return showFps_;
		}
		set
		{
			if (showFps_ != value)
			{
				showFps_ = value;
			}
		}
	}

	public bool enableTimeScaleDebug
	{
		get
		{
			return enableTimeScaleDebug_;
		}
		set
		{
			if (enableTimeScaleDebug_ != value)
			{
				enableTimeScaleDebug_ = value;
			}
		}
	}

	public bool showScreenPrint
	{
		get
		{
			return showScreenPrint_;
		}
		set
		{
			if (showScreenPrint_ != value)
			{
				showScreenPrint_ = value;
			}
		}
	}

	public bool showScreenLog
	{
		get
		{
			return showScreenLog_;
		}
		set
		{
			if (showScreenLog_ != value)
			{
				showScreenLog_ = value;
			}
		}
	}

	public static void ScreenPrint(string _text)
	{
		if (instance.showScreenPrint_)
		{
			instance.txtPrint = instance.txtPrint + _text + "\n";
		}
	}

	public static void ScreenPrint(Vector2 _pos, string _text, GUIStyle _style = null)
	{
		if (instance.showScreenDebugText)
		{
			TextInfo item = new TextInfo(_pos, _text, _style);
			instance.debugTextPool.Add(item);
		}
	}

	public static void ScreenLog(string _text, LogType _logType = LogType.None, GUIStyle _style = null, bool autoFadeOut = true)
	{
		LogInfo item = new LogInfo(_text, _style, autoFadeOut ? 5f : 0f);
		instance.pendingLogs.Enqueue(item);
		switch (_logType)
		{
		case LogType.Normal:
			Debug.Log(_text);
			break;
		case LogType.Warning:
			Debug.LogWarning(_text);
			break;
		case LogType.Error:
			Debug.LogError(_text);
			break;
		}
	}

	public static void SetFPSColor(Color _color)
	{
		instance.fpsStyle.normal.textColor = _color;
	}

	public static float GetFPS()
	{
		return instance.fps;
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		txtPrint = "";
		txtFPS = "";
		if (showScreenDebugText)
		{
			debugTextPool.Clear();
		}
		useGUILayout = false;
	}

	private void Start()
	{
		InvokeRepeating("UpdateFPS", 0f, 1f);
	}

	private void Update()
	{
		frames++;
		UpdateTimeScale();
		UpdateLog();
		StartCoroutine(CleanDebugText());
	}

	private void OnGUI()
	{
		GUIContent gUIContent = null;
		Vector2 zero = Vector2.zero;
		float num = offset.x;
		float num2 = offset.y;
		if (showFps)
		{
			gUIContent = new GUIContent(txtFPS);
			zero = fpsStyle.CalcSize(gUIContent);
			switch (fpsAnchor)
			{
			case TextAnchor.UpperCenter:
				num += ((float)Screen.width - zero.x) * 0.5f;
				break;
			case TextAnchor.UpperRight:
				num = (float)Screen.width - zero.x - num;
				break;
			case TextAnchor.MiddleLeft:
				num2 += ((float)Screen.height - zero.y) * 0.5f;
				break;
			case TextAnchor.MiddleCenter:
				num += ((float)Screen.width - zero.x) * 0.5f;
				num2 += ((float)Screen.height - zero.y) * 0.5f;
				break;
			case TextAnchor.MiddleRight:
				num = (float)Screen.width - zero.x - num;
				num2 += ((float)Screen.height - zero.y) * 0.5f;
				break;
			case TextAnchor.LowerLeft:
				num2 = (float)Screen.height - zero.y - num2;
				break;
			case TextAnchor.LowerCenter:
				num += ((float)Screen.width - zero.x) * 0.5f;
				num2 = (float)Screen.height - zero.y - num2;
				break;
			case TextAnchor.LowerRight:
				num = (float)Screen.width - zero.x - num;
				num2 = (float)Screen.height - zero.y - num2;
				break;
			}
			GUI.Label(new Rect(num, num2, zero.x, zero.y), txtFPS, fpsStyle);
			num = 10f;
			num2 = 10f + zero.y;
		}
		if (enableTimeScaleDebug)
		{
			string text = "TimeScale = " + Time.timeScale.ToString("f2");
			gUIContent = new GUIContent(text);
			zero = timeScaleStyle.CalcSize(gUIContent);
			GUI.Label(new Rect(num, num2, zero.x, zero.y), text, timeScaleStyle);
			num2 += zero.y;
		}
		if (showScreenPrint)
		{
			gUIContent = new GUIContent(txtPrint);
			zero = printStyle.CalcSize(gUIContent);
			GUI.Label(new Rect(num, num2, zero.x, zero.y), txtPrint, printStyle);
		}
		if (showScreenLog)
		{
			bool flag = logStyle.alignment == TextAnchor.LowerLeft || logStyle.alignment == TextAnchor.LowerCenter || logStyle.alignment == TextAnchor.LowerRight;
			bool flag2 = logStyle.alignment == TextAnchor.LowerRight || logStyle.alignment == TextAnchor.MiddleRight || logStyle.alignment == TextAnchor.UpperRight;
			float num3 = ((!flag) ? 50f : ((float)(Screen.height - 10)));
			for (int num4 = logs.Count - 1; num4 >= 0; num4--)
			{
				LogInfo logInfo = logs[num4];
				gUIContent = new GUIContent(logInfo.text);
				GUIStyle gUIStyle = ((logInfo.style == null) ? logStyle : logInfo.style);
				zero = gUIStyle.CalcSize(gUIContent);
				gUIStyle.normal.textColor = new Color(gUIStyle.normal.textColor.r, gUIStyle.normal.textColor.g, gUIStyle.normal.textColor.b, 1f - logInfo.ratio);
				num3 = ((!flag) ? (num3 + zero.y) : (num3 - zero.y));
				if (flag2)
				{
					GUI.Label(new Rect((float)Screen.width - 10f - zero.x, num3, zero.x, zero.y), logInfo.text, gUIStyle);
				}
				else
				{
					GUI.Label(new Rect(10f, num3, zero.x, zero.y), logInfo.text, gUIStyle);
				}
			}
		}
		if (showScreenDebugText)
		{
			for (int i = 0; i < debugTextPool.Count; i++)
			{
				TextInfo textInfo = debugTextPool[i];
				gUIContent = new GUIContent(textInfo.text);
				GUIStyle gUIStyle2 = ((textInfo.style == null) ? GUI.skin.label : textInfo.style);
				zero = gUIStyle2.CalcSize(gUIContent);
				Vector2 vector = new Vector2(textInfo.screenPos.x, (float)Screen.height - textInfo.screenPos.y) - zero * 0.5f;
				GUI.Label(new Rect(vector.x, vector.y, zero.x, zero.y), textInfo.text, gUIStyle2);
			}
		}
	}

	private void UpdateFPS()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		fps = (float)frames / (realtimeSinceStartup - lastInterval);
		frames = 0;
		lastInterval = realtimeSinceStartup;
		txtFPS = "fps: " + fps.ToString("f2");
	}

	private void UpdateTimeScale()
	{
		if (enableTimeScaleDebug)
		{
			if (Input.GetKey(KeyCode.Minus))
			{
				Time.timeScale = Mathf.Max(Time.timeScale - 0.01f, 0f);
			}
			else if (Input.GetKey(KeyCode.Equals))
			{
				Time.timeScale = Mathf.Min(Time.timeScale + 0.01f, 10f);
			}
			if (Input.GetKey(KeyCode.Alpha0))
			{
				Time.timeScale = 0f;
			}
			else if (Input.GetKey(KeyCode.Alpha9))
			{
				Time.timeScale = 1f;
			}
		}
	}

	private IEnumerator CleanDebugText()
	{
		yield return new WaitForEndOfFrame();
		txtPrint = "";
		if (showScreenDebugText)
		{
			debugTextPool.Clear();
		}
	}

	private void UpdateLog()
	{
		for (int num = logs.Count - 1; num >= 0; num--)
		{
			LogInfo logInfo = logs[num];
			logInfo.Tick();
			if (logInfo.canDelete)
			{
				logs.RemoveAt(num);
			}
		}
		bool flag = logStyle.alignment == TextAnchor.LowerLeft || logStyle.alignment == TextAnchor.LowerCenter || logStyle.alignment == TextAnchor.LowerRight;
		if (pendingLogs.Count <= 0)
		{
			return;
		}
		int num2 = Mathf.CeilToInt(pendingLogs.Count / 2);
		do
		{
			if (flag)
			{
				logs.Add(pendingLogs.Dequeue());
			}
			else
			{
				logs.Insert(0, pendingLogs.Dequeue());
			}
			num2--;
			if (instance.logs.Count > instance.logCount)
			{
				for (int i = 0; i < instance.logs.Count - instance.logCount; i++)
				{
					instance.logs[i].Dead();
				}
			}
		}
		while (num2 > 0);
	}

	public static void ClearScreen()
	{
		instance.pendingLogs.Clear();
		instance.logs.Clear();
	}
}
