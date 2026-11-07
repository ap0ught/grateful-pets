namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class ReputationChangeEvent : PooledEvent<ReputationChangeEvent>
{
	public new static readonly int CascadeLevel = 17;

	public Faction Faction;

	public int BaseAmount;

	public int Amount;

	public string Type;

	public bool Silent;

	public bool Transient;

	public bool Prospective;

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
		Faction = null;
		BaseAmount = 0;
		Amount = 0;
		Type = null;
		Silent = false;
		Transient = false;
		Prospective = false;
	}

	public static int GetFor(Faction Faction, int BaseAmount, string Type = null, bool Silent = false, bool Transient = false, bool Prospective = false)
	{
		int num = BaseAmount;
		bool flag = true;
		if (The.Player != null)
		{
			if (flag && The.Player.HasRegisteredEvent("ReputationChange"))
			{
				Event obj = Event.New("ReputationChange");
				obj.SetParameter("Faction", Faction);
				obj.SetParameter("BaseAmount", BaseAmount);
				obj.SetParameter("Amount", num);
				obj.SetParameter("Type", Type);
				obj.SetParameter("Silent", Silent);
				obj.SetParameter("Transient", Transient);
				obj.SetParameter("Prospective", Prospective);
				flag = The.Player.FireEvent(obj);
				num = obj.GetIntParameter("Amount");
			}
			if (flag)
			{
				bool flag2 = The.Game.WantEvent(PooledEvent<ReputationChangeEvent>.ID, CascadeLevel);
				bool flag3 = The.Player.WantEvent(PooledEvent<ReputationChangeEvent>.ID, CascadeLevel);
				if (flag2 | flag3)
				{
					ReputationChangeEvent reputationChangeEvent = PooledEvent<ReputationChangeEvent>.FromPool();
					reputationChangeEvent.Faction = Faction;
					reputationChangeEvent.BaseAmount = BaseAmount;
					reputationChangeEvent.Amount = num;
					reputationChangeEvent.Type = Type;
					reputationChangeEvent.Silent = Silent;
					reputationChangeEvent.Transient = Transient;
					reputationChangeEvent.Prospective = Prospective;
					if (flag2)
					{
						flag = The.Game.HandleEvent(reputationChangeEvent);
						num = reputationChangeEvent.Amount;
					}
					if (flag & flag3)
					{
						flag = The.Player.HandleEvent(reputationChangeEvent);
						num = reputationChangeEvent.Amount;
					}
				}
			}
		}
		return num;
	}
}
