using UnityEngine;
using UnityEngine.Events;

public class UAP_BackCloseEventListener : MonoBehaviour
{
	public UnityEvent m_OnBackEvent = new UnityEvent();

	private void OnEnable()
	{
		UAP_AccessibilityManager.RegisterOnBackCallback(OnScrubGesturePerformed);
	}

	private void OnDisable()
	{
		UAP_AccessibilityManager.UnregisterOnBackCallback(OnScrubGesturePerformed);
	}

	private void OnScrubGesturePerformed()
	{
		m_OnBackEvent.Invoke();
	}
}
