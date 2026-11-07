using System;

namespace XRL.World.Parts.Skill;

/// This part is not used in the base game.
[Serializable]
public class TenfoldPath_Vur : BaseInitiatorySkill
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ObjectAttacking");
		Registrar.Register("TargetedForMissileWeapon");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ObjectAttacking" || E.ID == "TargetedForMissileWeapon")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			if (gameObjectParameter != null && gameObjectParameter.FireEvent("CanApplyFear") && gameObjectParameter.FireEvent("ApplyFear") && !gameObjectParameter.MakeSave("Willpower", 15, null, null, "Vur Counteraggression Fear"))
			{
				if (gameObjectParameter.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You cannot bring yourself to attack " + ParentObject.t() + ".", 'r');
				}
				gameObjectParameter.UseEnergy(1000, "Attempted Attack");
				return false;
			}
		}
		return base.FireEvent(E);
	}
}
