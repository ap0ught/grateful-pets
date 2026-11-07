using System.Collections.Generic;
using ConsoleLib.Console;
using Qud.UI;
using UnityEngine;

namespace QupKit;

public class LegacyViewManager
{
	public static LegacyViewManager Instance;

	public Dictionary<GameObject, BaseView> ViewsByCanvas = new Dictionary<GameObject, BaseView>();

	public Dictionary<string, BaseView> Views = new Dictionary<string, BaseView>();

	[HideInInspector]
	private Canvas _MainCanvas;

	public List<BaseView> overlappedViews = new List<BaseView>();

	public BaseView ActiveView;

	[HideInInspector]
	public Canvas MainCanvas
	{
		get
		{
			if (_MainCanvas == null)
			{
				_MainCanvas = GameObject.Find("Legacy Main Canvas").GetComponent<Canvas>();
			}
			return _MainCanvas;
		}
	}

	public void Create()
	{
		Instance = this;
		foreach (string key in Views.Keys)
		{
			Views[key].OnCreate();
		}
	}

	public void RegisterRemainingViews(List<GameObject> ViewCanvases)
	{
		foreach (GameObject ViewCanvase in ViewCanvases)
		{
			if (ViewCanvase != null && !ViewsByCanvas.ContainsKey(ViewCanvase))
			{
				BaseView baseView = new BaseView();
				baseView.AttachTo(ViewCanvase);
				AddView(ViewCanvase.name, baseView);
			}
		}
	}

	public void OnCommand(string Command)
	{
		if (ActiveView != null)
		{
			ActiveView.OnCommand(Command);
		}
	}

	public void Awake()
	{
		Instance = this;
	}

	public void Update()
	{
		if (ActiveView != null)
		{
			ActiveView.Update();
		}
	}

	public void AddView(string ID, BaseView V)
	{
		V.views = this;
		Views.Add(ID, V);
		if (V.rootObject == null)
		{
			GameObject gameObject = new GameObject();
			CanvasGroup canvasGroup = gameObject.AddComponent<CanvasGroup>();
			gameObject.AddComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
			gameObject.name = ID;
			canvasGroup.alpha = 1f;
			canvasGroup.interactable = true;
			canvasGroup.blocksRaycasts = true;
			gameObject.transform.SetParent(MainCanvas.transform, worldPositionStays: false);
			V.rootObject = gameObject;
			V.canvasGroup = canvasGroup;
			V.Layout = new ControlLayout(ControlAnchor.Fill);
		}
		ViewsByCanvas.Add(V.rootObject, V);
		V.rootObject.transform.SetParent(MainCanvas.gameObject.transform);
		V.rootObject.SetActive(value: false);
	}

	public void OnGUI()
	{
		if (ActiveView != null && ActiveView != null)
		{
			ActiveView.OnGUI();
		}
	}

	public BaseView GetView(string ID)
	{
		return Views[ID];
	}

	public void SetActiveView(string ID, bool bHideOldView = true, bool bForceEnter = false)
	{
		if (ID == null)
		{
			foreach (BaseView overlappedView in overlappedViews)
			{
				overlappedView.Leave();
			}
			overlappedViews.Clear();
		}
		else
		{
			overlappedViews.RemoveAll((BaseView e) => e?.Name == ID);
		}
		if (!bForceEnter && ActiveView != null && ActiveView.Name == ID)
		{
			ActiveView.EnableInput();
			return;
		}
		Keyboard.ClearInput();
		if (ActiveView != null)
		{
			if (bHideOldView || ActiveView.Name.StartsWith("Popup:"))
			{
				ActiveView.Leave();
			}
			else
			{
				ActiveView.Overlapped();
				overlappedViews.Add(ActiveView);
			}
			ActiveView.DisableInput();
		}
		if (ID != null)
		{
			if (ID == "ModToolkit")
			{
				UIManager.showWindow("ModToolkit");
				ActiveView = null;
				return;
			}
			if (!Views.ContainsKey(ID))
			{
				Debug.LogError("Unknown view ID:" + ID);
			}
			ActiveView = Views[ID];
			ActiveView.Enter();
			ActiveView.EnableInput();
		}
		else
		{
			ActiveView = null;
		}
	}
}
