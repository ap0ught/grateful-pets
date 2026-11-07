using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class DiseaseMelee : IPart
{
	public int Chance = 10;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WeaponDealDamage");
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponDealDamage" && Chance.in100())
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Defender");
			if (gameObjectParameter != null && !gameObjectParameter.MakeSave("Toughness", 13, null, null, "Disease", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
			{
				if ((ParentObject.GetCurrentCell()?.ParentZone.Z ?? 0) % 2 == 0)
				{
					gameObjectParameter.ApplyEffect(new IronshankOnset());
				}
				else
				{
					gameObjectParameter.ApplyEffect(new GlotrotOnset());
				}
			}
		}
		return base.FireEvent(E);
	}
}
