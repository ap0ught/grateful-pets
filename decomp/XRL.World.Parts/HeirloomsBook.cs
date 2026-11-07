using System.Collections.Generic;
using System.Text;

namespace XRL.World.Parts;

public class HeirloomsBook : IBookContents
{
	public override string GetTitle()
	{
		return "Heirlooms of Qud";
	}

	public override List<BookPageInfo> GetContents()
	{
		List<BookPageInfo> list = new List<BookPageInfo>();
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		int num2 = 22;
		stringBuilder.Length = 0;
		BookPageInfo bookPageInfo = new BookPageInfo();
		stringBuilder.AppendLine("Attachment to physical objects is a peculiar phenomenon. Often the bonds we form transcend fiscal values. They are steeped in history and rolled in tradition, and they elude rational explanation.");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("It's unsurprising, then, that my research findings indicate certain preferences are held by the myriad factions of Qud for certain types of items. These preferences, each surely sculpted by some historical event lost to memory, persist in the veneration of certain cherished heirlooms.");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("I've assembled a catalogue of these preferences for posterity.");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("[{{c|compiled from the notes of Sheba Hegadias}}]");
		bookPageInfo.Format = "Auto";
		bookPageInfo.Text = stringBuilder.ToString();
		bookPageInfo.Title = GetTitle();
		list.Add(bookPageInfo);
		stringBuilder.Length = 0;
		stringBuilder.AppendLine();
		bookPageInfo = new BookPageInfo();
		List<Faction> list2 = new List<Faction>();
		foreach (Faction item in Factions.Loop())
		{
			if (item.Visible)
			{
				list2.Add(item);
			}
		}
		list2.Sort((Faction a, Faction b) => a.DisplayName.CompareTo(b.DisplayName));
		foreach (Faction item2 in list2)
		{
			AppendCatalogLine("          " + item2.DisplayName, Items.ItemCategoryDisplayNames[item2.Heirloom] + "          ", ".", 75, stringBuilder);
			stringBuilder.AppendLine();
			num++;
			if (num >= num2 - 1)
			{
				stringBuilder.AppendLine();
				bookPageInfo.Text = stringBuilder.ToString();
				bookPageInfo.Title = GetTitle();
				list.Add(bookPageInfo);
				bookPageInfo = new BookPageInfo();
				stringBuilder.Length = 0;
				stringBuilder.AppendLine();
				num = 0;
			}
		}
		stringBuilder.AppendLine();
		bookPageInfo.Text = stringBuilder.ToString();
		bookPageInfo.Title = GetTitle();
		list.Add(bookPageInfo);
		return list;
	}
}
