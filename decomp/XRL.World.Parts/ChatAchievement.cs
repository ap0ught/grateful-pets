using System;

namespace XRL.World.Parts;

[Serializable]
public class ChatAchievement : IPart
{
	public string AchievementID = "";

	public bool Triggered;

	public override bool SameAs(IPart p)
	{
		ChatAchievement chatAchievement = p as ChatAchievement;
		if (chatAchievement.AchievementID != AchievementID)
		{
			return false;
		}
		if (chatAchievement.Triggered != Triggered)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeConversationEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeConversationEvent E)
	{
		if (!Triggered && E.SpeakingWith == ParentObject && E.Actor.IsPlayer())
		{
			AchievementManager.SetAchievement(AchievementID);
			Triggered = true;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
