using System.Collections.Generic;
using UnityEngine;

public class exUIToggleGroup : exUIToggle
{
	public new static string[] eventNames = new string[1] { "onCheckChanged" };

	private List<exUIEventListener> onCheckChanged;

	[SerializeField]
	protected int index_;

	public List<exUIToggle> toggles = new List<exUIToggle>();

	public int index
	{
		get
		{
			return index_;
		}
		set
		{
			if (index_ == value)
			{
				return;
			}
			index_ = value;
			if (index_ < 0)
			{
				index_ = 0;
			}
			if (index_ > toggles.Count - 1)
			{
				index_ = toggles.Count - 1;
			}
			for (int i = 0; i < toggles.Count; i++)
			{
				exUIToggle exUIToggle2 = toggles[i];
				if (i == index_)
				{
					exUIToggle2.isChecked = true;
				}
				else
				{
					exUIToggle2.isChecked = false;
				}
			}
			exUIEvent exUIEvent2 = new exUIEvent();
			exUIEvent2.bubbles = false;
			OnCheckChanged(exUIEvent2);
		}
	}

	public void OnCheckChanged(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onCheckChanged", onCheckChanged, _event);
	}

	public override void CacheEventListeners()
	{
		base.CacheEventListeners();
		onCheckChanged = eventListenerTable["onCheckChanged"];
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
		for (int i = 0; i < toggles.Count; i++)
		{
			RegisterEvent(toggles[i]);
		}
		for (int j = 0; j < toggles.Count; j++)
		{
			exUIToggle exUIToggle2 = toggles[j];
			exUIToggle2.isRadio = true;
			if (j == index_)
			{
				exUIToggle2.Internal_SetChecked(_checked: true);
			}
			else
			{
				exUIToggle2.Internal_SetChecked(_checked: false);
			}
		}
	}

	public void AddToggle(exUIToggle _toggle)
	{
		toggles.Add(_toggle);
		RegisterEvent(_toggle);
	}

	private void RegisterEvent(exUIToggle _toggle)
	{
		_toggle.AddEventListener("onChecked", (exUIEvent _event) =>
		{
			index = toggles.IndexOf(_toggle);
		});
	}
}
