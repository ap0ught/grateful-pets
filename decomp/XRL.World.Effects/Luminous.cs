using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class Luminous : Effect, ITierInitialized
{
	public Luminous()
	{
	}

	public Luminous(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(200, 700);
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 83886144;
	}

	public override string GetDescription()
	{
		return "{{m|phosphorescent}}";
	}

	public override string GetDetails()
	{
		return "Radiates light in radius 3.";
	}

	public override bool Apply(GameObject Object)
	{
		Object.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_lightManipulation_activate");
		DidX("start", "to glow", null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
		return true;
	}

	public override void Remove(GameObject Object)
	{
		IComponent<GameObject>.EmitMessage(Object, Object.Poss("glow") + " dims until it's extinguished.");
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeRenderEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		if (Duration > 0)
		{
			AddLight(2);
		}
		return base.HandleEvent(E);
	}
}
