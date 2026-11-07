using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class RandomLoot : IPart
{
	[NonSerialized]
	public static int Deaths;

	public bool LootCreated;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AfterObjectCreatedEvent.ID)
		{
			return ID == BeforeDeathRemovalEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		Check(Create: true, E.Context, E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		Check(Create: false, "Death", E);
		return base.HandleEvent(E);
	}

	private void Check(bool Create, string Context = null, IEvent ParentEvent = null)
	{
		if (LootCreated)
		{
			return;
		}
		LootCreated = true;
		if (ParentObject.HasTagOrProperty("NoLoot") || (Create && ParentObject.GetIntProperty("Humanoid") == 0 && (ParentObject.GetIntProperty("Ape") == 0 || 90.in100())))
		{
			return;
		}
		List<GameObject> list = new List<GameObject>(8);
		if (ParentObject.HasIntProperty("RareLoot"))
		{
			int intProperty = ParentObject.GetIntProperty("RareLoot");
			int num = Stat.Random(1, 3);
			for (int i = 0; i < num; i++)
			{
				list.Add(PopulationManager.CreateOneFrom("Junk " + intProperty + "R", null, 100, 0, null, Context));
			}
		}
		else
		{
			int num2 = Stat.Random(1, 100) + ParentObject.Stat("Level");
			Deaths++;
			num2 += Deaths;
			int num3 = 0;
			while (num2 > 98)
			{
				list.Add(PopulationManager.CreateOneFrom("Junk " + Tier.Constrain(ParentObject.Stat("Level") / 5 + 1), null, 0, 0, null, Context));
				Deaths = 0;
				num2 -= 50 + Stat.Random(1, 100);
			}
		}
		if (Create)
		{
			ParentObject.ReceiveObject(list);
			LootCreated = true;
			return;
		}
		IInventory dropInventory = ParentObject.GetDropInventory();
		if (dropInventory == null)
		{
			return;
		}
		foreach (GameObject item in list)
		{
			if (GameObject.Validate(item) && item.IsReal)
			{
				dropInventory.AddObjectToInventory(item, null, Silent: false, NoStack: false, FlushTransient: true, null, ParentEvent);
				DroppedEvent.Send(ParentObject, item);
			}
		}
	}
}
