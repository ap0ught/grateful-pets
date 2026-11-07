using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetCyberneticsBehaviorDescriptionEvent : PooledEvent<GetCyberneticsBehaviorDescriptionEvent>
{
	public GameObject Object;

	public string BaseDescription;

	public string Description;

	public List<string> ToAdd = new List<string>();

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		BaseDescription = null;
		Description = null;
		ToAdd.Clear();
	}

	public void Add(string Text)
	{
		if (ToAdd == null)
		{
			ToAdd = new List<string>();
		}
		if (!ToAdd.Contains(Text))
		{
			ToAdd.Add(Text);
		}
	}

	public static string GetFor(GameObject Object, string BaseDescription = null)
	{
		string text = BaseDescription;
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetCyberneticsBehaviorDescription"))
		{
			Event obj = Event.New("GetCyberneticsBehaviorDescription");
			obj.SetParameter("Object", Object);
			obj.SetParameter("BaseDescription", BaseDescription);
			obj.SetParameter("Description", text);
			flag = Object.FireEvent(obj);
			text = obj.GetStringParameter("Description");
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetCyberneticsBehaviorDescriptionEvent>.ID, MinEvent.CascadeLevel))
		{
			GetCyberneticsBehaviorDescriptionEvent getCyberneticsBehaviorDescriptionEvent = PooledEvent<GetCyberneticsBehaviorDescriptionEvent>.FromPool();
			getCyberneticsBehaviorDescriptionEvent.Object = Object;
			getCyberneticsBehaviorDescriptionEvent.BaseDescription = BaseDescription;
			getCyberneticsBehaviorDescriptionEvent.Description = text;
			flag = Object.HandleEvent(getCyberneticsBehaviorDescriptionEvent);
			text = getCyberneticsBehaviorDescriptionEvent.Description;
			if (getCyberneticsBehaviorDescriptionEvent.ToAdd != null)
			{
				foreach (string item in getCyberneticsBehaviorDescriptionEvent.ToAdd)
				{
					if (text == null)
					{
						text = "";
					}
					else if (text != "" && !text.EndsWith("\n"))
					{
						text += "\n";
					}
					text += item;
				}
			}
		}
		return text;
	}
}
