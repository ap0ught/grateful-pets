using System;
using System.Text;

namespace XRL.World.Units;

[Serializable]
public class GameObjectTieredArmorUnit : GameObjectUnit
{
	public string Tier;

	public int Amount;

	public bool Gigantic;

	public bool Equippable;

	public override void Apply(GameObject Object)
	{
		int num = 0;
		for (int i = 0; num < Amount && i <= 5; i++)
		{
			string text = PopulationManager.RollOneFrom("DynamicInheritsTable:Armor:Tier" + Tier)?.Blueprint;
			if (text.IsNullOrEmpty() || !GameObjectFactory.Factory.Blueprints.TryGetValue(text, out var value))
			{
				continue;
			}
			if (Equippable & (i < 5))
			{
				string partParameter = value.GetPartParameter<string>("Armor", "WornOn");
				if (!partParameter.IsNullOrEmpty() && partParameter != "*" && !Object.Body.HasPart(partParameter))
				{
					continue;
				}
			}
			i = 0;
			num++;
			GameObject gameObject = GameObjectFactory.Factory.CreateObject(value);
			if (Gigantic)
			{
				gameObject.ApplyModification("ModGigantic");
			}
			Object.ReceiveObject(gameObject);
		}
	}

	public override void Reset()
	{
		base.Reset();
		Tier = null;
		Amount = 0;
		Gigantic = false;
		Equippable = false;
	}

	public override string GetDescription(bool Inscription = false)
	{
		StringBuilder sB = Event.NewStringBuilder("Spawns with").Compound(Amount).Compound("random pieces of");
		if (Gigantic)
		{
			sB.Compound("gigantic,");
		}
		int num = Tier.RollMax();
		int num2 = Tier.RollMin();
		string text = "low";
		string text2 = "low";
		if (num2 > 5)
		{
			text = "high";
		}
		else if (num2 > 2)
		{
			text = "mid";
		}
		if (num > 5)
		{
			text2 = "high";
		}
		else if (num > 2)
		{
			text2 = "mid";
		}
		if (string.Equals(text, text2))
		{
			sB.Compound(text).Append(" tier armor");
		}
		else
		{
			sB.Compound(text + "-to-" + text2).Append(" tier armor");
		}
		return Event.FinalizeString(sB);
	}
}
