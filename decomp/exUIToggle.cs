using System.Collections.Generic;
using UnityEngine;

public class exUIToggle : exUIButton
{
	public new static string[] eventNames = new string[2] { "onChecked", "onUnchecked" };

	private List<exUIEventListener> onChecked;

	private List<exUIEventListener> onUnchecked;

	[SerializeField]
	protected bool isChecked_;

	public bool isRadio;

	public bool isChecked
	{
		get
		{
			return isChecked_;
		}
		set
		{
			if (isChecked_ != value)
			{
				isChecked_ = value;
				if (!isChecked_)
				{
					exUIEvent exUIEvent2 = new exUIEvent();
					exUIEvent2.bubbles = false;
					OnUnchecked(exUIEvent2);
				}
				else
				{
					exUIEvent exUIEvent3 = new exUIEvent();
					exUIEvent3.bubbles = false;
					OnChecked(exUIEvent3);
				}
			}
		}
	}

	public void OnChecked(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onChecked", onChecked, _event);
	}

	public void OnUnchecked(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onUnchecked", onUnchecked, _event);
	}

	public override void CacheEventListeners()
	{
		base.CacheEventListeners();
		onChecked = eventListenerTable["onChecked"];
		onUnchecked = eventListenerTable["onUnchecked"];
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
		AddEventListener("onClick", (exUIEvent _event) =>
		{
			if (isRadio)
			{
				isChecked = true;
			}
			else
			{
				isChecked = !isChecked;
			}
		});
	}

	public void Internal_SetChecked(bool _checked)
	{
		isChecked_ = _checked;
	}
}
