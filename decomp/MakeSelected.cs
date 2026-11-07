using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MakeSelected : MonoBehaviour
{
	private static EventSystem _EventSystem;

	public EventSystem EventSystemManager
	{
		get
		{
			if (_EventSystem == null)
			{
				_EventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
			}
			return _EventSystem;
		}
	}

	public void Update()
	{
		EventSystemManager.firstSelectedGameObject = gameObject;
		if (GetComponent<Button>() != null)
		{
			GetComponent<Button>().Select();
		}
		Object.Destroy(this);
	}
}
