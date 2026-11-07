using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;
using XRL.World.Anatomy;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class ShatterArmor : IShatterEffect, ITierInitialized
{
	public int AVPenalty = 1;

	public GameObject Owner;

	public ShatterArmor()
	{
		DisplayName = "{{r|cleaved}}";
	}

	public ShatterArmor(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		Tier = Stat.Random(Tier - 2, Tier + 2);
		if (Tier < 1)
		{
			Tier = 1;
		}
		if (Tier > 8)
		{
			Tier = 8;
		}
		Duration = 300;
		AVPenalty = Stat.Random(1, 10);
	}

	public override int GetEffectType()
	{
		return 117441536;
	}

	public override bool SameAs(Effect e)
	{
		ShatterArmor shatterArmor = e as ShatterArmor;
		if (shatterArmor.AVPenalty != AVPenalty)
		{
			return false;
		}
		if (shatterArmor.Owner != Owner)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override string GetDescription()
	{
		return "{{r|cleaved ({{C|-" + AVPenalty + " AV}})}}";
	}

	public override string GetStateDescription()
	{
		return "{{r|cleaved}}";
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDetails()
	{
		return "-" + AVPenalty + " AV";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<IsRepairableEvent>.ID)
		{
			return ID == PooledEvent<RepairedEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(IsRepairableEvent E)
	{
		return false;
	}

	public override bool HandleEvent(RepairedEvent E)
	{
		Object.RemoveEffect(this);
		return base.HandleEvent(E);
	}

	public override int GetPenalty()
	{
		return AVPenalty;
	}

	public override void IncrementPenalty()
	{
		AVPenalty++;
	}

	public override GameObject GetOwner()
	{
		return Owner;
	}

	public override void SetOwner(GameObject Owner)
	{
		this.Owner = Owner;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			bool flag = Object.BaseStat("AV") > 0;
			List<GameObject> list = Event.NewGameObjectList();
			Body body = Object.Body;
			if (body != null)
			{
				foreach (BodyPart equippedPart in body.GetEquippedParts())
				{
					if (equippedPart.Equipped != null)
					{
						Armor part = equippedPart.Equipped.GetPart<Armor>();
						if (part != null && part.AV > 0)
						{
							list.Add(equippedPart.Equipped);
						}
					}
					else if (flag && equippedPart.Contact && !equippedPart.Abstract && !equippedPart.Extrinsic)
					{
						list.Add(null);
					}
				}
			}
			GameObject randomElement = list.GetRandomElement();
			if (randomElement != null)
			{
				randomElement.ApplyEffect(new ShatteredArmor(AVPenalty, Duration));
				return false;
			}
			if (!flag)
			{
				return false;
			}
		}
		else if (Object.Stat("AV") <= 0)
		{
			return false;
		}
		if (Object.Energy == null)
		{
			return false;
		}
		ShatterArmor effect = Object.GetEffect<ShatterArmor>();
		if (effect != null)
		{
			Object.PlayWorldSound("breakage", 0.5f, 0f, Combat: true);
			if (Duration > effect.Duration)
			{
				effect.Duration = Duration;
			}
			effect.UnapplyStats();
			effect.AVPenalty += AVPenalty;
			effect.ApplyStats();
			Object.ParticleText("*cleave (-" + effect.AVPenalty + " AV)*", IComponent<GameObject>.ConsequentialColorChar(null, Object));
			return false;
		}
		if (!Object.FireEvent("ApplyShatterArmor"))
		{
			return false;
		}
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_physicalRupture");
		ApplyStats();
		Object.ParticleText("*cleave (-" + AVPenalty + " AV)*", IComponent<GameObject>.ConsequentialColorChar(null, Object));
		return true;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
	}

	private void ApplyStats()
	{
		StatShifter.SetStatShift("AV", -AVPenalty);
	}

	private void UnapplyStats()
	{
		StatShifter.RemoveStatShifts();
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyStats();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyStats();
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 25 && num < 40)
			{
				E.Tile = null;
				E.RenderString = "X";
				E.ColorString = "&B^c";
			}
		}
		return true;
	}
}
