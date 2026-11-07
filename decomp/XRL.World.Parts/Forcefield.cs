using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.AI;

namespace XRL.World.Parts;

[Serializable]
public class Forcefield : IPart
{
	public bool RejectOwner = true;

	public GameObject Creator;

	[NonSerialized]
	public List<GameObject> AllowPassage;

	[NonSerialized]
	public List<string> AllowFactions;

	public string AllowPassageTag;

	public bool Unwalkable;

	public bool MissileOpaque;

	public bool MovesWithOwner;

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		base.Write(Basis, Writer);
		if (AllowPassage == null)
		{
			Writer.Write(0);
		}
		else
		{
			Writer.Write(1);
			Writer.WriteGameObjectList(AllowPassage);
		}
		Writer.Write(AllowFactions);
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		base.Read(Basis, Reader);
		if (Reader.ReadInt32() == 1)
		{
			AllowPassage = new List<GameObject>(1);
			Reader.ReadGameObjectList(AllowPassage);
		}
		AllowFactions = Reader.ReadList<string>();
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforePhysicsRejectObjectEntringCell");
		Object.ModIntProperty("Electromagnetic", 1);
		base.Register(Object, Registrar);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<BlocksRadarEvent>.ID && ID != PooledEvent<GetMaximumLiquidExposureEvent>.ID && ID != PooledEvent<OkayToDamageEvent>.ID && ID != PooledEvent<RealityStabilizeEvent>.ID && ID != PooledEvent<ShouldAttackToReachTargetEvent>.ID)
		{
			if (ID == TookDamageEvent.ID)
			{
				return MovesWithOwner;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(ShouldAttackToReachTargetEvent E)
	{
		if (E.Object == ParentObject && E.Actor != Creator && !CanPass(E.Actor))
		{
			E.ShouldAttack = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OkayToDamageEvent E)
	{
		if (MovesWithOwner && GameObject.Validate(ref Creator) && ParentObject.DistanceTo(Creator) <= 1 && GameObject.Validate(E.Actor) && !Creator.IsHostileTowards(E.Actor) && Creator.FireEvent(Event.New("CanBeAngeredByPropertyCrime", "Attacker", E.Actor, "Object", ParentObject)))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		if (MovesWithOwner && E.Object == ParentObject && E.Damage.Amount > 0 && GameObject.Validate(ref Creator) && ParentObject.DistanceTo(Creator) <= 1 && GameObject.Validate(E.Actor) && !Creator.IsHostileTowards(E.Actor) && Creator.FireEvent(Event.New("CanBeAngeredByPropertyCrime", "Attacker", E.Actor, "Object", ParentObject)))
		{
			Creator.AddOpinion<OpinionAttackProperty>(E.Actor, ParentObject);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMaximumLiquidExposureEvent E)
	{
		E.PercentageReduction = 100;
		return false;
	}

	public override bool HandleEvent(RealityStabilizeEvent E)
	{
		if (E.Check(CanDestroy: true))
		{
			Cell cell = ParentObject.GetCurrentCell()?.GetRandomLocalAdjacentCell();
			if (cell != null)
			{
				ParentObject.Discharge(cell, Stat.Random(1, 4), 0, "1d8", null, E.Effect.Owner, ParentObject);
			}
			ParentObject.TileParticleBlip("items/sw_quills.bmp", "&B", "K", 10, IgnoreVisibility: false, HFlip: false, VFlip: false, 0L);
			DidX("collapse", "under the pressure of normality", null, null, null, null, ParentObject);
			ParentObject.Destroy();
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BlocksRadarEvent E)
	{
		return false;
	}

	public void AddAllowPassage(GameObject obj)
	{
		if (AllowPassage == null)
		{
			AllowPassage = new List<GameObject>();
		}
		if (!AllowPassage.Contains(obj))
		{
			AllowPassage.Add(obj);
		}
	}

	public void RemoveAllowPassage(GameObject obj)
	{
		if (AllowPassage != null)
		{
			AllowPassage.Remove(obj);
			if (AllowPassage.Count == 0)
			{
				AllowPassage = null;
			}
		}
	}

	public void AddAllowPassage(string Faction)
	{
		if (AllowFactions == null)
		{
			AllowFactions = new List<string>(1);
		}
		AllowFactions.AddIfNot(Faction, AllowFactions.Contains);
	}

	public void RemoveAllowPassage(string Faction)
	{
		if (AllowFactions != null)
		{
			AllowFactions.Remove(Faction);
			if (AllowFactions.Count == 0)
			{
				AllowFactions = null;
			}
		}
	}

	public bool CanPass(GameObject Object)
	{
		GameObject.Validate(ref Creator);
		if (Object == null)
		{
			return false;
		}
		if (Object.HasTagOrProperty("IgnoresForceWall"))
		{
			return true;
		}
		if (Object.HasTagOrProperty("ForcefieldNullifier"))
		{
			return true;
		}
		if (MovesWithOwner && Object == Creator)
		{
			return true;
		}
		if (Unwalkable)
		{
			return false;
		}
		if (AllowPassage != null && AllowPassage.Contains(Object))
		{
			return true;
		}
		if (AllowFactions != null && Object.Brain != null)
		{
			foreach (string allowFaction in AllowFactions)
			{
				if (allowFaction == "Player")
				{
					if (Object.IsPlayerControlled())
					{
						return true;
					}
				}
				else if (Object.GetAllegianceLevel(allowFaction) >= Brain.AllegianceLevel.Member)
				{
					return true;
				}
			}
		}
		if (!AllowPassageTag.IsNullOrEmpty() && Object.HasTagOrStringProperty(AllowPassageTag))
		{
			return true;
		}
		if (!RejectOwner && Object == Creator)
		{
			return true;
		}
		return false;
	}

	public bool CanMissilePassFrom(GameObject Object, GameObject Projectile = null)
	{
		GameObject.Validate(ref Creator);
		if (Object == null)
		{
			return false;
		}
		if (Projectile != null)
		{
			if (Projectile.HasTagOrProperty("IgnoresForceWall"))
			{
				return true;
			}
			if (CanMissilePassForcefieldEvent.Check(Object, Projectile))
			{
				return true;
			}
		}
		if (MissileOpaque)
		{
			return false;
		}
		if (AllowPassage != null && AllowPassage.Contains(Object))
		{
			return true;
		}
		if (AllowFactions != null && Object.Brain != null)
		{
			foreach (string allowFaction in AllowFactions)
			{
				if (allowFaction == "Player")
				{
					if (Object.IsPlayerControlled())
					{
						return true;
					}
				}
				else if (Object.GetAllegianceLevel(allowFaction) >= Brain.AllegianceLevel.Member)
				{
					return true;
				}
			}
		}
		if (!AllowPassageTag.IsNullOrEmpty() && Object.HasTagOrStringProperty(AllowPassageTag))
		{
			return true;
		}
		if (!RejectOwner && Object == Creator)
		{
			return true;
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforePhysicsRejectObjectEntringCell" && CanPass(E.GetGameObjectParameter("Object")))
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
