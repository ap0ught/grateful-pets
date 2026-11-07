using System;
using System.Text;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class CooldownAmmoLoader : IPart
{
	public string Cooldown = "2d6";

	public string ProjectileObject;

	public bool Readout;

	private int CurrentCooldown;

	public long LastFireTimeTick;

	public override bool SameAs(IPart p)
	{
		CooldownAmmoLoader cooldownAmmoLoader = p as CooldownAmmoLoader;
		if (cooldownAmmoLoader.Cooldown != Cooldown)
		{
			return false;
		}
		if (cooldownAmmoLoader.ProjectileObject != ProjectileObject)
		{
			return false;
		}
		if (cooldownAmmoLoader.Readout != Readout)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<AIWantUseWeaponEvent>.ID && ID != PooledEvent<CheckLoadAmmoEvent>.ID && ID != PooledEvent<CheckReadyToFireEvent>.ID && ID != PooledEvent<GetAmmoCountAvailableEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetMissileWeaponProjectileEvent>.ID && ID != PooledEvent<GetMissileWeaponStatusEvent>.ID && ID != PooledEvent<GetNotReadyToFireMessageEvent>.ID && ID != PooledEvent<GetProjectileBlueprintEvent>.ID)
		{
			return ID == PooledEvent<LoadAmmoEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIWantUseWeaponEvent E)
	{
		if (CooldownActive())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAmmoCountAvailableEvent E)
	{
		if (CooldownActive())
		{
			E.Count = 0;
			return false;
		}
		E.Register(1);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckLoadAmmoEvent E)
	{
		if (CooldownActive())
		{
			if (E.Message == null)
			{
				E.Message = GetCoolingDownMessage();
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LoadAmmoEvent E)
	{
		if (CooldownActive())
		{
			if (E.Message == null)
			{
				E.Message = GetCoolingDownMessage();
			}
			return false;
		}
		if (!ProjectileObject.IsNullOrEmpty())
		{
			E.Projectile = GameObject.Create(ProjectileObject, 0, 0, null, null, null, "Projectile");
		}
		LastFireTimeTick = The.Game.TimeTicks;
		CurrentCooldown = Stat.Roll(Cooldown);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckReadyToFireEvent E)
	{
		if (CooldownActive())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetNotReadyToFireMessageEvent E)
	{
		if (CooldownActive())
		{
			E.Message = GetCoolingDownMessage();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMissileWeaponStatusEvent E)
	{
		if (E.Override == null)
		{
			if (CooldownActive())
			{
				if (E.Status != null)
				{
					E.Status.ammoTotal = 0;
					E.Status.ammoRemaining = 0;
				}
				E.Items.Length = 0;
				ApplyCooldownDisplay(E.Items);
				E.Override = this;
			}
			else if (E.Status != null)
			{
				E.Status.ammoTotal = 1;
				E.Status.ammoRemaining = 1;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetProjectileBlueprintEvent E)
	{
		if (!ProjectileObject.IsNullOrEmpty())
		{
			E.Blueprint = ProjectileObject;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMissileWeaponProjectileEvent E)
	{
		if (!ProjectileObject.IsNullOrEmpty())
		{
			E.Blueprint = ProjectileObject;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood())
		{
			E.AddMissileWeaponDamageTag(GetMissileWeaponPerformanceEvent.GetFor(ParentObject.Equipped, ParentObject));
			if (!E.Reference && CooldownActive())
			{
				string cooldownDisplay = GetCooldownDisplay();
				if (!cooldownDisplay.IsNullOrEmpty())
				{
					E.AddTag(cooldownDisplay, -5);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public bool CooldownActive()
	{
		if (LastFireTimeTick != The.Game.TimeTicks)
		{
			return LastFireTimeTick + CurrentCooldown > The.Game.TimeTicks;
		}
		return false;
	}

	public string GetCoolingDownMessage()
	{
		int num = (int)(LastFireTimeTick + CurrentCooldown - The.Game.TimeTicks);
		if (Readout)
		{
			return ParentObject.Does("need") + " " + num.Things("more round", "more rounds") + " before " + ParentObject.it + " can be fired again.";
		}
		return ParentObject.Does("are") + " unresponsive as " + ParentObject.does("cool", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + " down.";
	}

	public bool ApplyCooldownDisplay(StringBuilder SB)
	{
		long num = LastFireTimeTick + CurrentCooldown - XRLCore.Core.Game.TimeTicks;
		if (num > 0)
		{
			SB.Append("{{y|");
			if (Readout)
			{
				SB.Append('[').Append(num).Append(" sec]");
			}
			else
			{
				SB.Append("[cooldown]");
			}
			SB.Append("}}");
			return true;
		}
		return false;
	}

	public string GetCooldownDisplay()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		ApplyCooldownDisplay(stringBuilder);
		return stringBuilder.ToString();
	}
}
