using XRL.World.Parts;

namespace XRL.World.Effects;

public class CookingDomainMedicinal_OnDrinkHoney : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature drink@s honey,";
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("DrinkingFrom");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "DrinkingFrom")
		{
			LiquidVolume liquidVolume = (E.GetParameter("Container") as GameObject).LiquidVolume;
			if (liquidVolume.GetPrimaryLiquid() != null && liquidVolume.IsPureLiquid("honey"))
			{
				int i = 0;
				for (int num = DrinkMagnifier.Magnify(Object, 1); i < num; i++)
				{
					Trigger();
				}
			}
		}
		return base.FireEvent(E);
	}
}
