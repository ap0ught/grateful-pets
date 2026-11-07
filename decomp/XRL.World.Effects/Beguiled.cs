using System;
using Qud.API;
using XRL.World.AI;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class Beguiled : Effect
{
	public GameObject Beguiler;

	public int Level;

	public int LevelApplied;

	public bool Independent;

	public Beguiled()
	{
		DisplayName = "{{m|beguiled}}";
		Duration = 1;
	}

	public Beguiled(GameObject Beguiler)
		: this()
	{
		this.Beguiler = Beguiler;
	}

	public Beguiled(GameObject Beguiler, int Level)
		: this(Beguiler)
	{
		this.Level = Level;
	}

	public Beguiled(GameObject Beguiler, int Level, bool Independent)
		: this(Beguiler, Level)
	{
		this.Independent = Independent;
	}

	public override int GetEffectType()
	{
		return 2;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		string text = "Charmed by another creature into following them.";
		if (Level != 0)
		{
			text = text + "\n+" + Level * 5 + " hit points.";
		}
		return text;
	}

	public override string GetDescription()
	{
		return "{{m|beguiled}}";
	}

	public override bool Apply(GameObject Object)
	{
		if (!GameObject.Validate(ref Beguiler))
		{
			return false;
		}
		if (Object.Brain == null)
		{
			return false;
		}
		if (!Object.FireEvent("CanApplyBeguile"))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyBeguile"))
		{
			return false;
		}
		if (!ApplyEffectEvent.Check(Object, "Beguile", this))
		{
			return false;
		}
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_charm");
		IComponent<GameObject>.XDidYToZ(Object, "ogle", Beguiler, "lovingly", null, null, null, Beguiler);
		if (Beguiler.IsPlayer() && !Beguiler.HasEffect<Dominated>())
		{
			JournalAPI.AddAccomplishment(Object.An() + " ogled you lovingly after you employed your charm.", "The storied eroticism of =name= became intimately known to " + Object.an() + ".", "<spice.elements." + The.Player.GetMythicDomain() + ".weddingConditions.!random.capitalize>, =name= cemented " + The.Player.GetPronounProvider().PossessiveAdjective + " friendship with " + Object.GetPrimaryFactionName(VisibleOnly: true, Formatted: true, Base: true) + " by marrying " + Object.an() + ".", null, "general", MuralCategory.Trysts, MuralWeight.Medium, null, -1L);
		}
		Object.Heartspray();
		Beguiling.SyncTarget(Beguiler, Object, Independent);
		Object.SetAlliedLeader<AllyBeguile>(Beguiler);
		ApplyBeguilement();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (GameObject.Validate(ref Beguiler) && Object.PartyLeader == Beguiler && !Beguiler.SupportsFollower(Object, 13))
		{
			Object.Brain.PartyLeader = null;
			Object.Brain.Goals.Clear();
			DidXToY("lose", "interest in", Beguiler, null, null, null, null, null, Beguiler);
		}
		Object.Brain.RemoveAllegiance<AllyBeguile>(Beguiler?.BaseID ?? 0);
		UnapplyBeguilement();
		Beguiler = null;
		base.Remove(Object);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		Registrar.Register("BeginTakeAction");
		base.Register(Object, Registrar);
	}

	public Beguiling GetMutation()
	{
		if (!GameObject.Validate(ref Beguiler))
		{
			return null;
		}
		Beguiling part = Beguiler.GetPart<Beguiling>();
		if (part == null)
		{
			return null;
		}
		if (part.RealityDistortionBased && !CheckMyRealityDistortionUsability())
		{
			return null;
		}
		return part;
	}

	public void ApplyBeguilement()
	{
		int num = Level - LevelApplied;
		if (Object.HasStat("Hitpoints"))
		{
			Object.Statistics["Hitpoints"].BaseValue += 5 * num;
		}
		if (Object.Brain != null && GameObject.Validate(ref Beguiler))
		{
			Object.Brain.AddOpinion<OpinionBeguile>(Beguiler);
		}
		LevelApplied = Level;
	}

	public void UnapplyBeguilement()
	{
		if (LevelApplied != 0)
		{
			if (Object.HasStat("Hitpoints"))
			{
				Object.Statistics["Hitpoints"].BaseValue -= 5 * LevelApplied;
			}
			if (Object.Brain != null && GameObject.Validate(ref Beguiler))
			{
				Object.Brain.RemoveOpinion<OpinionBeguile>(Beguiler);
			}
			LevelApplied = 0;
		}
	}

	public void SyncToMutation()
	{
		if (!Independent)
		{
			Beguiling mutation = GetMutation();
			if (mutation == null)
			{
				Duration = 0;
			}
			else if (mutation.Level != Level)
			{
				Level = mutation.Level;
				ApplyBeguilement();
			}
		}
	}

	public bool IsSupported()
	{
		if (GameObject.Validate(ref Beguiler))
		{
			return Beguiler.SupportsFollower(Object, 2);
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (!IsSupported())
			{
				Duration = 0;
			}
			else
			{
				SyncToMutation();
			}
		}
		else if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyBeguilement();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyBeguilement();
		}
		return base.FireEvent(E);
	}
}
