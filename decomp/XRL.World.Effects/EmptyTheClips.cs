using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;
using XRL.World.Parts.Skill;

namespace XRL.World.Effects;

[Serializable]
public class EmptyTheClips : Effect, ITierInitialized
{
	public EmptyTheClips()
	{
		DisplayName = "Empty the Clips";
	}

	public EmptyTheClips(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(20, 30);
	}

	public override int GetEffectType()
	{
		return 128;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDetails()
	{
		return "Fires pistols twice as quickly.";
	}

	public override string GetDescription()
	{
		return "emptying the clips";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.TryGetEffect<EmptyTheClips>(out var Effect))
		{
			if (Duration > Effect.Duration)
			{
				Effect.Duration = Duration;
			}
			return false;
		}
		if (!Object.FireEvent(Event.New("ApplyEmptyTheClips", "Effect", this)))
		{
			return false;
		}
		Object.PlayWorldSound("Sounds/Abilities/sfx_ability_emptyTheClips");
		List<GameObject> missileWeapons = Object.GetMissileWeapons(null, Pistol_EmptyTheClips.IsPistol);
		if (missileWeapons.IsNullOrEmpty())
		{
			if (Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You begin itching for a trigger.", 'G');
			}
		}
		else if (missileWeapons.Count == 1)
		{
			DidX("clasp", Object.its + missileWeapons[0].DisplayNameOnlyDirectAndStripped + " eagerly", null, null, null, Object);
		}
		else
		{
			DidX("clasp", Object.its + " pistols eagerly", null, null, null, Object);
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
		DidX("loosen", Object.its + " pistol grip");
		base.Remove(Object);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 45 && num < 55)
			{
				E.Tile = null;
				E.RenderString = "\u001a";
				E.ColorString = "&B";
			}
		}
		return true;
	}
}
