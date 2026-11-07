using System;

namespace XRL.World.Parts;

/// This part is not used in the base game.
[Serializable]
public class SifrahInsightOnEat : IPart
{
	public int TinkeringChance;

	public int SocialChance;

	public int RitualChance;

	public int PsionicChance;

	public override bool SameAs(IPart Part)
	{
		SifrahInsightOnEat sifrahInsightOnEat = Part as SifrahInsightOnEat;
		if (sifrahInsightOnEat.TinkeringChance != TinkeringChance)
		{
			return false;
		}
		if (sifrahInsightOnEat.SocialChance != SocialChance)
		{
			return false;
		}
		if (sifrahInsightOnEat.RitualChance != RitualChance)
		{
			return false;
		}
		if (sifrahInsightOnEat.PsionicChance != PsionicChance)
		{
			return false;
		}
		return base.SameAs(Part);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("OnEat");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "OnEat")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Eater");
			if (gameObjectParameter != null && gameObjectParameter.IsPlayer())
			{
				if (TinkeringChance.in100())
				{
					TinkeringSifrah.AwardInsight();
				}
				if (SocialChance.in100())
				{
					SocialSifrah.AwardInsight();
				}
				if (RitualChance.in100())
				{
					RitualSifrah.AwardInsight();
				}
				if (PsionicChance.in100())
				{
					PsionicSifrah.AwardInsight();
				}
			}
		}
		return base.FireEvent(E);
	}
}
