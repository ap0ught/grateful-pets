using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class Lost : Effect, ITierInitialized
{
	public bool DisableUnlost;

	public long LostOn;

	public string World;

	public int MaxOrientZ = 10;

	public List<string> Visited = new List<string>();

	public Lost()
	{
		DisplayName = "lost";
		Duration = 9999;
	}

	public Lost(int Duration = 9999, string InitialZone = null, string World = null, int MaxOrientZ = 10, bool DisableUnlost = false)
		: this()
	{
		base.Duration = Duration;
		this.World = World;
		if (!InitialZone.IsNullOrEmpty())
		{
			Visited.Add(InitialZone);
			if (this.World == null)
			{
				this.World = ZoneID.GetWorldID(InitialZone);
			}
		}
		this.MaxOrientZ = MaxOrientZ;
		this.DisableUnlost = DisableUnlost;
	}

	public void Initialize(int Tier)
	{
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDetails()
	{
		return "Can't fast travel.";
	}

	public override int GetEffectType()
	{
		return 117440640;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool Apply(GameObject Object)
	{
		if (LostOn == 0L)
		{
			Zone zone = Object?.CurrentZone;
			if (zone != null)
			{
				if (World == null)
				{
					World = zone.ZoneWorld;
				}
				if (!Visited.Contains(zone.ZoneID))
				{
					Visited.Add(zone.ZoneID);
				}
			}
			LostOn = XRLCore.CurrentTurn - 1;
		}
		if (Object != null)
		{
			int i = 0;
			for (int count = Object.Effects.Count; i < count; i++)
			{
				if (Object.Effects[i] is Lost lost && lost.World == World)
				{
					bool flag = false;
					if (Duration > lost.Duration)
					{
						lost.Duration = Duration;
						flag = true;
					}
					if (DisableUnlost && !lost.DisableUnlost)
					{
						lost.DisableUnlost = true;
						flag = true;
					}
					if (flag)
					{
						Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_bewilderment");
					}
					return false;
				}
			}
		}
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_bewilderment");
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			if (Duration == -1)
			{
				Popup.ShowSpace("You recognize the area and stop being lost!");
			}
			else if (Duration != int.MinValue)
			{
				Popup.ShowSpace("You regain your bearings.");
			}
		}
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<AfterPlayerBodyChangeEvent>.ID && ID != PooledEvent<CanTravelEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID)
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Object == Object && !Object.IsPlayer() && !E.Reference)
		{
			E.AddAdjective("lost");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (IsPlayer() && Duration > 0 && !DisableUnlost && E.Zone != null)
		{
			if (World == null)
			{
				World = E.Zone.ZoneWorld;
			}
			if (E.Zone.ZoneWorld == World)
			{
				if (E.Zone.LastPlayerPresence != -1 && E.Zone.LastPlayerPresence < LostOn)
				{
					Duration = -1;
					Object.RemoveEffect(this);
				}
				else if (!Visited.Contains(E.Zone.ZoneID) && E.Zone.Z <= MaxOrientZ)
				{
					Visited.Add(E.Zone.ZoneID);
					if ((Object.HasSkill("Survival") ? 40 : 20).in100())
					{
						Object.RemoveEffect(this);
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanTravelEvent E)
	{
		if (E.Object == Object && Object.CurrentZone?.ZoneWorld == World)
		{
			return E.Object.ShowFailure("You are lost!");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterPlayerBodyChangeEvent E)
	{
		if (E.OldBody == Object && E.NewBody != null)
		{
			E.OldBody.RemoveEffect(this, NeedStackCheck: false);
			E.NewBody.ApplyEffect(this);
		}
		return base.HandleEvent(E);
	}
}
