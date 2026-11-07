namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class JoinPartyLeaderPossibleEvent : PooledEvent<JoinPartyLeaderPossibleEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Companion;

	public GameObject Leader;

	public Cell CurrentCell;

	/// <summary>Approximate target cell, usually the leader's current cell. Can be replaced.</summary>
	public Cell TargetCell;

	public bool IsMobile;

	public bool Result;

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Companion = null;
		Leader = null;
		CurrentCell = null;
		TargetCell = null;
		IsMobile = false;
		Result = false;
	}

	public static bool Check(GameObject Companion, GameObject Leader, Cell CurrentCell, ref Cell TargetCell, bool IsMobile)
	{
		if (!GameObject.Validate(ref Companion))
		{
			return false;
		}
		bool flag = IsMobile;
		if (Companion.HasRegisteredEvent("JoinPartyLeaderPossible"))
		{
			Event obj = Event.New("JoinPartyLeaderPossible");
			obj.SetParameter("Companion", Companion);
			obj.SetParameter("Leader", Leader);
			obj.SetParameter("CurrentCell", CurrentCell);
			obj.SetParameter("TargetCell", TargetCell);
			obj.SetFlag("IsMobile", IsMobile);
			obj.SetFlag("Result", flag);
			bool flag2 = Companion.FireEvent(obj);
			TargetCell = obj.GetParameter("TargetCell") as Cell;
			flag = obj.HasFlag("Result");
			if (!flag2)
			{
				return flag;
			}
		}
		if (Companion.WantEvent(PooledEvent<JoinPartyLeaderPossibleEvent>.ID, CascadeLevel))
		{
			JoinPartyLeaderPossibleEvent joinPartyLeaderPossibleEvent = PooledEvent<JoinPartyLeaderPossibleEvent>.FromPool();
			joinPartyLeaderPossibleEvent.Companion = Companion;
			joinPartyLeaderPossibleEvent.Leader = Leader;
			joinPartyLeaderPossibleEvent.CurrentCell = CurrentCell;
			joinPartyLeaderPossibleEvent.TargetCell = TargetCell;
			joinPartyLeaderPossibleEvent.IsMobile = IsMobile;
			joinPartyLeaderPossibleEvent.Result = flag;
			Companion.HandleEvent(joinPartyLeaderPossibleEvent);
			TargetCell = joinPartyLeaderPossibleEvent.TargetCell;
			flag = joinPartyLeaderPossibleEvent.Result;
		}
		return flag;
	}
}
