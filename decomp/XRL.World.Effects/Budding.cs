using System;
using XRL.Rules;
using XRL.World.Anatomy;
using XRL.World.Capabilities;

namespace XRL.World.Effects;

[Serializable]
public class Budding : Effect, ITierInitialized
{
	public const string DEFAULT_REPLICATION_CONTEXT = "Budding";

	public int numClones = 1;

	public int baseDuration = 20;

	public string ActorID;

	public string ReplicationContext = "Budding";

	public Budding()
	{
		Duration = 20;
		DisplayName = "{{r|budding}}";
	}

	public Budding(GameObject Actor = null, int numClones = 1, string ReplicationContext = "Budding")
		: this()
	{
		ActorID = Actor?.ID;
		this.numClones = numClones;
		this.ReplicationContext = ReplicationContext;
	}

	public Budding(string ActorID = null, int numClones = 1, string ReplicationContext = "Budding")
		: this()
	{
		this.ActorID = ActorID;
		this.numClones = numClones;
		this.ReplicationContext = ReplicationContext;
	}

	public void Initialize(int Tier)
	{
		numClones = Stat.Random(1, 3);
	}

	public override int GetEffectType()
	{
		return 67108880;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		return "{{r|budding}}";
	}

	public override string GetStateDescription()
	{
		return "{{r|about to bud}}";
	}

	public override string GetDetails()
	{
		return "Will spawn a clone soon.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect<Budding>())
		{
			return false;
		}
		if (!Cloning.CanBeCloned(Object, GameObject.FindByID(ActorID), ReplicationContext))
		{
			return false;
		}
		BodyPart bodyPart = Object.Body?.GetFirstPart("Back");
		if (Visible())
		{
			Object.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_budding");
			if (bodyPart != null)
			{
				IComponent<GameObject>.AddPlayerMessage("A grotesque protuberance swells from " + Object.poss(bodyPart.GetOrdinalName()) + " as " + Object.does("begin", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + " to bud!");
			}
			else
			{
				IComponent<GameObject>.AddPlayerMessage("A grotesque protuberance swells from " + Object.t() + " as " + Object.does("begin", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + " to bud!");
			}
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (numClones > 0)
		{
			BodyPart bodyPart = Object.Body?.GetFirstPart("Back");
			if (Visible())
			{
				if (bodyPart != null)
				{
					IComponent<GameObject>.AddPlayerMessage("The grotesque protuberance on " + Object.poss(bodyPart.GetOrdinalName()) + " subsides.");
				}
				else
				{
					IComponent<GameObject>.AddPlayerMessage(Object.Poss("grotesque protuberance") + " subsides.");
				}
			}
		}
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != SingletonEvent<BeforeBeginTakeActionEvent>.ID || Object?.Brain == null))
		{
			if (ID == SingletonEvent<EndTurnEvent>.ID)
			{
				return Object?.Brain == null;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(BeforeBeginTakeActionEvent E)
	{
		if (Object?.Brain != null)
		{
			ProcessTurn();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (Object?.Brain == null)
		{
			ProcessTurn();
		}
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Recuperating")
		{
			Object.RemoveEffect(this);
		}
		return base.FireEvent(E);
	}

	public void ProcessTurn()
	{
		Zone zone = Object?.CurrentZone;
		if (zone == null || !zone.IsActive() || zone.IsWorldMap() || Duration <= 0 || Duration == 9999)
		{
			return;
		}
		Duration--;
		if (Duration > 0 || !Cloning.CanBeCloned(Object, null, ReplicationContext))
		{
			return;
		}
		Object.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_budding_clone_complete");
		if (Cloning.GenerateBuddedClone(Object, GameObject.FindByID(ActorID), DuplicateGear: false, BecomesCompanion: true, 1, ReplicationContext) != null)
		{
			numClones--;
			if (numClones > 0)
			{
				Duration = baseDuration;
			}
		}
		else
		{
			Duration = 1;
		}
	}
}
