using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class SixDayZealot : IPart
{
	public int OriginalLastTalk;

	public int LastTalk;

	public int QuietTimer = 55;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CanSmartUse");
		Registrar.Register("CommandSmartUseEarly");
		Registrar.Register("BeginTakeAction");
		base.Register(Object, Registrar);
	}

	public void ZealotDeclaim(GameObject who, bool Dialog)
	{
		if (!Dialog && !ParentObject.IsAudible(IComponent<GameObject>.ThePlayer))
		{
			return;
		}
		if (ParentObject.IsFrozen())
		{
			EmitMessage("The zealot mumbles inaudibly, encased in ice.", ' ', Dialog);
			return;
		}
		string text = null;
		switch (Stat.Random(1, 4))
		{
		case 1:
			text = "Make an offering at the Argent Well! Pay homage to your Fathers!";
			break;
		case 2:
			text = "Cast down your artifacts! You are not worthy of their make!";
			break;
		case 3:
			text = "Piety compels you to deliver your sacred relics to the priests in the cathedral! Cleanse them of your filth!";
			break;
		case 4:
			text = "The Machine commands that you exorcise robots and bring their sacred husks here!";
			break;
		}
		IComponent<GameObject>.EmitMessage(who ?? ParentObject, "The zealot yells {{W|'" + text + "'}}", ' ', Dialog);
		if (!Dialog)
		{
			ParentObject.ParticleText("{{W|" + text + "}}", IgnoreVisibility: true);
		}
	}

	public void ZealotDeclaim(bool Dialog)
	{
		ZealotDeclaim(null, Dialog);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanSmartUse")
		{
			if (ConversationScript.IsPhysicalConversationPossible(ParentObject))
			{
				if (E.GetGameObjectParameter("User").IsPlayer())
				{
					if (!ParentObject.IsPlayerLed())
					{
						return false;
					}
				}
				else if (ParentObject.InActiveZone())
				{
					return false;
				}
			}
		}
		else if (E.ID == "CommandSmartUseEarly")
		{
			if (ConversationScript.IsPhysicalConversationPossible(ParentObject))
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("User");
				if (gameObjectParameter.IsPlayer())
				{
					if (!ParentObject.IsPlayerLed())
					{
						ZealotDeclaim(gameObjectParameter, Dialog: true);
					}
				}
				else if (ParentObject.InActiveZone())
				{
					ZealotDeclaim(Dialog: false);
				}
			}
		}
		else if (E.ID == "BeginTakeAction" && ParentObject.InActiveZone() && IComponent<GameObject>.Visible(ParentObject))
		{
			LastTalk--;
			if (LastTalk < 0 && Stat.Random(0, QuietTimer) == 0)
			{
				LastTalk = OriginalLastTalk;
				ZealotDeclaim(Dialog: false);
			}
		}
		return base.FireEvent(E);
	}
}
