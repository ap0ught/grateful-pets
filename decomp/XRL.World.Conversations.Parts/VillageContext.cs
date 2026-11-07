using HistoryKit;
using Qud.API;

namespace XRL.World.Conversations.Parts;

public class VillageContext : IConversationPart
{
	public string GameState;

	private HistoricEntitySnapshot _History;

	public HistoricEntitySnapshot History
	{
		get
		{
			if (_History != null)
			{
				return _History;
			}
			if (!GameState.IsNullOrEmpty() && The.Game.StringGameState.TryGetValue(GameState, out var value))
			{
				_History = HistoryAPI.GetVillageSnapshot(value);
				if (_History != null)
				{
					return _History;
				}
			}
			GameObject speaker = The.Speaker;
			value = speaker?.GetPropertyOrTag("Mayor");
			if (!value.IsNullOrEmpty())
			{
				_History = HistoryAPI.GetVillageSnapshot(value);
				if (_History != null)
				{
					return _History;
				}
			}
			value = speaker.GetPrimaryFaction();
			if (!value.IsNullOrEmpty())
			{
				_History = HistoryAPI.GetVillageSnapshot(value);
				if (_History != null)
				{
					return _History;
				}
			}
			return null;
		}
	}

	public VillageContext()
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
		if (E.Text.Contains("=village"))
		{
			HistoryAPI.ExpandVillageText(E.Text, null, History);
		}
		return base.HandleEvent(E);
	}
}
