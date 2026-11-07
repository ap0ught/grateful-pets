using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

[Serializable]
public abstract class OptionsDataRow : FrameworkDataElement
{
	public Func<bool> IsEnabled;

	public string CategoryId;

	public string Title;

	public string SearchWords;

	public string HelpText;

	protected HashSet<object> _observersSeen = new HashSet<object>();

	public bool ValueChangedSinceLastObserved(object obj)
	{
		if (_observersSeen.Contains(obj))
		{
			return false;
		}
		_observersSeen.Add(obj);
		return true;
	}

	protected void OnChange()
	{
		_observersSeen.Clear();
	}
}
public abstract class OptionsDataRow<T> : OptionsDataRow where T : IEquatable<T>
{
	public readonly GameOption Option;

	protected T _Value;

	protected T Initial;

	public T Value
	{
		get
		{
			return _Value;
		}
		set
		{
			ref T value2 = ref _Value;
			T other = value;
			if (value2.Equals(other))
			{
				return;
			}
			_Value = value;
			OnChange();
			if (Option.Restart)
			{
				if (value.Equals(Initial))
				{
					OptionsUI.RestartOptions.Remove(Option);
				}
				else
				{
					OptionsUI.RestartOptions.Add(Option);
				}
			}
		}
	}

	public OptionsDataRow(GameOption Option)
	{
		Id = Option.ID;
		this.Option = Option;
		Title = Option.DisplayText;
		CategoryId = Option.Category;
		SearchWords = Option.SearchKeywords + " " + Option.DisplayText + " " + Option.Category;
		IsEnabled = () =>
		{
			GameOption.RequiresSpec requires = Option.Requires;
			return (requires == null || requires.RequirementsMet) && Option.RequiresCapabilityMet();
		};
		HelpText = Option.HelpText;
	}

	protected void Initialize(T Value)
	{
		_Value = Value;
		Initial = Value;
	}
}
