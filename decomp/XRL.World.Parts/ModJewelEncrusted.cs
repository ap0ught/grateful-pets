using System;
using System.Collections.Generic;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class ModJewelEncrusted : IModification
{
	public int Amount = 100;

	public string adjColored = "";

	public bool bNamed;

	public ModJewelEncrusted()
	{
	}

	public ModJewelEncrusted(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override void ApplyModification(GameObject Object)
	{
		AddsRep.AddModifier(Object, "Water:" + Amount + ":hidden");
	}

	public override bool SameAs(IPart p)
	{
		ModJewelEncrusted modJewelEncrusted = p as ModJewelEncrusted;
		if (modJewelEncrusted.Amount != Amount)
		{
			return false;
		}
		if (modJewelEncrusted.adjColored != adjColored)
		{
			return false;
		}
		if (modJewelEncrusted.bNamed != bNamed)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetItemElementsEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.Understood() || !E.Object.HasProperName)
		{
			string value = "jewel-encrusted";
			if (!bNamed)
			{
				List<char> list = new List<char>
				{
					'b', 'B', 'g', 'G', 'W', 'r', 'R', 'm', 'M', 'c',
					'C'
				};
				char c = 'y';
				char c2 = 'y';
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.Append("{{");
				for (int i = 0; i < 5; i++)
				{
					while (c2 == c)
					{
						c2 = list.GetRandomElement();
					}
					if (i > 0)
					{
						stringBuilder.Append('-');
					}
					stringBuilder.Append(c2);
					c = c2;
				}
				stringBuilder.Append("-y");
				for (int j = 6; j < 15; j++)
				{
					while (c2 == c)
					{
						c2 = list.GetRandomElement();
					}
					stringBuilder.Append('-').Append(c2);
					c = c2;
				}
				stringBuilder.Append(" sequence|").Append(value).Append("}}");
				adjColored = stringBuilder.ToString();
				bNamed = true;
			}
			E.AddAdjective(adjColored, -20);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetInstanceDescription());
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("jewels", 10);
		}
		return base.HandleEvent(E);
	}

	public string GetInstanceDescription()
	{
		return "Jewel-Encrusted: This item is much more valuable than usual and grants the wearer " + Amount.Signed() + " reputation with water barons.";
	}
}
