using XRL.UI;
using XRL.World.Parts;
using XRL.World.Quests.GolemQuest;

namespace XRL.World.Conversations.Parts;

public class BuildGolem : IConversationPart
{
	public string Message;

	public int TimeDays;

	private GolemQuestSystem System;

	public override void Awake()
	{
		System = The.Game.GetSystem<GolemQuestSystem>();
		if (System != null)
		{
			System.Mound = The.Speaker.CurrentZone.GetFirstObjectWithPart("GolemQuestMound")?.GetPart<GolemQuestMound>();
		}
	}

	public override void Dispose()
	{
		if (System != null)
		{
			System.Mound = null;
			System = null;
		}
	}

	public override void LoadText(string Text)
	{
		Message = Text;
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != EnterElementEvent.ID && ID != EnteredElementEvent.ID)
		{
			return ID == GetChoiceTagEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnterElementEvent E)
	{
		if (System == null || !System.AllValid())
		{
			if (!Message.IsNullOrEmpty())
			{
				Popup.Show(Message);
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		if (System != null)
		{
			System.UpdateQuest();
			System.Mound?.Build(The.Player, TimeDays * 1200);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = "{{W|[Build Golem]}}";
		return base.HandleEvent(E);
	}
}
