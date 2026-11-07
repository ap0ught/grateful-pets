using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using Qud.API;
using XRL.UI;
using XRL.Wish;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[HasWishCommand]
[HasModSensitiveStaticCache]
public class WishMenu : IPlayerPart
{
	public class WishCommandXML
	{
		public string DisplayName = "default";

		public List<string> Commands;

		public string Author;

		public string ModName;

		public IRenderable Renderable;

		public string DisplayText
		{
			get
			{
				string text = DisplayName + "\n";
				if (!string.IsNullOrEmpty(Author) && !string.IsNullOrEmpty(ModName))
				{
					text = text + Markup.Color("c", "- From \"" + ModName + "\" by " + Author) + "\n";
				}
				foreach (string command in Commands)
				{
					text = text + Markup.Color("K", "- wish: " + command) + "\n";
				}
				return text;
			}
		}
	}

	public static string COMMAND_NAME = "CmdWishMenu";

	[ModSensitiveStaticCache(false)]
	public static List<WishCommandXML> MenuItems = new List<WishCommandXML>();

	public static Dictionary<string, Action<XmlDataHelper>> nodes = new Dictionary<string, Action<XmlDataHelper>>
	{
		{ "wishcommands", HandleNodes },
		{ "wish", HandleWishNode }
	};

	public static void CacheInit()
	{
		if (MenuItems == null)
		{
			MenuItems = new List<WishCommandXML>();
		}
		if (MenuItems.Count != 0)
		{
			return;
		}
		foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("wishcommands"))
		{
			item.HandleNodes(nodes);
		}
	}

	public static void HandleNodes(XmlDataHelper xml)
	{
		xml.HandleNodes(nodes);
	}

	/// <summary>
	///  A really quiet attempt at creating a blueprint hopefully.  The normal createSample throws loud errors
	/// </summary>
	private static bool TryCreateSample(string blueprint, out GameObject result)
	{
		result = null;
		try
		{
			result = GameObjectFactory.Factory.CreateObject(blueprint, -9999, 0, null, null, null, "Sample");
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static void HandleWishNode(XmlDataHelper xml)
	{
		WishCommandXML wishCommandXML = new WishCommandXML();
		wishCommandXML.Renderable = Renderable.UITile("empty", 'k', 'k');
		wishCommandXML.Commands = xml.ParseAttribute("Commands", wishCommandXML.Commands, required: true);
		string text = wishCommandXML.Commands[0];
		try
		{
			if (text.StartsWith("goto:"))
			{
				string zoneID = text.Substring(5);
				Zone zone = The.ZoneManager.GetZone(zoneID);
				GameObject firstObjectWithPart = The.ZoneManager.GetZone(zone.GetZoneWorld()).GetCell(zone.wX, zone.wY).GetFirstObjectWithPart("TerrainTravel");
				wishCommandXML.DisplayName = "Teleport to " + firstObjectWithPart.the + firstObjectWithPart.DisplayName;
				wishCommandXML.Renderable = new Renderable(firstObjectWithPart.RenderForUI());
			}
			else
			{
				if (text.StartsWith("item:"))
				{
					text = text.Substring(5);
				}
				if (GameObjectFactory.Factory.Blueprints.ContainsKey(text))
				{
					TryCreateSample(text, out var result);
					wishCommandXML.DisplayName = "Spawn " + result.an(int.MaxValue, null, null, AsIfKnown: true);
					wishCommandXML.Renderable = new Renderable(result.RenderForUI());
					result.Destroy();
				}
			}
		}
		catch (Exception)
		{
		}
		wishCommandXML.DisplayName = xml.ParseAttribute("DisplayName", wishCommandXML.DisplayName);
		wishCommandXML.Author = xml.ParseAttribute("Author", wishCommandXML.Author);
		wishCommandXML.ModName = xml.ParseAttribute("ModName", wishCommandXML.ModName);
		string text2 = xml.ParseAttribute<string>("Icon", null);
		if (!string.IsNullOrEmpty(text2))
		{
			string text3 = xml.ParseAttribute("ColorPair", "yw");
			wishCommandXML.Renderable = Renderable.UITile(text2, text3[0], text3[1]);
		}
		MenuItems.Add(wishCommandXML);
		xml.DoneWithElement();
	}

	[WishCommand("menu", null)]
	public static void OpenWishMenu()
	{
		CacheInit();
		int num = Popup.PickOption("Wish Menu", null, "", "Sounds/UI/ui_notification", MenuItems.Select((WishCommandXML a) => a.DisplayText).ToArray(), null, MenuItems.Select((WishCommandXML a) => a.Renderable).ToArray(), null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
		if (num < 0)
		{
			return;
		}
		foreach (string command in MenuItems[num].Commands)
		{
			Wishing.HandleWish(The.Player, command);
		}
	}

	[WishCommand("revealandgoto", null)]
	public static void RevealAndGoto(string rest)
	{
		JournalMapNote mapNote = JournalAPI.GetMapNote(rest);
		if (mapNote != null)
		{
			if (!mapNote.Revealed)
			{
				JournalAPI.RevealMapNote(mapNote);
			}
			Wishing.HandleWish(The.Player, "goto:" + mapNote.ZoneID);
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<CommandEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			OpenWishMenu();
		}
		return base.HandleEvent(E);
	}
}
