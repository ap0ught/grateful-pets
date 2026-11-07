using System;
using System.Collections.Generic;
using XRL.World.Anatomy;

namespace XRL.World.Parts;

/// Loads severed body parts into the parent object as inventory, as for
/// example with the wild-eyed watervine merchant and his inventory of
/// naphtaali and goatfolk body parts.
[Serializable]
public class BodyPartInventory : IPart
{
	public string ByInherit;

	public override bool SameAs(IPart p)
	{
		if ((p as BodyPartInventory).ByInherit != ByInherit)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AfterObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		if (!ByInherit.IsNullOrEmpty())
		{
			foreach (KeyValuePair<string, string> item in ByInherit.CachedDictionaryExpansion())
			{
				string key = item.Key;
				string value = item.Value;
				try
				{
					BodyPart.MakeSeveredBodyParts(value.RollCached(), null, null, null, key, null, ParentObject);
				}
				catch
				{
				}
			}
		}
		return base.HandleEvent(E);
	}
}
