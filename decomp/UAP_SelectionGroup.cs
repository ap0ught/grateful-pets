using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Accessibility/Helper/Group Focus Notification")]
public class UAP_SelectionGroup : MonoBehaviour
{
	private List<UAP_BaseElement> m_AllElements = new List<UAP_BaseElement>();

	private bool m_Selected;

	private GameObject m_LastFocusObject;

	public void AddElement(UAP_BaseElement element)
	{
		if (!m_AllElements.Contains(element))
		{
			m_AllElements.Add(element);
		}
	}

	public void RemoveElement(UAP_BaseElement element)
	{
		if (!m_AllElements.Contains(element))
		{
			m_AllElements.Remove(element);
		}
	}

	private void Update()
	{
		GameObject currentFocusObject = UAP_AccessibilityManager.GetCurrentFocusObject();
		if (currentFocusObject == m_LastFocusObject)
		{
			return;
		}
		m_LastFocusObject = currentFocusObject;
		bool flag = false;
		if (UAP_AccessibilityManager.IsEnabled() && currentFocusObject != null)
		{
			UAP_BaseElement component = currentFocusObject.GetComponent<UAP_BaseElement>();
			if (m_AllElements.Contains(component))
			{
				flag = true;
			}
		}
		if (flag != m_Selected)
		{
			m_Selected = flag;
			gameObject.BroadcastMessage("Accessibility_Selected", flag, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void OnDisable()
	{
		bool flag = false;
		gameObject.BroadcastMessage("Accessibility_Selected", flag, SendMessageOptions.DontRequireReceiver);
	}

	private void OnDestroy()
	{
		bool flag = false;
		gameObject.BroadcastMessage("Accessibility_Selected", flag, SendMessageOptions.DontRequireReceiver);
	}
}
