using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Persuasion_InspiringPresence : BaseSkill
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("MinionTakingAction");
		base.Register(Object, Registrar);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("jewels", 3);
		}
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "MinionTakingAction")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
			int amount = ParentObject.StatMod("Ego") * 4;
			gameObjectParameter.ApplyEffect(new Emboldened(5, "Hitpoints", amount));
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		if (GO.IsPlayer())
		{
			SocialSifrah.AwardInsight();
		}
		return base.AddSkill(GO);
	}
}
