using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class PsionicDervish : IPart
{
	public int Tier = -1;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ObjectCreated");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ObjectCreated")
		{
			if (Tier == -1)
			{
				Tier = ParentObject.GetTier();
			}
			int num = 0;
			if (Tier <= 4)
			{
				num = 1;
			}
			else if (Tier <= 6)
			{
				num = 2;
			}
			else if (Tier >= 8)
			{
				num = 3;
			}
			string randomElement = new string[4] { "LongBlade", "ShortBlade", "Axe", "Cudgel" }.GetRandomElement();
			int num2 = XRL.World.Capabilities.Tier.Constrain(Tier + 1);
			string text = randomElement;
			if (text == "ShortBlade")
			{
				text = "Dagger";
			}
			GameObject gameObject = GameObject.Create(PopulationManager.RollOneFrom("DynamicInheritsTable:Base" + text + ":Tier" + num2).Blueprint);
			if (!gameObject.HasPart<ModPsionic>())
			{
				gameObject.ApplyModification(new ModPsionic());
			}
			ParentObject.ReceiveObject(gameObject);
			ParentObject.Brain.PerformEquip();
			int num3 = 0;
			if (num == 1)
			{
				num3 = 2;
			}
			if (num == 2)
			{
				num3 = 4;
			}
			if (num == 3)
			{
				num3 = 6;
			}
			string populationName = "";
			if (randomElement == "LongBlade")
			{
				populationName = "SkillPowers_LongBlades";
			}
			if (randomElement == "ShortBlade")
			{
				populationName = "SkillPowers_ShortBlades";
			}
			if (randomElement == "Axe")
			{
				populationName = "SkillPowers_Axe";
			}
			if (randomElement == "Cudgel")
			{
				populationName = "SkillPowers_Cudgel";
			}
			int num4 = 20;
			while (num3 > 0 && num4 > 0)
			{
				string blueprint = PopulationManager.RollOneFrom(populationName).Blueprint;
				if (!ParentObject.HasPart(blueprint))
				{
					ParentObject.AddSkill(blueprint);
					num3--;
				}
				num4--;
			}
		}
		return base.FireEvent(E);
	}
}
