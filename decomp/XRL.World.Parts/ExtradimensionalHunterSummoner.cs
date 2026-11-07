using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Rules;
using XRL.World.AI;

namespace XRL.World.Parts;

[Serializable]
public class ExtradimensionalHunterSummoner : IPart
{
	public int CultChance = 50;

	public int ActiveOnDeathChance = 20;

	public string NumberHuntersStart = "3";

	public string NumberHuntersOngoing = "1-3";

	public string Cooldown = "150-200";

	public int Counter;

	public bool Active;

	[NonSerialized]
	private bool Registered;

	public override void Initialize()
	{
		Counter = Cooldown.RollCached();
	}

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			if (!Active)
			{
				return ID == TookDamageEvent.ID;
			}
			return false;
		}
		return true;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		Tick(Amount);
	}

	public void Tick(int Turns)
	{
		if (!Active)
		{
			if (!Registered)
			{
				RegisterClamDeath();
			}
		}
		else if (ParentObject.IsHostileTowards(The.Player))
		{
			Counter -= Turns;
			if (Counter <= 0)
			{
				Counter = Cooldown.RollCached();
				Summon(Stat.Roll(NumberHuntersOngoing));
			}
		}
	}

	public List<GameObject> GetClams()
	{
		return ParentObject.CurrentZone.GetObjects("Giant Clam");
	}

	public void Summon(int Number = 1)
	{
		List<GameObject> list = new List<GameObject>();
		List<GameObject> clams = GetClams();
		for (int i = 0; i < Number; i++)
		{
			if (CultChance.in100())
			{
				PsychicHunterSystem.CreateExtradimensionalCultHunters(ParentObject.CurrentZone, 1, list, Place: false, TeleportSwirl: false, UseMessage: false);
			}
			else
			{
				PsychicHunterSystem.CreateExtradimensionalSoloHunters(ParentObject.CurrentZone, 1, list, Place: false, TeleportSwirl: false, UseMessage: false);
			}
			GameObject gameObject = list.Last();
			GameObject randomElement = clams.GetRandomElement();
			if (randomElement != null && GiantClamProperties.Teleport(gameObject, randomElement.CurrentCell, 'O'))
			{
				IComponent<GameObject>.XDidYToZ(gameObject, "emerge", "from", randomElement, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: true);
				continue;
			}
			gameObject.SetLongProperty("ClamTeleportTurn", The.Game.Turns);
			PsychicHunterSystem.PlaceHunter(ParentObject.CurrentZone, gameObject, null, TeleportSwirl: true, "&O");
		}
		if (list.Count > 0)
		{
			DidX("open", "wide, and the nacre inside " + ParentObject.its + " shell glimmers with unseen light", null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, UsePopup: true);
			if (ParentObject.CurrentZone.IsActive())
			{
				PsychicHunterSystem.PsychicPresenceMessage(list.Count);
			}
		}
	}

	public void Activate()
	{
		Summon(Stat.Roll(NumberHuntersStart));
		Active = true;
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		if (!Active && E.Object == ParentObject && E.Actor != null && E.Actor.IsPlayerControlled())
		{
			Activate();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void RegisterClamDeath()
	{
		foreach (GameObject clam in GetClams())
		{
			clam.RegisterPartEvent(this, "BeforeDeathRemoval");
		}
		Registered = true;
	}

	public override bool FireEvent(Event E)
	{
		if (!Active && E.ID == "BeforeDeathRemoval" && ActiveOnDeathChance.in100())
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Killer");
			if (gameObjectParameter != null)
			{
				ParentObject.AddOpinion<OpinionKilledAlly>(gameObjectParameter, ParentObject);
			}
			Activate();
		}
		return base.FireEvent(E);
	}
}
