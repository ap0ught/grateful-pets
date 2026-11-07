using XRL.Language;

namespace XRL.World.Effects;

public class CookingDomainElectric_OnElectricDamaged : ProceduralCookingEffectWithTrigger
{
	public int Tier = 50;

	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature take@s electric damage, there's " + Grammar.A(Tier) + "% chance";
	}

	public override string GetTemplatedTriggerDescription()
	{
		return "whenever @thisCreature take@s electric damage, there's a 50% chance";
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("TookDamage");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "TookDamage" && E.GetParameter("Damage") is Damage damage && damage.IsElectricDamage() && Tier.in100())
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
