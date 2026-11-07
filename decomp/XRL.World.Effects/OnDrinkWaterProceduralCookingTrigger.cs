using XRL.World.Parts;

namespace XRL.World.Effects;

public class OnDrinkWaterProceduralCookingTrigger : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature drink@s freshwater, there's a 25% chance";
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("Drank");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Drank" && Object != null && !Object.OnWorldMap() && E.GetIntParameter("WasFreshWater") == 1 && 25.in100())
		{
			int i = 0;
			for (int num = DrinkMagnifier.Magnify(Object, 1); i < num; i++)
			{
				Trigger();
			}
		}
		return base.FireEvent(E);
	}
}
