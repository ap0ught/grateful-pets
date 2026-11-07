using System.Collections.Generic;
using System.Linq;
using Qud.API;
using XRL.Annals;
using XRL.UI;

namespace XRL.World.Conversations.Parts;

/// <summary>Share a secret from Resheph's life to gain some XP.</summary>
public class GiveReshephSecret : IConversationPart
{
	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != GetChoiceTagEvent.ID && ID != EnterElementEvent.ID)
		{
			return ID == HideElementEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = "{{g|[Share secrets from Resheph's life]}}";
		return false;
	}

	public static bool IsShared(JournalSultanNote N)
	{
		return The.Game.StringGameState.ContainsKey("soldcultist_" + N.EventID);
	}

	public override bool HandleEvent(EnterElementEvent E)
	{
		List<JournalSultanNote> knownNotesForResheph = JournalAPI.GetKnownNotesForResheph(IsShared);
		List<JournalSultanNote> knownNotesForResheph2 = JournalAPI.GetKnownNotesForResheph((JournalSultanNote x) => !IsShared(x));
		if (knownNotesForResheph2.Count == 0)
		{
			Popup.Show("You do not have any unshared secrets about the life of Resheph.");
			return false;
		}
		string[] options = knownNotesForResheph2.Select((JournalSultanNote g) => g.Text).ToArray();
		List<(int, int)> list = Popup.PickSeveral("Choose secrets about the life of Resheph to share.", null, "", "Sounds/UI/ui_notification", options, null, null, null, The.Speaker, null, null, -1, 1, 60, 0, -1, RespectOptionNewlines: false, AllowEscape: true);
		if (list.IsNullOrEmpty())
		{
			return false;
		}
		int num = 0;
		int num2 = knownNotesForResheph.Count + 1;
		foreach (var item in list)
		{
			num += QudHistoryHelpers.GetReshephGospelXP(num2++);
			The.Game.SetStringGameState("soldcultist_" + knownNotesForResheph2[item.Item1].EventID, "1");
		}
		Popup.Show("You muse over the secret" + ((list.Count > 1) ? "s" : "") + " with " + The.Speaker.ShortDisplayNameWithoutTitlesStripped + " and gain some insight.");
		Popup.Show("You gain {{C|" + num + "}} XP.", null, "Sounds/UI/ui_notification", CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: false);
		The.Player.AwardXP(num, -1, 0, int.MaxValue, null, The.Speaker);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(HideElementEvent E)
	{
		return false;
	}
}
