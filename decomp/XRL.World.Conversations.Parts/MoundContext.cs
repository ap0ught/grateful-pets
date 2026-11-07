using XRL.Language;
using XRL.World.Parts;

namespace XRL.World.Conversations.Parts;

public class MoundContext : IConversationPart
{
	public bool Preposition = true;

	public MoundContext()
	{
		Priority = -1000;
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation))
		{
			return ID == PrepareTextEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(PrepareTextEvent E)
	{
		if (E.Text.Contains("=mound"))
		{
			string newValue = "soon";
			GolemQuestMound golemQuestMound = The.Speaker.CurrentZone?.GetFirstObjectWithPart("GolemQuestMound")?.GetPart<GolemQuestMound>();
			if (golemQuestMound != null)
			{
				int completeDays = golemQuestMound.CompleteDays;
				if (completeDays >= 1)
				{
					newValue = (Preposition ? "in " : "") + Grammar.Cardinal(completeDays) + ((completeDays == 1) ? " day" : " days");
				}
			}
			E.Text.Replace("=mound.complete.days=", newValue);
		}
		return base.HandleEvent(E);
	}
}
