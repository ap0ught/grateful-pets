using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XRL.UI;

namespace Qud.UI;

public class ControlledInputField : InputField, IControlledInputField, IControlledSelectable
{
	protected class ScrollViewCalcs
	{
		public ScrollRect scroller;

		public float contentHeight;

		public float scrollHeight;

		public float contentTop;

		public float contentBottom;

		public float scrollTop;

		public float scrollBottom;

		public float scrollPercent;
	}

	public QudBaseMenuController controller;

	public object data;

	private RectTransform _rt;

	private bool? wasSelected;

	private string oldItemText;

	private float _debouceClick;

	private ScrollViewCalcs _sv = new ScrollViewCalcs();

	public bool selected
	{
		get
		{
			return wasSelected == true;
		}
		set
		{
			if (value != wasSelected)
			{
				if (value)
				{
					Select();
					ActivateInputField();
				}
				else
				{
					InstantClearState();
				}
			}
		}
	}

	public bool IsSelected()
	{
		return selected;
	}

	public override void OnSelect(BaseEventData eventData)
	{
		if (controller != null)
		{
			controller.SetSelectedInput(this, controller.lastEventSource);
		}
		wasSelected = true;
		SelectChanged(newState: true);
		base.OnSelect(eventData);
	}

	public virtual void Update()
	{
		_debouceClick -= Time.deltaTime;
		bool flag = isFocused;
		if (wasSelected != flag)
		{
			wasSelected = flag;
			SelectChanged(flag);
		}
	}

	public virtual void SelectChanged(bool newState)
	{
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		Select();
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		if (!(_debouceClick > 0f))
		{
			base.OnPointerUp(eventData);
			if ((bool)controller)
			{
				controller.ActivateInput(this);
			}
			_debouceClick = 0.5f;
		}
	}

	public void Init()
	{
		wasSelected = false;
	}

	public void UpdateValue(string value)
	{
		controller.ActivateInput(this, QudBaseMenuController.MenuEventPipeline.KeyboardOrJoystick);
	}

	protected ScrollViewCalcs GetScrollViewCalcs(ScrollViewCalcs reuse)
	{
		if (_rt == null)
		{
			_rt = gameObject.GetComponent<RectTransform>();
		}
		ScrollViewCalcs scrollViewCalcs = reuse ?? new ScrollViewCalcs();
		if (scrollViewCalcs.scroller == null)
		{
			scrollViewCalcs.scroller = gameObject.GetComponentInParent<ScrollRect>();
		}
		if (scrollViewCalcs.scroller != null && scrollViewCalcs.scroller.vertical)
		{
			Vector3[] array = new Vector3[4];
			Vector3[] array2 = new Vector3[4];
			scrollViewCalcs.scroller.content.GetWorldCorners(array);
			_rt.GetWorldCorners(array2);
			scrollViewCalcs.contentHeight = scrollViewCalcs.scroller.content.rect.height * (float)Options.StageScale;
			scrollViewCalcs.scrollHeight = scrollViewCalcs.scroller.viewport.rect.height * (float)Options.StageScale;
			scrollViewCalcs.contentTop = array[2].y - array2[2].y;
			scrollViewCalcs.contentBottom = array[2].y - array2[0].y;
			scrollViewCalcs.scrollPercent = scrollViewCalcs.scroller.verticalNormalizedPosition;
			scrollViewCalcs.scrollTop = (1f - scrollViewCalcs.scrollPercent) * (scrollViewCalcs.contentHeight - scrollViewCalcs.scrollHeight);
			scrollViewCalcs.scrollBottom = scrollViewCalcs.scrollTop + scrollViewCalcs.scrollHeight;
		}
		return scrollViewCalcs;
	}

	public bool IsInFullView()
	{
		GetScrollViewCalcs(_sv);
		bool flag = _sv.scrollTop > _sv.contentTop;
		bool flag2 = _sv.scrollBottom < _sv.contentBottom;
		return !(flag | flag2);
	}

	public bool IsInView()
	{
		GetScrollViewCalcs(_sv);
		bool flag = _sv.scrollTop > _sv.contentBottom;
		bool flag2 = _sv.scrollBottom < _sv.contentTop;
		return !(flag | flag2);
	}
}
