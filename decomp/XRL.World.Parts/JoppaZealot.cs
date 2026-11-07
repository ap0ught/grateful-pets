using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class JoppaZealot : IPart
{
	public int LastTalk;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEarlyEvent.ID && ID != SingletonEvent<CommandTakeActionEvent>.ID)
		{
			return ID == PooledEvent<GetPointsOfInterestEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetPointsOfInterestEvent E)
	{
		if (E.Actor.IsPlayer() && !The.Game.HasQuest("O Glorious Shekhinah!") && E.StandardChecks(this, E.Actor))
		{
			E.Add(ParentObject, ParentObject.GetReferenceDisplayName());
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (ConversationScript.IsPhysicalConversationPossible(ParentObject))
		{
			if (E.Actor.IsPlayer())
			{
				if (The.Game.HasQuest("O Glorious Shekhinah!") && !ParentObject.IsPlayerLed())
				{
					return false;
				}
			}
			else if (ParentObject.InActiveZone())
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEarlyEvent E)
	{
		if (ConversationScript.IsPhysicalConversationPossible(ParentObject))
		{
			if (E.Actor.IsPlayer())
			{
				if (The.Game.HasQuest("O Glorious Shekhinah!") && ParentObject.IsPlayerLed())
				{
				}
			}
			else
			{
				ParentObject.InActiveZone();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandTakeActionEvent E)
	{
		if (ParentObject.InActiveZone() && !ParentObject.IsPlayerLed())
		{
			LastTalk--;
			if (LastTalk < 0 && 55.in100())
			{
				LastTalk = 350;
				ZealotDeclaim(Dialog: false);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void ZealotDeclaim(GameObject who, bool Dialog)
	{
		if ((!Dialog && !ParentObject.IsAudible(IComponent<GameObject>.ThePlayer, 80)) || ParentObject.IsFrozen())
		{
			return;
		}
		string text = null;
		switch (Stat.Random(1, 4))
		{
		case 1:
			text = "Who ventures into the Great Salt Desert, and nearer the Six Day Stilt?";
			break;
		case 2:
			text = "Hmm, what of your artifacts? Make an offering of them to Shekhinah at the Sacred Well.";
			break;
		case 3:
			text = "The beauty! My stomach is in stirs.";
			break;
		case 4:
			text = "Is it a dybbuk that possesses the robot? It should be sacred and still.";
			break;
		}
		if (!string.IsNullOrEmpty(text))
		{
			if (who == null)
			{
				who = ParentObject;
			}
			IComponent<GameObject>.EmitMessage(who, who.The + who.ShortDisplayName + who.GetVerb("yell") + ", {{W|'" + text + "'}}", ' ', Dialog);
			if (!Dialog)
			{
				ParentObject.ParticleText("{{W|" + text + "}}", (ParentObject.CurrentCell.X < 40) ? 0.4f : (-0.4f), (ParentObject.CurrentCell.Y < 12) ? 0.2f : (-0.2f), ' ', IgnoreVisibility: true);
			}
		}
	}

	public void ZealotDeclaim(bool Dialog)
	{
		ZealotDeclaim(null, Dialog);
	}
}
