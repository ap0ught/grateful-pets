using System;

namespace XRL.World.Parts;

[Serializable]
public class VariableReplacementInDisplayName : IPart
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetDisplayNameEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		foreach (string key in E.DB.Keys)
		{
			if (key.Contains("="))
			{
				string text = GameText.VariableReplace(key, ParentObject);
				if (text != key)
				{
					E.DB.Add(text, E.DB[key]);
					E.DB.Remove(key);
				}
			}
		}
		return base.HandleEvent(E);
	}
}
