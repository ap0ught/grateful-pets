using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class UAP_BaseElement_3D : UAP_BaseElement
{
	public Camera m_CameraRenderingThisObject;

	public override bool AutoFillTextLabel()
	{
		if (string.IsNullOrEmpty(m_Text))
		{
			m_Text = gameObject.name;
		}
		return true;
	}

	public override bool Is3DElement()
	{
		return true;
	}

	public float GetPixelHeight()
	{
		return (float)Screen.height / 8f;
	}

	public float GetPixelWidth()
	{
		return (float)Screen.height / 8f;
	}

	public override void HoverHighlight(bool enable, EHighlightSource selectionSource)
	{
		EventTrigger eventTrigger = null;
		if (m_ReferenceElement != null && m_ReferenceElement.activeInHierarchy)
		{
			eventTrigger = m_ReferenceElement.GetComponent<EventTrigger>();
		}
		if (eventTrigger == null && gameObject.activeInHierarchy)
		{
			eventTrigger = gameObject.GetComponent<EventTrigger>();
		}
		if (eventTrigger != null)
		{
			if (enable)
			{
				eventTrigger.OnSelect(new BaseEventData(EventSystem.current)
				{
					selectedObject = eventTrigger.gameObject
				});
			}
			else
			{
				eventTrigger.OnDeselect(new BaseEventData(EventSystem.current)
				{
					selectedObject = eventTrigger.gameObject
				});
			}
		}
		OnHoverHighlight(enable);
		m_CallbackOnHighlight.Invoke(enable, selectionSource);
	}

	protected override string GetLabelText(GameObject go)
	{
		if (go == null)
		{
			return "";
		}
		Text component = go.GetComponent<Text>();
		if (component != null)
		{
			return component.text;
		}
		return "";
	}
}
