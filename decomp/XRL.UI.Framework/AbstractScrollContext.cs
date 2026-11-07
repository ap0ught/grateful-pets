using System;
using System.Collections.Generic;
using System.Linq;

namespace XRL.UI.Framework;

public class AbstractScrollContext : NavigationContext
{
	public InputAxisTypes AxisType;

	public bool DualAxis;

	public Func<IEnumerable<AbstractScrollContext>> gridSiblings;

	public bool wraps = true;

	public virtual int selectedPosition { get; set; }

	public virtual int selectedRow
	{
		get
		{
			if (!DualAxis)
			{
				return selectedPosition;
			}
			return selectedPosition / rowWidth;
		}
	}

	public virtual int selectedColumn
	{
		get
		{
			if (!DualAxis)
			{
				return selectedPosition;
			}
			return selectedPosition % rowWidth;
		}
	}

	public virtual int length { get; } = -1;

	public virtual int rowWidth { get; } = -1;

	public override bool disabled
	{
		get
		{
			if (!base.disabled && length != 0)
			{
				NavigationContext contextAt = GetContextAt(selectedPosition);
				if (contextAt != null && contextAt.disabled)
				{
					return !NextEnabledContext(1).HasValue;
				}
				return false;
			}
			return true;
		}
		set
		{
			base.disabled = value;
		}
	}

	public InputAxisTypes SecondaryAxisType
	{
		get
		{
			if (AxisType == InputAxisTypes.NavigationXAxis)
			{
				return InputAxisTypes.NavigationYAxis;
			}
			if (AxisType == InputAxisTypes.NavigationYAxis)
			{
				return InputAxisTypes.NavigationXAxis;
			}
			if (AxisType == InputAxisTypes.NavigationPageXAxis)
			{
				return InputAxisTypes.NavigationPageYAxis;
			}
			if (AxisType == InputAxisTypes.NavigationPageYAxis)
			{
				return InputAxisTypes.NavigationPageXAxis;
			}
			if (AxisType == InputAxisTypes.NavigationUAxis)
			{
				return InputAxisTypes.NavigationVAxis;
			}
			if (AxisType == InputAxisTypes.NavigationVAxis)
			{
				return InputAxisTypes.NavigationUAxis;
			}
			throw new ArgumentOutOfRangeException("Unknown Axis Type to oppose");
		}
	}

	public override bool hasChildren
	{
		get
		{
			if (length > 0)
			{
				return true;
			}
			return base.hasChildren;
		}
	}

	public virtual NavigationContext GetContextAt(int index)
	{
		return null;
	}

	public override void Setup()
	{
		for (int i = 0; i < length; i++)
		{
			NavigationContext contextAt = GetContextAt(i);
			if (contextAt != null)
			{
				contextAt.parentContext = this;
				contextAt.Setup();
			}
		}
		base.Setup();
	}

	public override void OnEnter()
	{
		NavigationContext navigationContext = (NavigationContext)currentEvent.data["to"];
		Event triggeringEvent = NavigationController.instance.triggeringEvent;
		bool flag = false;
		try
		{
			flag = triggeringEvent?.data["axis"] as InputAxisTypes? == AxisType;
		}
		catch (Exception)
		{
		}
		if (navigationContext == this)
		{
			NavigationContext navigationContext2 = (NavigationContext)currentEvent.data["from"];
			IEnumerable<NavigationContext> enumerable = navigationContext2?.parents.Prepend(navigationContext2).TakeWhile((NavigationContext c) => !parents.Prepend(this).Contains(c));
			AbstractScrollContext abstractScrollContext = ((enumerable != null) ? enumerable.FirstOrDefault((NavigationContext c) => c is AbstractScrollContext) : null) as AbstractScrollContext;
			if (triggeringEvent != null && triggeringEvent.axisValue.HasValue && rowWidth > 0 && abstractScrollContext != null && abstractScrollContext.AxisType == AxisType)
			{
				if (flag)
				{
					if (triggeringEvent != null && triggeringEvent.axisValue > 0)
					{
						int val = (length - 1) / rowWidth;
						SelectIndex(Math.Min(length - 1, rowWidth * Math.Min(val, abstractScrollContext.selectedRow)));
					}
					else if (triggeringEvent != null && triggeringEvent.axisValue < 0)
					{
						int num = Math.Min((length - 1) / rowWidth, abstractScrollContext.selectedRow);
						SelectIndex(Math.Min(length - 1, rowWidth * (num + 1) - 1));
					}
				}
				else if (triggeringEvent != null && triggeringEvent.axisValue > 0)
				{
					int val2 = abstractScrollContext.selectedColumn;
					SelectIndex(Math.Min(length - 1, val2));
				}
				else if (triggeringEvent != null && triggeringEvent.axisValue < 0)
				{
					int num2;
					for (num2 = abstractScrollContext.selectedColumn; num2 + rowWidth < length; num2 += rowWidth)
					{
					}
					SelectIndex(Math.Min(length - 1, num2));
				}
			}
			else if (flag && triggeringEvent != null && triggeringEvent.axisValue == 1)
			{
				SelectIndex(0);
			}
			else if (flag && triggeringEvent != null && triggeringEvent.axisValue == -1)
			{
				SelectIndex(length - 1);
			}
			else
			{
				SelectIndex(Math.Min(Math.Max(0, selectedPosition), length - 1));
			}
			currentEvent.Cancel();
		}
		else
		{
			int? num3 = ContextIndex(navigationContext);
			if (num3.HasValue)
			{
				int valueOrDefault = num3.GetValueOrDefault();
				selectedPosition = valueOrDefault;
				UpdateGridSiblings();
			}
		}
		base.OnEnter();
	}

	public virtual int? ContextIndex(NavigationContext to)
	{
		for (int i = 0; i < length; i++)
		{
			if (to.IsInside(GetContextAt(i)))
			{
				return i;
			}
		}
		return null;
	}

	public void UpdateGridSiblings()
	{
		if (gridSiblings == null)
		{
			return;
		}
		foreach (AbstractScrollContext item in gridSiblings())
		{
			item.selectedPosition = selectedPosition;
		}
	}

	public virtual void SelectIndex(int index)
	{
		selectedPosition = index;
		NavigationContext contextAt = GetContextAt(selectedPosition);
		if (contextAt is AbstractScrollContext abstractScrollContext && abstractScrollContext.AxisType == AxisType)
		{
			if (currentEvent.axisValue > 0)
			{
				abstractScrollContext.selectedPosition = 0;
			}
			else if (currentEvent.axisValue < 0)
			{
				abstractScrollContext.selectedPosition = abstractScrollContext.length - 1;
			}
		}
		contextAt?.Activate();
	}

	public virtual int? NextEnabledContext(int direction)
	{
		int position = selectedPosition + direction;
		bool secondaryAxis;
		if (rowWidth > 0)
		{
			if (wraps)
			{
				throw new Exception("Haven't implemented wrapping for grids... sorry...");
			}
			secondaryAxis = Math.Abs(direction) == rowWidth;
			for (; position != selectedPosition; position += direction)
			{
				if (position >= length || position < 0 || !otherAxisSame())
				{
					return null;
				}
				if (!GetContextAt(position).disabled)
				{
					return position;
				}
			}
			return position;
		}
		for (; position != selectedPosition; position += direction)
		{
			if (position >= length)
			{
				if (!wraps)
				{
					return null;
				}
				position -= length;
			}
			if (position < 0)
			{
				if (!wraps)
				{
					return null;
				}
				position += length;
			}
			if (!GetContextAt(position).disabled)
			{
				return position;
			}
			if (position == selectedPosition)
			{
				break;
			}
		}
		return position;
		bool otherAxisSame()
		{
			if (!secondaryAxis)
			{
				return position / rowWidth == selectedRow;
			}
			return position % rowWidth == selectedColumn;
		}
	}

	public virtual void Next(int magnitude)
	{
		int? num = NextEnabledContext(magnitude);
		if (num.HasValue)
		{
			currentEvent.Handle();
			SelectIndex(num.Value);
		}
	}

	public virtual void Prev(int magnitude)
	{
		int? num = NextEnabledContext(-magnitude);
		if (num.HasValue)
		{
			currentEvent.Handle();
			SelectIndex(num.Value);
		}
	}

	public virtual void HandleAxisEvent()
	{
		if (currentEvent.axisValue.HasValue)
		{
			if (currentEvent.axisValue < 0)
			{
				Prev(1);
			}
			else if (currentEvent.axisValue > 0)
			{
				Next(1);
			}
		}
	}

	public virtual void HandleSecondaryAxisEvent()
	{
		if (currentEvent.axisValue.HasValue)
		{
			if (currentEvent.axisValue < 0)
			{
				Prev(rowWidth);
			}
			else if (currentEvent.axisValue > 0)
			{
				Next(rowWidth);
			}
		}
	}

	public virtual void SetAxis(InputAxisTypes axis, bool dual = false)
	{
		AxisType = axis;
		DualAxis = dual;
		axisHandlers = new Dictionary<InputAxisTypes, Action> { { axis, HandleAxisEvent } };
		if (dual)
		{
			axisHandlers.Add(SecondaryAxisType, HandleSecondaryAxisEvent);
		}
	}
}
