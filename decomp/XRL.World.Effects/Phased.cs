using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class Phased : Effect, ITierInitialized
{
	private int FrameOffset;

	private int FlickerFrame;

	public string Tile;

	public string RenderString = "@";

	public bool WasPhased;

	public Phased()
	{
		DisplayName = "{{g|phased}}";
		FrameOffset = Stat.Random(1, 60);
	}

	public Phased(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(20, 100);
	}

	public Phased(Phased Source)
		: this()
	{
		Duration = Source.Duration;
		FrameOffset = Source.FrameOffset;
		FlickerFrame = Source.FlickerFrame;
		Tile = Source.Tile;
		RenderString = Source.RenderString;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 256;
	}

	public override bool SameAs(Effect e)
	{
		Phased phased = e as Phased;
		if (phased.Tile != Tile)
		{
			return false;
		}
		if (phased.RenderString != RenderString)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeginTakeActionEvent>.ID && (ID != EffectAppliedEvent.ID || WasPhased) && ID != EnteredCellEvent.ID && ID != PooledEvent<RealityStabilizeEvent>.ID)
		{
			return ID == WasDerivedFromEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		if (!WasPhased && E.Effect == this)
		{
			Object.PlayWorldSound("Sounds/Misc/sfx_phase_shiftOut");
			if (Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You phase out.");
			}
			Object.FireEvent("AfterPhaseOut");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(WasDerivedFromEvent E)
	{
		E.Derivation.ForceApplyEffect(new Phased(this));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (Duration > 0 && Duration != 9999 && Object.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("You will phase back in in " + Duration.Things("round") + ".");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (Duration != 9999 && E.Cell.OnWorldMap())
		{
			Object.RemoveEffect(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RealityStabilizeEvent E)
	{
		if (E.Check(CanDestroy: true))
		{
			GameObject gameObject = Object;
			gameObject.RemoveEffect(this);
			if (!gameObject.IsValid())
			{
				return false;
			}
			if (gameObject.GetPhase() == 1)
			{
				gameObject.TakeDamage("2d6".RollCached(), "from being forced into phase.", "Normality Phase Unavoidable", null, null, null, E.Effect.Owner);
				if (!gameObject.IsValid())
				{
					return false;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override string GetDetails()
	{
		return "Can't physically interact with creatures and objects unless they're also phased.\nCan pass through solids.";
	}

	public override bool Apply(GameObject Object)
	{
		WasPhased = Object.HasEffect<Phased>();
		if (!Object.FireEvent("ApplyPhased"))
		{
			return false;
		}
		Tile = Object.Render.Tile;
		RenderString = Object.Render.RenderString;
		FlushNavigationCaches();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (!Object.HasEffectOtherThan(typeof(Phased), this))
		{
			Object.PlayWorldSound("Sounds/Misc/sfx_phase_shiftIn");
			if (Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You phase back in.");
			}
			if (Object.CurrentCell != null && !Object.OnWorldMap())
			{
				foreach (GameObject item in Object.CurrentCell.GetObjectsWithPart("Physics"))
				{
					if (item == Object || !item.Physics.Solid || (item.HasTagOrProperty("Flyover") && Object.IsFlying))
					{
						continue;
					}
					List<Cell> list = new List<Cell>(8);
					Object.CurrentCell.GetAdjacentCells(1, list, LocalOnly: false);
					Cell cell = null;
					for (int i = 0; i < list.Count; i++)
					{
						cell = list[i];
						for (int j = 0; j < list[i].Objects.Count; j++)
						{
							if ((!list[i].Objects[j].HasPart<Forcefield>() || list[i].Objects[j].GetPart<Forcefield>().Creator != Object || !list[i].Objects[j].GetPart<Forcefield>().MovesWithOwner) && list[i].Objects[j].Physics != null && list[i].Objects[j].Physics.Solid && (!list[i].Objects[j].HasTagOrProperty("Flyover") || !Object.IsFlying))
							{
								cell = null;
								break;
							}
						}
						if (cell != null)
						{
							break;
						}
					}
					if (cell == null)
					{
						if (Object.IsPlayer())
						{
							Achievement.VIOLATE_PAULI.Unlock();
						}
						Object.DilationSplat();
						Object.Die(item, null, "You violated the Pauli exclusion principle.", Object.It + " @@violated the Pauli exclusion principle.", Accidental: true);
						continue;
					}
					Object.DirectMoveTo(cell);
					break;
				}
			}
			FlushNavigationCaches();
			Object.FireEvent("AfterPhaseIn");
			Object.Gravitate();
		}
		base.Remove(Object);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int num = 0;
			if (Object.HasTag("Astral"))
			{
				Object.Render.Tile = null;
				num = (XRLCore.CurrentFrame + FrameOffset) % 400;
				if (Stat.Random(1, 400) == 1 || FlickerFrame > 0)
				{
					if (FlickerFrame == 0)
					{
						Object.Render.RenderString = "_";
					}
					else if (FlickerFrame == 1)
					{
						Object.Render.RenderString = "-";
					}
					else if (FlickerFrame == 2)
					{
						Object.Render.RenderString = "|";
					}
					E.ColorString = "&K";
					if (FlickerFrame == 0)
					{
						FlickerFrame = 3;
					}
					FlickerFrame--;
				}
				else
				{
					Object.Render.RenderString = RenderString;
					Object.Render.Tile = Tile;
				}
				if (num < 4)
				{
					Object.Render.ColorString = "&Y";
					Object.Render.DetailColor = "k";
				}
				else if (num < 8)
				{
					Object.Render.ColorString = "&y";
					Object.Render.DetailColor = "K";
				}
				else if (num < 12)
				{
					Object.Render.ColorString = "&k";
					Object.Render.DetailColor = "y";
				}
				else
				{
					Object.Render.ColorString = "&K";
					Object.Render.DetailColor = "y";
				}
				if (!Options.DisableTextAnimationEffects)
				{
					FrameOffset += Stat.Random(0, 20);
				}
				if (Stat.Random(1, 400) == 1 || FlickerFrame > 0)
				{
					Object.Render.ColorString = "&K";
				}
				return true;
			}
			num = (XRLCore.CurrentFrame + FrameOffset) % 60;
			num /= 2;
			num %= 6;
			if (num == 0)
			{
				E.ApplyColors("&k", 100);
			}
			if (num == 1)
			{
				E.ApplyColors("&K", 100);
			}
			if (num == 2)
			{
				E.ApplyColors("&c", 100);
			}
			if (num == 4)
			{
				E.ApplyColors("&y", 100);
			}
		}
		return true;
	}

	public override bool allowCopyOnNoEffectDeepCopy()
	{
		return true;
	}
}
