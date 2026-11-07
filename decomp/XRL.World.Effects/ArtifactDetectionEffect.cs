using System;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class ArtifactDetectionEffect : Effect
{
	public int Radius = 1;

	public int IdentifyChance;

	public bool Identified;

	public GameObject Source;

	[NonSerialized]
	private ArtifactDetection Detector;

	[NonSerialized]
	private Examiner Examiner;

	[NonSerialized]
	private bool Initialized;

	public ArtifactDetectionEffect()
	{
		Duration = 1;
	}

	public ArtifactDetectionEffect(ArtifactDetection Detector, Examiner Examiner)
		: this()
	{
		this.Detector = Detector;
		this.Examiner = Examiner;
		Initialized = Examiner != null && Detector != null;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool SuppressInLookDisplay()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 64;
	}

	public override string GetDescription()
	{
		return null;
	}

	public bool Initialize()
	{
		if (Initialized)
		{
			return true;
		}
		Initialized = true;
		Detector = Source.GetPart<ArtifactDetection>();
		Examiner = Object.GetPart<Examiner>();
		if (Examiner != null)
		{
			return Detector != null;
		}
		return false;
	}

	public bool CheckDetection()
	{
		if (!GameObject.Validate(ref Source) || Object.CurrentCell == null)
		{
			return Unapply();
		}
		if (!Initialize() || !Detector.WasReady())
		{
			return Unapply();
		}
		if (!Detector.DetectKnown && Examiner.GetEpistemicStatus() == 2)
		{
			return Unapply();
		}
		int num = Object.DistanceTo(The.Player);
		if (num > Radius)
		{
			return Unapply();
		}
		if (!Identified && IdentifyChance > 0 && ((int)((double)(100 + IdentifyChance) / Math.Pow(num + 9, 2.0) * 100.0)).in100())
		{
			Identified = true;
		}
		return true;
	}

	public override bool Apply(GameObject Object)
	{
		if (!Initialize())
		{
			return false;
		}
		if (Examiner.Complexity < 0)
		{
			return false;
		}
		return CheckDetection();
	}

	private bool Unapply()
	{
		Source = null;
		Object.RemoveEffect(this);
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<EndTurnEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		CheckDetection();
		return base.HandleEvent(E);
	}

	public bool ValidTarget(GameObject Object)
	{
		Cell cell = Object.CurrentCell;
		if (cell == null)
		{
			Unapply();
			return false;
		}
		if (!Object.IsVisible())
		{
			return true;
		}
		if (!cell.IsLit() || !cell.IsExplored())
		{
			return true;
		}
		return false;
	}

	public override bool FinalRender(RenderEvent E)
	{
		if (E.CustomDraw)
		{
			return true;
		}
		if (!ValidTarget(Object))
		{
			return true;
		}
		if (!Initialize())
		{
			Unapply();
			return true;
		}
		if (!Detector.WasReady())
		{
			Unapply();
			return true;
		}
		if (Identified)
		{
			E.HighestLayer = 0;
			E.RenderString = Object.Render.RenderString;
			E.Tile = (Options.UseTiles ? Object.Render.Tile : null);
			Object.ComponentRender(E);
		}
		else
		{
			Examiner.Render(E);
		}
		E.CustomDraw = true;
		return false;
	}
}
