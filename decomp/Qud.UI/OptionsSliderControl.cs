using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using UnityEngine;
using UnityEngine.UI;
using XRL;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

[ExecuteInEditMode]
public class OptionsSliderControl : MonoBehaviour, IFrameworkControl, IFrameworkControlSubcontexts
{
	private class Context : NavigationContext
	{
	}

	public NavigationContext editingContext;

	public FrameworkContext frameworkContext;

	public OptionsSliderRow data;

	public UITextSkin text;

	public UITextSkin valueLabel;

	public Slider slider;

	public Image sliderHandleImage;

	public int valueCache;

	private Context context;

	public static MenuOption CHANGE_VALUE = new MenuOption
	{
		InputCommand = "Accept",
		Description = "Change Value",
		disabled = true
	};

	public static MenuOption ARROWS_CHANGE_VALUE = new MenuOption
	{
		InputCommand = "NavigationXYAxis",
		Description = "Change Value",
		disabled = true
	};

	public static MenuOption SAVE_VALUE = new MenuOption
	{
		InputCommand = "Accept",
		Description = "Save",
		disabled = true
	};

	public static MenuOption CANCEL_VALUE = new MenuOption
	{
		InputCommand = "Cancel",
		Description = "Cancel",
		disabled = true
	};

	private bool initializing;

	public void SetupContexts(ScrollChildContext scontext)
	{
		if (this.context != null && this.context.IsActive() && this.context.parentContext is ScrollChildContext scrollChildContext && scrollChildContext.index != scontext.index)
		{
			this.context = new Context();
			if (editingContext.IsActive())
			{
				editingContext.parentContext.Activate();
			}
			editingContext = null;
		}
		else if (NavigationController.instance.activeContext is Context { parentContext: ScrollChildContext parentContext } context && parentContext.index == scontext.index)
		{
			this.context = context;
		}
		if (scontext != null)
		{
			scontext.proxyTo = this.context ?? (this.context = new Context());
			this.context.parentContext = scontext;
			this.context.menuOptionDescriptions = new List<MenuOption> { CHANGE_VALUE };
			Context context2 = this.context;
			if (context2.buttonHandlers == null)
			{
				context2.buttonHandlers = new Dictionary<InputButtonTypes, Action> { 
				{
					InputButtonTypes.AcceptButton,
					XRL.UI.Framework.Event.Helpers.Handle(() =>
					{
						valueCache = data.Value;
						editingContext.Activate();
					})
				} };
			}
		}
		if (editingContext == null)
		{
			editingContext = new NavigationContext
			{
				menuOptionDescriptions = new List<MenuOption> { SAVE_VALUE, CANCEL_VALUE, ARROWS_CHANGE_VALUE },
				parentContext = this.context,
				axisHandlers = new Dictionary<InputAxisTypes, Action>
				{
					{
						InputAxisTypes.NavigationXAxis,
						XRL.UI.Framework.Event.Helpers.Handle(XRL.UI.Framework.Event.Helpers.Axis(Increase, Decrease))
					},
					{
						InputAxisTypes.NavigationYAxis,
						XRL.UI.Framework.Event.Helpers.Handle(XRL.UI.Framework.Event.Helpers.Axis(Decrease5, Increase5))
					}
				},
				buttonHandlers = new Dictionary<InputButtonTypes, Action>
				{
					{
						InputButtonTypes.AcceptButton,
						XRL.UI.Framework.Event.Helpers.Handle(() =>
						{
							this.context.Activate();
						})
					},
					{
						InputButtonTypes.CancelButton,
						XRL.UI.Framework.Event.Helpers.Handle(() =>
						{
							SetValue(valueCache);
							this.context.Activate();
						})
					}
				},
				enterHandler = () =>
				{
					sliderHandleImage.color = The.Color.Yellow;
				},
				exitHandler = () =>
				{
					sliderHandleImage.color = ConsoleLib.Console.ColorUtility.FromWebColor("426470");
				}
			};
		}
	}

	public void SetValue(int value)
	{
		data.Value = Math.Min(data.Max, Math.Max(data.Min, value));
		slider.value = data.Value;
		valueLabel.SetText(data.Value.ToString());
		Options.SetOption(data.Id, Convert.ToString(data.Value));
	}

	public void Increase5()
	{
		SetValue(data.Value + data.Increment * 5);
	}

	public void Increase()
	{
		SetValue(data.Value + data.Increment);
	}

	public void Decrease()
	{
		SetValue(data.Value - data.Increment);
	}

	public void Decrease5()
	{
		SetValue(data.Value - data.Increment * 5);
	}

	public void OnSliderChanged(float value)
	{
		int num = (Convert.ToInt32(value) - data.Min) / data.Increment * data.Increment + data.Min;
		if (!initializing)
		{
			Options.SetOption(data.Id, Convert.ToString(num));
			data.Value = num;
			valueLabel.SetText(data.Value.ToString());
		}
		slider.value = num;
	}

	public NavigationContext GetNavigationContext()
	{
		return context;
	}

	public void setData(FrameworkDataElement data)
	{
		initializing = true;
		try
		{
			if (data is OptionsSliderRow optionsSliderRow)
			{
				this.data = optionsSliderRow;
				slider.minValue = optionsSliderRow.Min;
				slider.maxValue = optionsSliderRow.Max;
				slider.value = optionsSliderRow.Value;
				slider.wholeNumbers = true;
				valueLabel.SetText(optionsSliderRow.Value.ToString());
				Render();
			}
			else
			{
				this.data = null;
			}
		}
		finally
		{
			initializing = false;
		}
	}

	public void Render()
	{
		text.SetText(data.Title);
		data.ValueChangedSinceLastObserved(this);
	}

	public void Update()
	{
		OptionsSliderRow optionsSliderRow = data;
		if (optionsSliderRow != null && optionsSliderRow.ValueChangedSinceLastObserved(this))
		{
			Render();
		}
	}
}
