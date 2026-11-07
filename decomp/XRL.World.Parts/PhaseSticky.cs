using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class PhaseSticky : IPart
{
	public static readonly int ICON_COLOR_PRIORITY = 30;

	public bool DestroyOnBreak;

	[Obsolete("Retired sticky max weight checking")]
	public int MaxWeight = 1000;

	public int SaveTarget = 15;

	public int Duration = 12;

	private int FrameOffset;

	private int FlickerFrame;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID && ID != GetNavigationWeightEvent.ID && ID != LeftCellEvent.ID && ID != ObjectEnteredCellEvent.ID && ID != OnDestroyObjectEvent.ID)
		{
			return ID == PooledEvent<RealityStabilizeEvent>.ID;
		}
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int num = 0;
			if (ParentObject.HasTag("Astral"))
			{
				ParentObject.Render.Tile = null;
				num = (XRLCore.CurrentFrame + FrameOffset) % 400;
				if (num < 4)
				{
					ParentObject.Render.ColorString = "&Y";
					ParentObject.Render.DetailColor = "k";
				}
				else if (num < 8)
				{
					ParentObject.Render.ColorString = "&y";
					ParentObject.Render.DetailColor = "K";
				}
				else if (num < 12)
				{
					ParentObject.Render.ColorString = "&k";
					ParentObject.Render.DetailColor = "y";
				}
				else
				{
					ParentObject.Render.ColorString = "&K";
					ParentObject.Render.DetailColor = "y";
				}
				if (!Options.DisableTextAnimationEffects)
				{
					FrameOffset += Stat.Random(0, 20);
				}
				if (Stat.Random(1, 400) == 1 || FlickerFrame > 0)
				{
					ParentObject.Render.ColorString = "&K";
				}
			}
			else
			{
				num = (XRLCore.CurrentFrame + FrameOffset) % 60;
				num /= 2;
				num %= 6;
				string text = null;
				switch (num)
				{
				case 0:
					text = "&k";
					break;
				case 1:
					text = "&K";
					break;
				case 2:
					text = "&c";
					break;
				case 4:
					text = "&y";
					break;
				}
				if (!text.IsNullOrEmpty())
				{
					E.ApplyColors(text, ICON_COLOR_PRIORITY);
				}
			}
		}
		return base.Render(E);
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (E.Smart && !E.Juggernaut && E.PhaseMatches(ParentObject))
		{
			E.MinWeight(E.Autoexploring ? 75 : 2);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RealityStabilizeEvent E)
	{
		if (E.Check())
		{
			Sticky sticky = ParentObject.RequirePart<Sticky>();
			sticky.DestroyOnBreak = DestroyOnBreak;
			sticky.SaveTarget = SaveTarget;
			sticky.Duration = Duration;
			ParentObject.RemovePart(this);
			if (ParentObject.DisplayNameOnlyDirect == "phase web")
			{
				ParentObject.DisplayName = "web";
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (!E.Object.HasEffect<Greased>() && E.Object.GetMatterPhase() <= 1 && !E.Object.HasTag("ExcavatoryTerrainFeature") && E.Object.PhaseMatches(ParentObject) && !ParentObject.IsBroken() && !ParentObject.IsRusted() && E.Object.ApplyEffect(new Stuck(12, SaveTarget, "Web Stuck Restraint", DestroyOnBreak ? ParentObject : null, "stuck", "in", ParentObject.ID)))
		{
			E.Object.ApplyEffect(new PhasedWhileStuck(4));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		StripStuck(E.Cell);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		StripStuck(ParentObject.CurrentCell);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ApplyStuck");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyStuck")
		{
			return false;
		}
		return base.FireEvent(E);
	}

	private bool IsOurs(Effect GFX)
	{
		if (GFX is Stuck stuck && stuck.DestroyOnBreak == ParentObject)
		{
			return true;
		}
		return false;
	}

	private void StripStuck(Cell C)
	{
		if (C == null)
		{
			return;
		}
		int i = 0;
		for (int count = C.Objects.Count; i < count; i++)
		{
			Effect effect = C.Objects[i].GetEffect(typeof(Stuck), IsOurs);
			if (effect != null)
			{
				effect.Duration = 0;
			}
		}
	}
}
