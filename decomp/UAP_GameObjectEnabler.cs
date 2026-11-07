using UnityEngine;

[AddComponentMenu("Accessibility/Helper/GameObject Enabler")]
public class UAP_GameObjectEnabler : MonoBehaviour
{
	public GameObject[] m_ObjectsToEnable;

	public GameObject[] m_ObjectsToDisable;

	private void Awake()
	{
		SetActiveState();
	}

	private void Start()
	{
		SetActiveState();
	}

	private void OnEnable()
	{
		UAP_AccessibilityManager.RegisterAccessibilityModeChangeCallback(Accessibility_StateChange);
		SetActiveState();
	}

	private void SetActiveState()
	{
		if (!base.gameObject.activeInHierarchy || !enabled)
		{
			return;
		}
		GameObject[] objectsToEnable = m_ObjectsToEnable;
		foreach (GameObject gameObject in objectsToEnable)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(UAP_AccessibilityManager.IsEnabled());
			}
		}
		objectsToEnable = m_ObjectsToDisable;
		foreach (GameObject gameObject2 in objectsToEnable)
		{
			if (gameObject2 != null)
			{
				gameObject2.SetActive(!UAP_AccessibilityManager.IsEnabled());
			}
		}
	}

	private void OnDisable()
	{
		UAP_AccessibilityManager.UnregisterAccessibilityModeChangeCallback(Accessibility_StateChange);
	}

	public void Accessibility_StateChange(bool newState)
	{
		SetActiveState();
	}
}
