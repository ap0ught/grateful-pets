using XRL.UI;

namespace XRL.World.Conversations.Parts;

public class Trade : IConversationPart
{
	public static GameObject Trader = null;

	public static Choice Choice = null;

	public static bool Enabled = true;

	public static bool Visible = true;

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != GetChoiceTagEvent.ID && ID != EnterElementEvent.ID)
		{
			return ID == IsElementVisibleEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = "{{g|[begin trade]}}";
		return false;
	}

	public override bool HandleEvent(EnterElementEvent E)
	{
		ShowScreen();
		return false;
	}

	public override void Initialize()
	{
		base.Initialize();
		if (Choice == null)
		{
			Choice = ParentElement as Choice;
		}
	}

	public override bool HandleEvent(IsElementVisibleEvent E)
	{
		return Visible;
	}

	public static bool CheckVisible()
	{
		return Visible = Enabled && ConversationUI.CurrentNode != null && ConversationUI.CurrentNode.AllowEscape && Choice != null && Choice.CheckPredicates();
	}

	public static bool CheckEnabled(bool Base = true, bool Physical = true, bool Mental = false)
	{
		Trader = The.Speaker;
		Enabled = true;
		if (Trader == null)
		{
			Enabled = false;
		}
		else if (!Base)
		{
			Enabled = false;
		}
		else
		{
			Enabled = CanTradeWith(Trader);
			GameObject transmitter = Conversation.Transmitter;
			if (!Enabled && CanTradeWith(transmitter))
			{
				Trader = transmitter;
				Enabled = true;
			}
			Enabled = CanTradeEvent.Check(The.Player, The.Speaker, Conversation.Transmitter, Conversation.Receiver, ref Trader, Conversation.Current, Enabled, Physical, Mental);
		}
		CheckVisible();
		return Enabled;
	}

	public static bool CanTradeWith(GameObject Object)
	{
		if (Object != null && The.Player.PhaseMatches(Object) && !Object.HasTagOrProperty("NoTrade"))
		{
			return Object.InSameOrAdjacentCellTo(The.Player);
		}
		return false;
	}

	public static void Reset()
	{
		Trader = null;
		Choice = null;
		Enabled = true;
	}

	public static void ShowScreen()
	{
		TradeUI.ShowTradeScreen(Trader);
	}
}
