using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using Qud.UI;
using Wintellect.PowerCollections;
using XRL.World;
using XRL.World.Conversations;
using XRL.World.Conversations.Parts;
using XRL.World.Effects;

namespace XRL.UI;

[UIView("Conversation", true, false, false, "Conversation", null, false, 0, true)]
public class ConversationUI : IWantsTextConsoleInit
{
	public class RenderableLines
	{
		public List<string> Lines;

		public int Position;

		public int End => Position + Lines.Count - 1;

		public virtual int MaxWidth => Width;

		public RenderableLines(ref int Position, IConversationElement Element)
		{
			this.Position = Position;
			Lines = StringFormat.ClipTextToArray(Element.GetDisplayText(WithColor: true), MaxWidth, KeepNewlines: true);
			Position += Lines.Count + 1;
		}

		public virtual void Render(int Scroll)
		{
			int num = Position - Scroll;
			int num2 = 0;
			int count = Lines.Count;
			while (num2 < count)
			{
				if (num >= Top)
				{
					if (num > Bottom)
					{
						break;
					}
					SB.WriteAt(Left, num, Lines[num2]);
				}
				num2++;
				num++;
			}
		}

		public bool ScrollIntoView(ref int Scroll)
		{
			if (End > Bottom + Scroll)
			{
				Scroll = End - Bottom;
				return true;
			}
			if (Position < Top + Scroll)
			{
				Scroll = Position - Top;
				return true;
			}
			return false;
		}
	}

	public class RenderableSelection : RenderableLines
	{
		public int Index;

		public override int MaxWidth => base.MaxWidth - 5;

		public char DisplayChar => (char)((Index <= 8) ? (49 + Index) : (65 + (Index - 9)));

		public RenderableSelection(int Index, ref int Position, IConversationElement Element)
			: base(ref Position, Element)
		{
			this.Index = Index;
		}

		public override void Render(int Scroll)
		{
			int num = Position - Scroll;
			int num2 = -1;
			int y = -1;
			int num3 = 0;
			int count = Lines.Count;
			while (num3 < count)
			{
				if (num >= Top)
				{
					if (num > Bottom)
					{
						break;
					}
					if (num2 == -1)
					{
						num2 = num;
					}
					if (num3 == 0)
					{
						SB.WriteAt(Left, num, ((Index == SelectedChoice) ? "{{Y|>}} " : "  ") + "{{W|" + DisplayChar + "}}) ");
					}
					SB.WriteAt(Left + 5, y = num, Lines[num3]);
				}
				num3++;
				num++;
			}
			if (num2 != -1)
			{
				GameManager.Instance.AddRegion(Left, num2, Right, y, "Click:" + Index, "Close", "Hover:" + Index);
			}
		}
	}

	public static TextConsole TC;

	public static ScreenBuffer SB;

	public static int X1 = 0;

	public static int X2 = 79;

	public static int XP = 2;

	public static int Y1 = 0;

	public static int Y2 = 24;

	public static int YP = 2;

	public static GameObject Speaker;

	public static GameObject Listener;

	/// <summary>A communication device used by the speaker.</summary>
	public static GameObject Transmitter;

	/// <summary>A communication device used by the listener.</summary>
	public static GameObject Receiver;

	public static Conversation CurrentConversation;

	public static Node StartNode;

	public static Node CurrentNode;

	public static Choice LastChoice;

	public static List<Choice> CurrentChoices;

	public static IRenderable Icon;

	public static int SelectedChoice = 0;

	public static bool Physical;

	public static bool Mental;

	public static int Width => X2 - XP * 2 - X1;

	public static int Height => Y2 - YP * 2 - Y1;

	public static int Top => Y1 + YP;

	public static int Bottom => Y2 - YP;

	public static int Left => X1 + XP;

	public static int Right => X2 - XP;

	[Obsolete("Field renamed to CurrentConversation.")]
	public static Conversation CurrentDialogue => CurrentConversation;

	public void Init(TextConsole TextConsole, ScreenBuffer ScreenBuffer)
	{
		TC = TextConsole;
		SB = ScreenBuffer;
	}

	public static void RenderClassic()
	{
		int Position = Top;
		RenderableLines renderableLines = new RenderableLines(ref Position, CurrentNode);
		List<RenderableSelection> list = new List<RenderableSelection>(CurrentChoices.Count);
		for (int i = 0; i < CurrentChoices.Count; i++)
		{
			list.Add(new RenderableSelection(i, ref Position, CurrentChoices[i]));
		}
		int Scroll = 0;
		int end = list.Last().End;
		do
		{
			RenderBox();
			GameManager.Instance.ClearRegions();
			if (SelectedChoice >= CurrentChoices.Count)
			{
				SelectedChoice = 0;
				renderableLines.ScrollIntoView(ref Scroll);
			}
			else if (SelectedChoice < 0)
			{
				if (renderableLines.ScrollIntoView(ref Scroll))
				{
					SelectedChoice = 0;
				}
				else
				{
					SelectedChoice = CurrentChoices.Count - 1;
				}
			}
			list[SelectedChoice].ScrollIntoView(ref Scroll);
			renderableLines.Render(Scroll);
			foreach (RenderableSelection item in list)
			{
				item.Render(Scroll);
			}
			if (Scroll > 0)
			{
				SB.WriteAt(Left, Top - 1, "{{W|<more...>}}");
			}
			if (end - Scroll - Top > Height)
			{
				SB.WriteAt(Left, Bottom + 1, "{{W|<more...>}}");
			}
			if (Scroll > 0 || end - Top >= Height)
			{
				ScrollbarHelper.Paint(SB, Top, X2, Height, ScrollbarHelper.Orientation.Vertical, 0, end, Scroll, Scroll + Bottom);
			}
			TC.DrawBuffer(SB);
		}
		while (Input());
	}

	public static void RenderBox()
	{
		if (X1 > 0 || Y1 > 0 || X2 < 79 || Y2 < 24)
		{
			SB.RenderBase();
		}
		else
		{
			SB.Clear();
		}
		SB.SingleBox(X1, Y1, X2, Y2, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
		string Title = GetTitle();
		IRenderable Icon = ConversationUI.Icon;
		RenderNodeEvent.Send(CurrentNode, ref Title, ref Icon);
		if (!Title.IsNullOrEmpty())
		{
			SB.WriteAt(Left, Y1, "{{y|[ " + Title + " ]}}");
		}
		if (Trade.Visible)
		{
			SB.WriteAt(Width - 15, Y2, " {{keybind|" + ControlManager.getCommandInputDescription("CmdStartTrade") + "}} - Trade ");
		}
	}

	public static bool Input()
	{
		Keys keys = Keyboard.getvk(Options.MapDirectionsToKeypad);
		string text = CommandBindingManager.MapKeyToCommand(Keyboard.MetaKey);
		if (keys == Keys.Escape)
		{
			return !Escape();
		}
		if (keys == Keys.Space || keys == Keys.Enter)
		{
			return !Select();
		}
		if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event != null)
		{
			string text2 = Keyboard.CurrentMouseEvent.Event;
			if (text2 == "Close")
			{
				return !Escape();
			}
			if (text2.Contains("Click:") && int.TryParse(text2.Split(':')[1], out var result))
			{
				return !Select(result);
			}
			if (text2.Contains("Hover:") && int.TryParse(text2.Split(':')[1], out result))
			{
				SelectedChoice = result;
			}
			if (text2 == "Command:CmdStartTrade" && Trade.Visible)
			{
				Trade.ShowScreen();
			}
		}
		else
		{
			switch (keys)
			{
			case Keys.Next:
				SelectedChoice = CurrentChoices.Count - 1;
				break;
			case Keys.Prior:
				SelectedChoice = 0;
				break;
			case Keys.D1:
			case Keys.D2:
			case Keys.D3:
			case Keys.D4:
			case Keys.D5:
			case Keys.D6:
			case Keys.D7:
			case Keys.D8:
			case Keys.D9:
				return !Select((int)(keys - 49));
			default:
				if (keys >= Keys.A && keys <= Keys.Z)
				{
					return !Select((int)(9 + (keys - 65)));
				}
				if (text == "CmdMoveS")
				{
					SelectedChoice++;
				}
				else if (text == "CmdMoveN")
				{
					SelectedChoice--;
				}
				break;
			}
		}
		return true;
	}

	public static void HaveConversation(string ConversationID, GameObject Speaker = null, GameObject Listener = null, GameObject Transmitter = null, GameObject Receiver = null, bool TradeEnabled = true, bool Physical = false, bool Mental = false)
	{
		if (Conversation.Blueprints.TryGetValue(ConversationID, out var value))
		{
			HaveConversation(value, Speaker, Listener, Transmitter, Receiver, TradeEnabled, Physical, Mental);
		}
		else
		{
			MetricsManager.LogError("Unknown conversation '" + ConversationID + "'");
		}
	}

	public static void HaveConversation(ConversationXMLBlueprint Blueprint, GameObject Speaker = null, GameObject Listener = null, GameObject Transmitter = null, GameObject Receiver = null, bool TradeEnabled = true, bool Physical = false, bool Mental = false)
	{
		GameObject player = The.Player;
		if (Listener == null)
		{
			Listener = player;
		}
		ConversationUI.Speaker = Speaker;
		ConversationUI.Listener = Listener;
		ConversationUI.Transmitter = Transmitter;
		ConversationUI.Receiver = Receiver;
		ConversationUI.Physical = Physical;
		ConversationUI.Mental = Mental;
		HaveConversation(new Conversation(Blueprint), Speaker, Listener, Transmitter, Receiver, TradeEnabled, Physical, Mental);
	}

	public static void HaveConversation(Conversation CurrentConversation, GameObject Speaker = null, GameObject Listener = null, GameObject Transmitter = null, GameObject Receiver = null, bool TradeEnabled = true, bool Physical = false, bool Mental = false)
	{
		GameObject player = The.Player;
		if (Listener == null)
		{
			Listener = player;
		}
		ConversationUI.CurrentConversation = CurrentConversation;
		ConversationUI.Speaker = Speaker;
		ConversationUI.Listener = Listener;
		ConversationUI.Transmitter = Transmitter;
		ConversationUI.Receiver = Receiver;
		ConversationUI.Physical = Physical;
		ConversationUI.Mental = Mental;
		try
		{
			GameManager.Instance.PushGameView("Conversation", bHard: false);
			if (Listener != player)
			{
				The.Game.Player.SetBody(Listener, Transient: true);
			}
			Trade.CheckEnabled(TradeEnabled, Physical, Mental);
			InternalConversation();
		}
		finally
		{
			if (Listener != player && Listener.IsPlayer())
			{
				The.Game.Player.SetBody(player, Transient: true);
			}
			Reset();
			GameManager.Instance.PopGameView(bHard: true);
		}
	}

	private static void InternalConversation()
	{
		if (!CanHaveConversationEvent.Check(Listener, Speaker, Transmitter, Receiver, CurrentConversation, Trade.Enabled, Physical, Mental) || !BeforeConversationEvent.Check(Listener, Speaker, Transmitter, Receiver, CurrentConversation, Trade.Enabled, Physical, Mental))
		{
			return;
		}
		CurrentConversation.Awake();
		if (!CurrentConversation.Enter())
		{
			return;
		}
		Icon = (Transmitter ?? Speaker)?.RenderForUI("Conversation");
		CurrentChoices = new List<Choice>();
		StartNode = (CurrentNode = CurrentConversation.GetStart());
		if (StartNode == null || !StartNode.Enter() || !BeginConversationEvent.Check(Listener, Speaker, Transmitter, Receiver, CurrentConversation, StartNode, ref Icon, Trade.Visible, Physical, Mental) || Speaker == null || !Speaker.FireEvent("ObjectTalking"))
		{
			return;
		}
		CurrentConversation.Entered();
		CurrentConversation.Prepare();
		StartNode.Entered();
		CheckLost();
		while (CurrentNode != null)
		{
			Prepare();
			if (UIManager.UseNewPopups)
			{
				Render();
			}
			else
			{
				RenderClassic();
			}
		}
		CurrentConversation.Leave();
		CurrentConversation.Left();
		AfterConversationEvent.Send(Listener, Speaker, Transmitter, Receiver, CurrentConversation, Trade.Visible, Physical, Mental);
		CurrentConversation.Dispose();
	}

	public static void Reset()
	{
		Speaker = null;
		Listener = null;
		CurrentConversation = null;
		StartNode = null;
		CurrentNode = null;
		LastChoice = null;
		CurrentChoices = null;
		Icon = null;
		SelectedChoice = 0;
		Physical = false;
		Mental = false;
		Trade.Reset();
	}

	public static void CheckLost()
	{
		if (Listener.TryGetEffect<Lost>(out var Effect))
		{
			if (CanGiveDirectionsEvent.Check(Listener, Speaker, Transmitter, Receiver, CurrentConversation, Trade.Visible, Physical, Mental))
			{
				Effect.Duration = int.MinValue;
				Listener.RemoveEffect(Effect);
				Popup.Show("You ask about your location and are no longer lost.");
				GaveDirectionsEvent.Send(Listener, Speaker, Transmitter, Receiver, CurrentConversation, Trade.Visible, Physical, Mental);
			}
		}
		else if (Speaker.TryGetEffect<Lost>(out Effect) && CanGiveDirectionsEvent.Check(Speaker, Listener, Transmitter, Receiver, CurrentConversation, Trade.Visible, Physical, Mental))
		{
			Effect.Duration = int.MinValue;
			Speaker.RemoveEffect(Effect);
			Popup.Show(Speaker.Does("ask", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " about " + Speaker.its + " location and " + Speaker.GetVerb("are", PrependSpace: false) + " no longer lost.");
			Listener.AwardXP(100);
			GaveDirectionsEvent.Send(Speaker, Listener, Transmitter, Receiver, CurrentConversation, Trade.Visible, Physical, Mental);
		}
	}

	public static string GetTitle()
	{
		if (Options.DebugShowConversationNode)
		{
			if (Speaker != null)
			{
				return "{{W|" + Speaker.DisplayName + " - " + CurrentConversation.ID + " - " + CurrentNode.ID + "}}";
			}
			return "{{W|" + CurrentConversation.ID + " - " + CurrentNode.ID + "}}";
		}
		if (Speaker != null)
		{
			return "{{W|" + Speaker.DisplayName + "}}";
		}
		return "";
	}

	public static void Prepare()
	{
		Event.ResetPool();
		ConversationEvent.ResetPools();
		Trade.CheckVisible();
		CurrentNode.Prepare();
		for (int num = CurrentChoices.Count - 1; num >= 0; num--)
		{
			CurrentChoices[num].Reset();
			CurrentChoices.RemoveAt(num);
		}
		foreach (IConversationElement element in CurrentNode.Elements)
		{
			element.Awake();
			if (element is Choice choice && choice.IsVisible() && (LastChoice?.Hash != choice.Hash || !HideElementEvent.Check(element, "LastChoice")))
			{
				choice.Prepare();
				CurrentChoices.Add(choice);
			}
		}
		Algorithms.StableSortInPlace(CurrentChoices);
	}

	public static void Render()
	{
		GameObject gameObject = Transmitter ?? Speaker;
		List<string> options = CurrentChoices.Select((Choice x) => x.GetDisplayText(WithColor: true)).ToList();
		string Title = GetTitle();
		IRenderable Icon = ConversationUI.Icon;
		RenderNodeEvent.Send(CurrentNode, ref Title, ref Icon);
		int num = Popup.ShowConversation(Title, Icon, CurrentNode.GetDisplayText(WithColor: true), options, Trade.Visible, CurrentNode.AllowEscape, CurrentConversation?.HasState("RenderMapBehind") ?? false);
		switch (num)
		{
		case -1:
			Escape();
			break;
		case -2:
			Trade.ShowScreen();
			break;
		case -3:
			InventoryActionEvent.Check(gameObject, Listener, gameObject, "Look");
			break;
		default:
			Select(num);
			break;
		}
	}

	public static bool Select(int Choice = -1)
	{
		if (Choice >= 0)
		{
			if (Choice >= CurrentChoices.Count)
			{
				return false;
			}
			SelectedChoice = Choice;
		}
		Choice choice = CurrentChoices[SelectedChoice];
		if (!choice.Enter())
		{
			return false;
		}
		if (!CurrentNode.Leave())
		{
			return false;
		}
		Node targetNode = GetTargetNode(choice);
		targetNode?.Awake();
		if (targetNode != null && !targetNode.Enter())
		{
			return false;
		}
		LastChoice = CurrentChoices[SelectedChoice];
		choice.Entered();
		CurrentNode.Left();
		CurrentNode = targetNode;
		targetNode?.Entered();
		Speaker?.PlayWorldSoundTag("ChatNodeSound", "sfx_interact_chat_dialognode", Listener?.CurrentCell);
		return true;
	}

	public static bool Escape()
	{
		if (!CurrentNode.AllowEscape)
		{
			return false;
		}
		if (!CurrentNode.Leave())
		{
			return false;
		}
		LastChoice = null;
		CurrentNode.Left();
		CurrentNode = null;
		return true;
	}

	public static Node GetTargetNode(Choice Choice)
	{
		string Target = Choice.Target;
		GetTargetElementEvent.Send(Choice, ref Target);
		if (Target.IsNullOrEmpty())
		{
			return CurrentNode;
		}
		if (Target == "End")
		{
			return null;
		}
		if (Target == "Start")
		{
			return StartNode;
		}
		foreach (IConversationElement element in CurrentConversation.Elements)
		{
			if (element.ID == Target && element is Node node)
			{
				node.Awake();
				return node;
			}
		}
		MetricsManager.LogError("Invalid target: '" + Target + "', [" + CurrentConversation.ID + "." + CurrentNode.ID + "." + (Choice.ID ?? "Unknown") + "]");
		return null;
	}
}
