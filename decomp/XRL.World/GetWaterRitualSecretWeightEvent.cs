using Qud.API;
using XRL.World.Conversations;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetWaterRitualSecretWeightEvent : PooledEvent<GetWaterRitualSecretWeightEvent>
{
	public GameObject Actor;

	public GameObject Speaker;

	public IBaseJournalEntry Secret;

	public int BaseWeight;

	public int Weight;

	public IConversationPart Source;

	public string Context;

	public bool Buy;

	public bool Sell;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Speaker = null;
		Secret = null;
		BaseWeight = 0;
		Weight = 0;
		Source = null;
		Context = null;
		Buy = false;
		Sell = false;
	}
}
