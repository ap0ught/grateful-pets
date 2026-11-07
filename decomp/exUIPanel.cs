using System.Collections.Generic;

public class exUIPanel : exUIControl
{
	public new static string[] eventNames = new string[8] { "onEnter", "onExit", "onStartFadeIn", "onFinishFadeIn", "onFadeIn", "onStartFadeOut", "onFinishFadeOut", "onFadeOut" };

	private List<exUIEventListener> onEnter;

	private List<exUIEventListener> onExit;

	private List<exUIEventListener> onStartFadeIn;

	private List<exUIEventListener> onFinishFadeIn;

	private List<exUIEventListener> onFadeIn;

	private List<exUIEventListener> onStartFadeOut;

	private List<exUIEventListener> onFinishFadeOut;

	private List<exUIEventListener> onFadeOut;

	public void OnEnter(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onEnter", onEnter, _event);
	}

	public void OnExit(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onExit", onExit, _event);
	}

	public void OnStartFadeIn(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onStartFadeIn", onStartFadeIn, _event);
	}

	public void OnFinishFadeIn(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onFinishFadeIn", onFinishFadeIn, _event);
	}

	public void OnFadeIn(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onFadeIn", onFadeIn, _event);
	}

	public void OnStartFadeOut(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onStartFadeOut", onStartFadeOut, _event);
	}

	public void OnFinishFadeOut(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onFinishFadeOut", onFinishFadeOut, _event);
	}

	public void OnFadeOut(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onFadeOut", onFadeOut, _event);
	}

	public override void CacheEventListeners()
	{
		base.CacheEventListeners();
		onEnter = eventListenerTable["onEnter"];
		onExit = eventListenerTable["onExit"];
		onStartFadeIn = eventListenerTable["onStartFadeIn"];
		onFinishFadeIn = eventListenerTable["onFinishFadeIn"];
		onFadeIn = eventListenerTable["onFadeIn"];
		onStartFadeOut = eventListenerTable["onStartFadeOut"];
		onFinishFadeOut = eventListenerTable["onFinishFadeOut"];
		onFadeOut = eventListenerTable["onFadeOut"];
	}

	public override string[] GetEventNames()
	{
		string[] array = base.GetEventNames();
		string[] array2 = new string[array.Length + eventNames.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = array[i];
		}
		for (int j = 0; j < eventNames.Length; j++)
		{
			array2[j + array.Length] = eventNames[j];
		}
		return array2;
	}

	protected new void Awake()
	{
		base.Awake();
	}

	public void Enter()
	{
		exUIEvent exUIEvent2 = new exUIEvent();
		exUIEvent2.bubbles = false;
		OnEnter(exUIEvent2);
	}

	public void Exit()
	{
		exUIEvent exUIEvent2 = new exUIEvent();
		exUIEvent2.bubbles = false;
		OnExit(exUIEvent2);
	}

	public void StartFadeIn()
	{
		exUIEvent exUIEvent2 = new exUIEvent();
		exUIEvent2.bubbles = false;
		OnStartFadeIn(exUIEvent2);
	}

	public void FinishFadeIn()
	{
		exUIEvent exUIEvent2 = new exUIEvent();
		exUIEvent2.bubbles = false;
		OnFinishFadeIn(exUIEvent2);
	}

	public void FadeIn(float _ratio)
	{
		exUIRatioEvent exUIRatioEvent2 = new exUIRatioEvent();
		exUIRatioEvent2.bubbles = false;
		exUIRatioEvent2.ratio = _ratio;
		OnFadeIn(exUIRatioEvent2);
	}

	public void StartFadeOut()
	{
		exUIEvent exUIEvent2 = new exUIEvent();
		exUIEvent2.bubbles = false;
		OnStartFadeOut(exUIEvent2);
	}

	public void FinishFadeOut()
	{
		exUIEvent exUIEvent2 = new exUIEvent();
		exUIEvent2.bubbles = false;
		OnFinishFadeOut(exUIEvent2);
	}

	public void FadeOut(float _ratio)
	{
		exUIRatioEvent exUIRatioEvent2 = new exUIRatioEvent();
		exUIRatioEvent2.bubbles = false;
		exUIRatioEvent2.ratio = _ratio;
		OnFadeOut(exUIRatioEvent2);
	}
}
