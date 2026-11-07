using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Distraction : IPart
{
	public int ID;

	public GameObject Original;

	public string SaveStat = "Intelligence";

	public string SaveVs = "Hologram Illusion Distraction";

	public int SaveTarget = 15;

	public int SaveTurns = 10;

	public int Radius = 12;

	public GameObject Source;

	public Distraction()
	{
	}

	public Distraction(GameObject Original, GameObject Source = null, int SaveTarget = 15)
		: this()
	{
		this.Original = Original;
		this.Source = Source;
		this.SaveTarget = SaveTarget;
	}

	public bool MakeSave(GameObject Object)
	{
		GameObject gameObject = Source ?? Original;
		if (Object.MakeSave(SaveStat, SaveTarget, null, null, SaveVs, IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, gameObject))
		{
			Object.ApplyEffect(new DistractionImmunity
			{
				SourceID = gameObject.BaseID,
				DistractionID = ID,
				Duration = Stat.Random(3600, 8400)
			});
			return true;
		}
		return false;
	}

	public bool Distract(GameObject Object)
	{
		GameObject parentObject = ParentObject;
		if (Object.IsInvalid() || Object.IsPlayer() || Object.HasEffect<Distracted>() || Object.Brain == null || Object.Brain.Target == parentObject)
		{
			return false;
		}
		GameObject gameObject = Source ?? Original;
		if (Object.HasEffect<DistractionImmunity>() && gameObject.HasID)
		{
			foreach (Effect effect in Object.Effects)
			{
				if (effect is DistractionImmunity distractionImmunity && distractionImmunity.SourceID == gameObject.BaseID && distractionImmunity.DistractionID == ID)
				{
					return false;
				}
			}
		}
		if (CanApplyEffectEvent.Check<Distracted>(Object) && Object.DistanceTo(parentObject) <= Object.Brain.MaxKillRadius && !MakeSave(Object))
		{
			return Object.ApplyEffect(new Distracted(this));
		}
		return false;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (!GameObject.Validate(ParentObject) || !GameObject.Validate(ref Original))
		{
			return;
		}
		foreach (GameObject item in ParentObject.CurrentZone.FastFloodVisibility(ParentObject.CurrentCell.X, ParentObject.CurrentCell.Y, Radius, typeof(Brain), ParentObject))
		{
			if (item.Brain != null && item.Brain.Target == Original)
			{
				Distract(item);
			}
		}
	}
}
