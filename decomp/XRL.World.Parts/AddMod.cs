using System;
using XRL.World.Tinkering;

namespace XRL.World.Parts;

[Serializable]
public class AddMod : IPart
{
	public string Mods = "";

	public string Tiers = "";

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		string[] array = Mods.Split(',');
		string[] array2 = Tiers.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			if (!string.IsNullOrEmpty(array[i]))
			{
				int tier = 1;
				if (!string.IsNullOrEmpty(Tiers) && array2.Length > i)
				{
					tier = Convert.ToInt32(array2[i]);
				}
				if (array[i].Length > 0 && array[i][0] == '@')
				{
					ItemModding.ApplyModificationFromPopulationTable(ParentObject, array[i].Substring(1), tier, Creation: true);
				}
				else
				{
					ItemModding.ApplyModification(ParentObject, array[i], tier, DoRegistration: true, null, Creation: true);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
