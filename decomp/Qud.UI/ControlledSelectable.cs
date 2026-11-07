using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XRL.UI;

namespace Qud.UI;

public abstract class ControlledSelectable : Selectable, IControlledSelectable
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

	private ScrollViewCalcs _sv = new ScrollViewCalcs();

	private float _debouceClick;

	public bool selected
	{
		get
		{
			return wasSelected == true;
		}
		set
		{
			Deselect();
			if (value)
			{
				Select();
			}
		}
	}

	public void Deselect()
	{
		if (EventSystem.current.currentSelectedGameObject == gameObject)
		{
			PointerEventData eventData = new PointerEventData(EventSystem.current);
			ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, eventData, ExecuteEvents.pointerExitHandler);
			EventSystem.current.SetSelectedGameObject(null);
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
			controller.SetSelected(this, controller.lastEventSource);
		}
		wasSelected = true;
		SelectChanged(newState: true);
		base.OnSelect(eventData);
	}

	public virtual void Update()
	{
		_debouceClick -= Time.deltaTime;
		bool flag = IsPressed() || currentSelectionState == SelectionState.Selected;
		if (wasSelected != flag)
		{
			wasSelected = flag;
			SelectChanged(flag);
		}
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

	public virtual void SelectChanged(bool newState)
	{
		if (_rt == null)
		{
			_rt = GetComponent<RectTransform>();
		}
		if (!newState)
		{
			return;
		}
		ScrollRect componentInParent = gameObject.GetComponentInParent<ScrollRect>();
		if (!(componentInParent != null) || !componentInParent.vertical)
		{
			return;
		}
		Vector3[] array = new Vector3[4];
		Vector3[] array2 = new Vector3[4];
		componentInParent.content.GetWorldCorners(array);
		_rt.GetWorldCorners(array2);
		float num = componentInParent.content.rect.height * (float)Options.StageScale;
		float num2 = componentInParent.viewport.rect.height * (float)Options.StageScale;
		if (num2 < num)
		{
			float num3 = array[2].y - array2[2].y;
			float num4 = array[2].y - array2[0].y;
			float verticalNormalizedPosition = componentInParent.verticalNormalizedPosition;
			float num5 = (1f - verticalNormalizedPosition) * (num - num2);
			float num6 = num5 + num2;
			bool flag = num5 > num3;
			bool flag2 = num6 < num4;
			if (flag)
			{
				componentInParent.verticalNormalizedPosition = 1f - num3 / (num - num2);
			}
			else if (flag2)
			{
				componentInParent.verticalNormalizedPosition = 1f - (num4 - num2) / (num - num2);
			}
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		if (eventData.IsPointerMoving())
		{
			Select();
		}
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		if (!(_debouceClick > 0f))
		{
			base.OnPointerUp(eventData);
			if ((bool)controller)
			{
				controller.Activate(this);
			}
			_debouceClick = 0.5f;
		}
	}

	public virtual void UpdateData()
	{
		SelectChanged(selected);
	}
}
