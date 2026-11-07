using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class Scintillating : Effect, ITierInitialized
{
	public int Level;

	public string Affected;

	public Scintillating()
	{
		DisplayName = "{{rainbow|scintillating}}";
	}

	public Scintillating(int Duration, int Level = 1)
		: this()
	{
		base.Duration = Duration;
		this.Level = Level;
	}

	public void Initialize(int Tier)
	{
		Tier = Stat.Random(Tier - 2, Tier + 2);
		if (Tier < 1)
		{
			Tier = 1;
		}
		if (Tier > 8)
		{
			Tier = 8;
		}
		Duration = 5;
		Level = (int)((double)Tier * 1.5);
	}

	public override int GetEffectType()
	{
		return 128;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		return "{{rainbow|scintillating}}";
	}

	public override string GetDetails()
	{
		return "Cannot take actions.\nConfuses nearby hostile creatures per Confusion 7.";
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect<Scintillating>())
		{
			return false;
		}
		if (!Object.FireEvent("ApplyScintillating"))
		{
			return false;
		}
		DidX("start", "scintillating in {{rainbow|prismatic hues}}", "!");
		Object.ParticleText("*scintillating*", IComponent<GameObject>.ConsequentialColorChar(Object));
		Object.ForfeitTurn();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		DidX("stop", "scintillating");
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeforeBeginTakeActionEvent>.ID)
		{
			return ID == SingletonEvent<BeginTakeActionEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeBeginTakeActionEvent E)
	{
		ConfuseHostiles();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("IsMobile");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "IsMobile" && Duration > 0)
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int currentFrame = XRLCore.CurrentFrame;
			string text = null;
			string text2 = null;
			if (currentFrame < 5)
			{
				text = "&r";
				text2 = "R";
			}
			else if (currentFrame <= 10)
			{
				text = "&W";
				text2 = "w";
			}
			else if (currentFrame <= 15)
			{
				text = "&G";
				text2 = "g";
			}
			else if (currentFrame <= 20)
			{
				text = "&C";
				text2 = "c";
			}
			else if (currentFrame <= 25)
			{
				text = "&B";
				text2 = "b";
			}
			else if (currentFrame <= 35)
			{
				text = "&M";
				text2 = "m";
			}
			else if (currentFrame <= 40)
			{
				text = "&B";
				text2 = "b";
			}
			else if (currentFrame <= 45)
			{
				text = "&C";
				text2 = "c";
			}
			else if (currentFrame <= 50)
			{
				text = "&G";
				text2 = "g";
			}
			else if (currentFrame < 55)
			{
				text = "&W";
				text2 = "w";
			}
			else
			{
				text = "&r";
				text2 = "R";
			}
			if (!text.IsNullOrEmpty() || !text2.IsNullOrEmpty())
			{
				E.ApplyColors(text, text2, Effect.ICON_COLOR_PRIORITY, Effect.ICON_COLOR_PRIORITY);
			}
		}
		return true;
	}

	public void ConfuseHostiles()
	{
		List<GameObject> list = Object.CurrentZone?.FindObjectsWithPart("Brain");
		if (list == null || list.Count == 0)
		{
			return;
		}
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			GameObject gameObject = list[i];
			if ((gameObject.IsHostileTowards(Object) || gameObject.IsHostileTowards(Object.PartyLeader)) && !HasAffected(gameObject) && gameObject.Brain.CheckVisibilityOf(Object))
			{
				int level = Confusion.GetConfusionLevel(Level);
				int penalty = Confusion.GetMentalPenalty(Level);
				if (PerformMentalAttack((MentalAttackEvent E) => Confusion.Confuse(E, Attack: false, level, penalty), Object, gameObject, null, "Confuse", "1d8", 4, Confusion.GetDuration(Level).RollCached(), int.MinValue, Level))
				{
					MarkAffected(gameObject);
				}
			}
		}
	}

	public bool HasAffected(GameObject Object)
	{
		if (Affected == null)
		{
			return false;
		}
		if (!Object.HasID)
		{
			return false;
		}
		return Affected.CachedCommaExpansion().Contains(Object.ID);
	}

	public void MarkAffected(GameObject Object)
	{
		if (Affected == null)
		{
			Affected = Object.ID;
		}
		else
		{
			Affected = Affected + "," + Object.ID;
		}
	}
}
