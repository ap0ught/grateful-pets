using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class exUIEventSender : MonoBehaviour
{
	[Serializable]
	public class SlotInfo
	{
		public GameObject receiver;

		public string method = "";
	}

	[Serializable]
	public class Emitter
	{
		public string eventName;

		public List<SlotInfo> slots;
	}

	public List<Emitter> emitterList = new List<Emitter>();

	private void Awake()
	{
		exUIControl component = GetComponent<exUIControl>();
		if (component != null)
		{
			Type type = component.GetType();
			{
				foreach (Emitter emitter in emitterList)
				{
					EventInfo eventInfo = type.GetEvent(emitter.eventName);
					if (eventInfo != null)
					{
						foreach (SlotInfo slot in emitter.slots)
						{
							bool flag = false;
							MonoBehaviour[] components = slot.receiver.GetComponents<MonoBehaviour>();
							foreach (MonoBehaviour monoBehaviour in components)
							{
								MethodInfo method = monoBehaviour.GetType().GetMethod(slot.method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(GameObject) }, null);
								if (method != null)
								{
									Delegate handler = Delegate.CreateDelegate(typeof(Action<GameObject>), monoBehaviour, method);
									eventInfo.AddEventHandler(component, handler);
									flag = true;
								}
							}
							if (!flag)
							{
								Debug.LogWarning("Can not find method " + slot.method + " in " + slot.receiver.name);
							}
						}
					}
					else
					{
						Debug.LogWarning("Can not find event " + emitter.eventName + " in " + gameObject.name);
					}
				}
				return;
			}
		}
		Debug.LogWarning("Can not find exUIControl in this GameObject");
	}
}
