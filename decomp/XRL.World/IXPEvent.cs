namespace XRL.World;

[GameEvent(Base = true)]
public abstract class IXPEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Kill;

	public GameObject InfluencedBy;

	public GameObject PassedUpFrom;

	public GameObject PassedDownFrom;

	public string ZoneID;

	public int Amount;

	public int AmountBefore;

	public int Tier;

	public int Minimum;

	public int Maximum;

	public string Deed;

	public bool TierScaling = true;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Kill = null;
		InfluencedBy = null;
		PassedUpFrom = null;
		PassedDownFrom = null;
		ZoneID = null;
		Amount = 0;
		AmountBefore = 0;
		Tier = 0;
		Minimum = 0;
		Maximum = 0;
		Deed = null;
		TierScaling = true;
	}

	public void ApplyTo(IXPEvent E)
	{
		E.Actor = Actor;
		E.Kill = Kill;
		E.InfluencedBy = InfluencedBy;
		E.PassedUpFrom = PassedUpFrom;
		E.PassedDownFrom = PassedDownFrom;
		E.ZoneID = ZoneID;
		E.Amount = Amount;
		E.AmountBefore = AmountBefore;
		E.Tier = Tier;
		E.Minimum = Minimum;
		E.Maximum = Maximum;
		E.Deed = Deed;
		E.TierScaling = TierScaling;
	}
}
