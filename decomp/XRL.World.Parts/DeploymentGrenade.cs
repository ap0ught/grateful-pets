using System;
using System.Collections.Generic;
using XRL.World.AI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class DeploymentGrenade : IGrenade
{
	public int Radius = 1;

	public int Chance = 100;

	public int AtLeast;

	public string Count;

	public string Duration;

	public string Blueprint = "Forcefield";

	public string UsabilityEvent;

	public string AccessibilityEvent;

	public string ActivationVerb = "detonate";

	public string PhaseDuration;

	public string OmniphaseDuration;

	public bool RealRadius;

	public bool BlockedBySolid = true;

	public bool BlockedByNonEmpty = true;

	public bool Seeping;

	public bool DustPuff = true;

	public bool DustPuffEach;

	public bool NoXPValue = true;

	public bool LoyalToThrower;

	public bool TriflingCompanion = true;

	public bool IsRealityDistortionBased;

	public int RealityStabilizationPenetration;

	public override bool SameAs(IPart p)
	{
		DeploymentGrenade deploymentGrenade = p as DeploymentGrenade;
		if (deploymentGrenade.Radius != Radius)
		{
			return false;
		}
		if (deploymentGrenade.Chance != Chance)
		{
			return false;
		}
		if (deploymentGrenade.AtLeast != AtLeast)
		{
			return false;
		}
		if (deploymentGrenade.Count != Count)
		{
			return false;
		}
		if (deploymentGrenade.Duration != Duration)
		{
			return false;
		}
		if (deploymentGrenade.Blueprint != Blueprint)
		{
			return false;
		}
		if (deploymentGrenade.UsabilityEvent != UsabilityEvent)
		{
			return false;
		}
		if (deploymentGrenade.AccessibilityEvent != AccessibilityEvent)
		{
			return false;
		}
		if (deploymentGrenade.ActivationVerb != ActivationVerb)
		{
			return false;
		}
		if (deploymentGrenade.PhaseDuration != PhaseDuration)
		{
			return false;
		}
		if (deploymentGrenade.OmniphaseDuration != OmniphaseDuration)
		{
			return false;
		}
		if (deploymentGrenade.RealRadius != RealRadius)
		{
			return false;
		}
		if (deploymentGrenade.BlockedBySolid != BlockedBySolid)
		{
			return false;
		}
		if (deploymentGrenade.BlockedByNonEmpty != BlockedByNonEmpty)
		{
			return false;
		}
		if (deploymentGrenade.Seeping != Seeping)
		{
			return false;
		}
		if (deploymentGrenade.DustPuff != DustPuff)
		{
			return false;
		}
		if (deploymentGrenade.DustPuffEach != DustPuffEach)
		{
			return false;
		}
		if (deploymentGrenade.NoXPValue != NoXPValue)
		{
			return false;
		}
		if (deploymentGrenade.LoyalToThrower != LoyalToThrower)
		{
			return false;
		}
		if (deploymentGrenade.TriflingCompanion != TriflingCompanion)
		{
			return false;
		}
		if (deploymentGrenade.IsRealityDistortionBased != IsRealityDistortionBased)
		{
			return false;
		}
		if (deploymentGrenade.RealityStabilizationPenetration != RealityStabilizationPenetration)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetComponentNavigationWeightEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetComponentNavigationWeightEvent E)
	{
		E.MinWeight(2);
		return base.HandleEvent(E);
	}

	private bool CanDeploy(Cell Cell, Cell BaseCell, Event Check, Dictionary<int, bool> Track, GameObject Actor, int Phase)
	{
		if (!Chance.in100())
		{
			return false;
		}
		if (RealRadius && Cell != BaseCell && Cell.RealDistanceTo(BaseCell) > (double)Radius)
		{
			return false;
		}
		if (BlockedBySolid && Cell.IsSolid(Seeping, Phase))
		{
			return false;
		}
		if (BlockedByNonEmpty && !Cell.IsEmptyForPopulation())
		{
			return false;
		}
		if (Track != null && Track.ContainsKey(Cell.LocalCoordKey))
		{
			return false;
		}
		if (Check != null && !Cell.FireEvent(Check))
		{
			return false;
		}
		if (IsRealityDistortionBased)
		{
			IComponent<GameObject>.CheckRealityDistortionAccessibility(ParentObject, Cell, Actor, ParentObject, null, RealityStabilizationPenetration);
		}
		return true;
	}

	private void Deploy(Cell Cell, GameObject Object, Dictionary<int, bool> Track, GameObject Actor, int Phase)
	{
		if (!Duration.IsNullOrEmpty())
		{
			Object.AddPart(new Temporary(Duration.RollCached()));
		}
		switch (Phase)
		{
		case 2:
			Object.ForceApplyEffect(new Phased(PhaseDuration.IsNullOrEmpty() ? 9999 : PhaseDuration.RollCached()));
			break;
		case 3:
			Object.ForceApplyEffect(new Omniphase(OmniphaseDuration.IsNullOrEmpty() ? 9999 : OmniphaseDuration.RollCached()));
			break;
		}
		Cell.AddObject(Object);
		Object.MakeActive();
		if (NoXPValue && Object.HasStat("XPValue"))
		{
			Object.GetStat("XPValue").BaseValue = 0;
		}
		if (LoyalToThrower && Actor != null)
		{
			Object.SetAlliedLeader<AllyConstructed>(Actor);
			Object.IsTrifling = TriflingCompanion;
		}
		if (DustPuffEach)
		{
			Object.DustPuff();
		}
		if (Track != null)
		{
			Track[Cell.LocalCoordKey] = true;
		}
	}

	private void Deploy(Cell Cell, Dictionary<int, bool> Track, GameObject Actor, int Phase)
	{
		GameObjectFactory.ProcessSpecification(Blueprint, (GameObject obj) =>
		{
			Deploy(Cell, obj, Track, Actor, Phase);
		}, null, 1, 0, 0, null, "DeploymentGrenade");
	}

	private void Deploy(List<Cell> Cells, Cell BaseCell, Event Check, Dictionary<int, bool> Track, GameObject Actor, int Phase)
	{
		GameObjectFactory.ProcessSpecification(Blueprint, (GameObject obj) =>
		{
			int num = 0;
			Cell cell;
			do
			{
				cell = Cells.GetRandomElement();
				if (cell == null)
				{
					break;
				}
				if (!CanDeploy(cell, BaseCell, Check, Track, Actor, Phase))
				{
					cell = null;
				}
			}
			while (cell == null && ++num < 10);
			if (cell != null)
			{
				Deploy(cell, obj, Track, Actor, Phase);
				Cells.Remove(cell);
			}
			else
			{
				obj.Obliterate();
			}
		}, null, 1, 0, 0, null, "DeploymentGrenade");
	}

	protected override bool DoDetonate(Cell BaseCell, GameObject Actor = null, GameObject ApparentTarget = null, bool Indirect = false)
	{
		if (!UsabilityEvent.IsNullOrEmpty() && !ParentObject.FireEvent(UsabilityEvent))
		{
			return false;
		}
		if (IsRealityDistortionBased)
		{
			GameObject parentObject = ParentObject;
			GameObject parentObject2 = ParentObject;
			int realityStabilizationPenetration = RealityStabilizationPenetration;
			if (!IComponent<GameObject>.CheckRealityDistortionUsability(parentObject, null, Actor, parentObject2, null, null, realityStabilizationPenetration))
			{
				return false;
			}
		}
		PlayWorldSound(GetPropertyOrTag("DetonatedSound"), 1f, 0f, Combat: true);
		DidX(ActivationVerb, null, "!");
		int phase = ParentObject.GetPhase();
		Event check = ((!AccessibilityEvent.IsNullOrEmpty()) ? Event.New(AccessibilityEvent) : null);
		Dictionary<int, bool> dictionary = ((AtLeast > 0 || !Count.IsNullOrEmpty()) ? new Dictionary<int, bool>() : null);
		List<Cell> localAdjacentCells = BaseCell.GetLocalAdjacentCells(Radius);
		if (Count.IsNullOrEmpty())
		{
			int num = 0;
			do
			{
				if (num == 1)
				{
					localAdjacentCells.ShuffleInPlace();
				}
				if (CanDeploy(BaseCell, BaseCell, check, dictionary, Actor, phase))
				{
					Deploy(BaseCell, dictionary, Actor, phase);
				}
				foreach (Cell item in localAdjacentCells)
				{
					if (num > 0 && dictionary.Count >= AtLeast)
					{
						break;
					}
					if (CanDeploy(item, BaseCell, check, dictionary, Actor, phase))
					{
						Deploy(item, dictionary, Actor, phase);
					}
				}
			}
			while (dictionary != null && dictionary.Count < AtLeast && ++num < 10);
		}
		else
		{
			int num2 = Count.RollCached();
			for (int i = 0; i < num2; i++)
			{
				Deploy(localAdjacentCells, BaseCell, check, dictionary, Actor, phase);
			}
		}
		if (DustPuff)
		{
			ParentObject.DustPuff();
		}
		ParentObject.Destroy(null, Silent: true);
		return true;
	}
}
