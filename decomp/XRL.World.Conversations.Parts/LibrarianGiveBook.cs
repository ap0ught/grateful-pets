using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using Qud.API;
using XRL.Language;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Conversations.Parts;

public class LibrarianGiveBook : IConversationPart
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
		E.Tag = "{{g|[Give Books]}}";
		return false;
	}

	public static bool IsAwardableBook(GameObject Object)
	{
		if (Object.HasIntProperty("LibrarianAwarded"))
		{
			return false;
		}
		if (Object.HasTagOrIntProperty("LibrarianAwardable"))
		{
			return true;
		}
		if (Object.HasPart<Book>())
		{
			return true;
		}
		if (Object.HasPart<VillageHistoryBook>())
		{
			return true;
		}
		if (Object.HasPart<MarkovBook>())
		{
			return true;
		}
		if (Object.HasPart<Cookbook>())
		{
			return true;
		}
		return false;
	}

	public override bool HandleEvent(EnterElementEvent E)
	{
		Inventory inventory = The.Player.Inventory;
		List<string> list = new List<string>();
		List<GameObject> list2 = new List<GameObject>();
		List<int> list3 = new List<int>();
		List<int> list4 = new List<int>();
		List<IRenderable> list5 = new List<IRenderable>();
		bool flag = false;
		foreach (GameObject @object in inventory.GetObjects(IsAwardableBook))
		{
			if (@object.IsMarkedImportantByPlayer())
			{
				flag = true;
				continue;
			}
			double valueEach = @object.ValueEach;
			int num = (int)(valueEach * valueEach / 25.0);
			if (num > 0)
			{
				list2.Add(@object);
				list3.Add(num);
				list4.Add(@object.Count);
				list5.Add(@object.Render);
				list.Add(@object.GetDisplayName(1120) + " [{{C|" + num + "}} XP]");
			}
		}
		if (list2.Count == 0)
		{
			return The.Player.ShowFailure(flag ? "You only have books you've marked important. Unmark any you wish to donate." : "You have no books to give.");
		}
		List<(int, int)> list6 = Popup.PickSeveral("Choose books to give.", null, "", "Sounds/UI/ui_notification", list, HotkeySpread.get(new string[2] { "Menus", "UINav" }), list4, list5, The.Speaker, null, null, -1, 0, 60, 0, -1, RespectOptionNewlines: false, AllowEscape: true);
		if (list6.IsNullOrEmpty())
		{
			return false;
		}
		int num2 = 0;
		List<GameObject> list7 = new List<GameObject>(list6.Count);
		string text = The.Speaker.Does("provide", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: true, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: true);
		string text2 = The.Speaker.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: true, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: true);
		foreach (var item2 in list6)
		{
			GameObject gameObject = list2[item2.Item1];
			int item = item2.Item2;
			if (gameObject.ConfirmUseImportant(The.Player, "donate", null, item))
			{
				gameObject.SplitStack(item, The.Player);
				GameObject speaker = The.Speaker;
				if (speaker != null && speaker.ReceiveObject(gameObject))
				{
					num2 += list3[item2.Item1] * item;
					list7.Add(gameObject);
					JournalAPI.AddAccomplishment(text2 + " provided you with insightful commentary on " + gameObject.ShortDisplayNameSingleStripped + ".", "Remember the kindness of =name=, who patiently taught " + gameObject.ShortDisplayNameSingleStripped + " to " + The.Player.GetPronounProvider().PossessiveAdjective + " simple pupil, " + text2 + ".", "At a remote library near the Six Day Stilt, =name= met with the " + The.Speaker.GetCreatureType() + " librarian  Sheba Hagadias and together they wrote an exegesis on the book " + gameObject.ShortDisplayNameSingleStripped + ".", "DonateBook", "general", MuralCategory.LearnsSecret, MuralWeight.Low, null, -1L);
					gameObject.SetIntProperty("LibrarianAwarded", 1);
				}
			}
		}
		SoundManager.PlayUISound("Sounds/Interact/sfx_interact_book_turnIn");
		string text3 = Grammar.MakeAndList(list7.Select((GameObject x) => "'" + x.ShortDisplayNameSingle + "'").ToList());
		Popup.Show(text + " some insightful commentary on " + text3 + ".");
		Popup.Show("You gain {{C|" + num2 + "}} XP.", null, "Sounds/UI/ui_notification", CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: false);
		The.Player.AwardXP(num2, -1, 0, int.MaxValue, null, The.Speaker);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(HideElementEvent E)
	{
		return false;
	}
}
