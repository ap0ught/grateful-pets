using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class TreatAsSolid : IPart
{
	public string TargetPart;

	public string TargetTag;

	public string TargetTagValue;

	public string Message;

	public bool RealityDistortionBased;

	public bool GazeBased;

	public bool LightBased;

	public bool RequiresPhaseMatch = true;

	public bool Hits = true;

	private bool MatchInner(Cell Cell, GameObject Attacker, out GameObject ObjectHit, out bool RecheckHit, out bool RecheckPhase, GameObject Launcher = null, GameObject ApparentTarget = null, GameObject PhaseFrom = null, bool PenetrateCreatures = false, bool PenetrateWalls = false, bool Prospective = false)
	{
		ObjectHit = null;
		RecheckHit = false;
		RecheckPhase = false;
		if (Cell == null)
		{
			return false;
		}
		int num = 0;
		while (true)
		{
			IL_0012:
			if ((GazeBased || LightBased) && Cell.IsBlackedOut())
			{
				GameObject parentObject = ParentObject;
				GameObject apparentTarget = ApparentTarget;
				bool flag = BeforeProjectileHitEvent.Check(parentObject, Attacker, null, out var Recheck, out var RecheckPhase2, PenetrateCreatures, PenetrateWalls, Launcher, apparentTarget, Cell, this, LightBased: true, Prospective);
				if (RecheckPhase2)
				{
					RecheckPhase = true;
				}
				if (Recheck)
				{
					RecheckHit = true;
					if (++num < 100)
					{
						continue;
					}
				}
				if (flag)
				{
					return true;
				}
			}
			if (!RealityDistortionBased && TargetPart.IsNullOrEmpty() && TargetTag.IsNullOrEmpty())
			{
				break;
			}
			int i = 0;
			for (int count = Cell.Objects.Count; i < count; i++)
			{
				GameObject gameObject = Cell.Objects[i];
				GameObject apparentTarget = Launcher;
				bool flag2 = Match(gameObject, Attacker, out var RecheckHit2, out var RecheckPhase3, apparentTarget, ApparentTarget, PhaseFrom, PenetrateCreatures, PenetrateWalls, Prospective, Cell);
				if (RecheckPhase3)
				{
					RecheckPhase = true;
				}
				if (RecheckHit2)
				{
					RecheckHit = true;
					if (++num < 100)
					{
						goto IL_0012;
					}
				}
				if (flag2)
				{
					if (Hits)
					{
						ObjectHit = Cell.Objects[i];
					}
					return true;
				}
			}
			break;
		}
		return false;
	}

	public bool Match(Cell Cell, GameObject Attacker, out GameObject ObjectHit, out bool RecheckHit, out bool RecheckPhase, GameObject Launcher = null, GameObject ApparentTarget = null, GameObject PhaseFrom = null, bool PenetrateCreatures = false, bool PenetrateWalls = false, bool Prospective = false)
	{
		bool flag = MatchInner(Cell, Attacker, out ObjectHit, out RecheckHit, out RecheckPhase, Launcher, ApparentTarget, PhaseFrom, PenetrateCreatures, PenetrateWalls, Prospective);
		if (flag && !Prospective && !Message.IsNullOrEmpty() && (Cell.IsVisible() || ((GazeBased || LightBased) && Cell.IsAnyLocalAdjacentCellVisible())))
		{
			IComponent<GameObject>.AddPlayerMessage(GameText.VariableReplace(Message, ParentObject, ObjectHit));
		}
		return flag;
	}

	public bool Match(Cell Cell, GameObject Attacker, GameObject Launcher = null, GameObject ApparentTarget = null, GameObject PhaseFrom = null, bool PenetrateCreatures = false, bool PenetrateWalls = false, bool Prospective = false)
	{
		GameObject ObjectHit;
		bool RecheckHit;
		bool RecheckPhase;
		return Match(Cell, Attacker, out ObjectHit, out RecheckHit, out RecheckPhase, Launcher, ApparentTarget, PhaseFrom, PenetrateCreatures, PenetrateWalls, Prospective);
	}

	private bool MatchInner(GameObject Object, GameObject Attacker, GameObject Launcher = null, GameObject ApparentTarget = null, GameObject PhaseFrom = null, bool PenetrateCreatures = false, bool PenetrateWalls = false, bool Prospective = false)
	{
		if (Object == null)
		{
			return false;
		}
		if (!TargetPart.IsNullOrEmpty() && Object.HasPart(TargetPart) && PhaseOkay(Object, PhaseFrom))
		{
			return true;
		}
		if (!TargetTag.IsNullOrEmpty() && Object.HasTag(TargetTag) && PhaseOkay(Object, PhaseFrom) && (TargetTagValue.IsNullOrEmpty() || Object.GetTag(TargetTag) == TargetTagValue))
		{
			return true;
		}
		if (RealityDistortionBased && PhaseOkay(Object, PhaseFrom))
		{
			foreach (RealityStabilized item in Object.YieldEffects<RealityStabilized>())
			{
				if (Prospective ? (item.Strength >= 70) : item.Strength.in100())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool Match(GameObject Object, GameObject Attacker, out bool RecheckHit, out bool RecheckPhase, GameObject Launcher = null, GameObject ApparentTarget = null, GameObject PhaseFrom = null, bool PenetrateCreatures = false, bool PenetrateWalls = false, bool Prospective = false, Cell FromCell = null)
	{
		RecheckHit = false;
		RecheckPhase = false;
		bool flag = MatchInner(Object, Attacker, Launcher, ApparentTarget, PhaseFrom, PenetrateCreatures, PenetrateWalls, Prospective);
		if (flag)
		{
			flag = BeforeProjectileHitEvent.Check(ParentObject, Attacker, Object, out var Recheck, out var RecheckPhase2, PenetrateCreatures, PenetrateWalls, Launcher, ApparentTarget, FromCell, this, LightBased: false, Prospective);
			if (RecheckPhase2)
			{
				RecheckPhase = true;
			}
			if (Recheck)
			{
				RecheckHit = true;
				return flag;
			}
		}
		if (flag && FromCell == null && !Prospective && !Message.IsNullOrEmpty() && Object.IsVisible())
		{
			IComponent<GameObject>.AddPlayerMessage(GameText.VariableReplace(Message, ParentObject, Object));
		}
		return flag;
	}

	public bool Match(GameObject Object, GameObject Attacker, GameObject Launcher = null, GameObject ApparentTarget = null, GameObject PhaseFrom = null, bool PenetrateCreatures = false, bool PenetrateWalls = false, bool Prospective = false, Cell FromCell = null)
	{
		int num = 0;
		bool flag;
		bool Recheck;
		do
		{
			flag = MatchInner(Object, Attacker, Launcher, ApparentTarget, PhaseFrom, PenetrateCreatures, PenetrateWalls, Prospective);
			if (!flag)
			{
				break;
			}
			flag = BeforeProjectileHitEvent.Check(ParentObject, Attacker, Object, out Recheck, out var _, PenetrateCreatures, PenetrateWalls, Launcher, ApparentTarget, FromCell, this, LightBased: false, Prospective);
		}
		while (Recheck && ++num < 100);
		if (flag && FromCell == null && !Prospective && !Message.IsNullOrEmpty() && Object.IsVisible())
		{
			IComponent<GameObject>.AddPlayerMessage(GameText.VariableReplace(Message, ParentObject, Object));
		}
		return flag;
	}

	private bool PhaseOkay(GameObject Object, GameObject PhaseFrom)
	{
		if (!RequiresPhaseMatch)
		{
			return true;
		}
		return PhaseFrom?.PhaseMatches(Object) ?? false;
	}

	public override bool SameAs(IPart p)
	{
		TreatAsSolid treatAsSolid = p as TreatAsSolid;
		if (treatAsSolid.TargetPart != TargetPart)
		{
			return false;
		}
		if (treatAsSolid.TargetTag != TargetTag)
		{
			return false;
		}
		if (treatAsSolid.TargetTagValue != TargetTagValue)
		{
			return false;
		}
		if (treatAsSolid.Message != Message)
		{
			return false;
		}
		if (treatAsSolid.RealityDistortionBased != RealityDistortionBased)
		{
			return false;
		}
		if (treatAsSolid.GazeBased != GazeBased)
		{
			return false;
		}
		if (treatAsSolid.LightBased != LightBased)
		{
			return false;
		}
		if (treatAsSolid.RequiresPhaseMatch != RequiresPhaseMatch)
		{
			return false;
		}
		if (treatAsSolid.Hits != Hits)
		{
			return false;
		}
		return base.SameAs(p);
	}
}
