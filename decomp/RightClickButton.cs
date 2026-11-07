using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class RightClickButton : Button
{
	[Serializable]
	public class ButtonRightClickedEvent : UnityEvent
	{
	}

	[SerializeField]
	public UnityEvent m_OnRightClick = new ButtonRightClickedEvent();

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			base.OnPointerClick(eventData);
		}
		else if (eventData.button == PointerEventData.InputButton.Right)
		{
			m_OnRightClick.Invoke();
		}
	}
}
