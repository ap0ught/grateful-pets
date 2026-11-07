using System;
using System.Collections.Generic;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class ArtifactDetection : IActivePart
{
	public const int RADIUS_ZONE = 80;

	public int Radius;

	public int IdentifyChance;

	public bool Identify;

	public bool DetectKnown;

	[NonSerialized]
	private List<Examiner> Artifacts = new List<Examiner>();

	public ArtifactDetection()
	{
		WorksOnHolder = true;
		WorksOnCarrier = true;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		DetectArtifacts(UseCharge: true, Amount);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetShortDescriptionEvent.ID && ID != TakenEvent.ID)
		{
			return ID == DroppedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (Radius < 80)
		{
			E.Postfix.Compound("{{rules|You detect unidentified artifacts within a radius of ", '\n').Append(Radius).Append(".}}");
		}
		else
		{
			E.Postfix.Compound("{{rules|You detect all unidentified artifacts on the local map.}}", '\n');
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TakenEvent E)
	{
		DetectArtifacts();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(DroppedEvent E)
	{
		DetectArtifacts();
		return base.HandleEvent(E);
	}

	public List<Examiner> GetNearbyArtifacts(Cell C)
	{
		if (Radius >= 80)
		{
			foreach (GameObject item in C.ParentZone.YieldObjects())
			{
				if (item.TryGetPart<Examiner>(out var Part) && IsDetectable(Part))
				{
					Artifacts.Add(Part);
				}
			}
		}
		else
		{
			foreach (Cell item2 in C.YieldAdjacentCells(Radius, LocalOnly: true))
			{
				foreach (GameObject @object in item2.Objects)
				{
					if (@object.TryGetPart<Examiner>(out var Part2) && IsDetectable(Part2))
					{
						Artifacts.Add(Part2);
					}
				}
			}
		}
		return Artifacts;
	}

	public bool IsDetectable(Examiner Artifact)
	{
		if (Artifact.Complexity > 0 && !Artifact.KeepTile && (DetectKnown || Artifact.GetEpistemicStatus() != 2))
		{
			return Artifact.ParentObject.CanHypersensesDetect();
		}
		return false;
	}

	public void DetectArtifacts(bool UseCharge = false, int MultipleCharge = 1)
	{
		Artifacts.Clear();
		GameObject activePartFirstSubject = GetActivePartFirstSubject();
		Cell cell = activePartFirstSubject?.CurrentCell;
		if (cell == null || !activePartFirstSubject.IsPlayer() || cell.OnWorldMap())
		{
			LastStatus = ActivePartStatus.NeedsSubject;
		}
		else
		{
			if (IsDisabled(UseCharge, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, MultipleCharge, null, UseChargeIfUnpowered: false, 0L))
			{
				return;
			}
			foreach (Examiner nearbyArtifact in GetNearbyArtifacts(cell))
			{
				if (!nearbyArtifact.ParentObject.HasEffect<ArtifactDetectionEffect>())
				{
					ArtifactDetectionEffect e = new ArtifactDetectionEffect(this, nearbyArtifact)
					{
						Source = ParentObject,
						Radius = ((Radius < 80) ? Radius : 9999),
						IdentifyChance = (Identify ? IdentifyChance : 0)
					};
					nearbyArtifact.ParentObject.ApplyEffect(e);
				}
			}
		}
	}
}
