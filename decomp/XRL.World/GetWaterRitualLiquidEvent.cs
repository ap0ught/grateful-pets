namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetWaterRitualLiquidEvent : PooledEvent<GetWaterRitualLiquidEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject Target;

	public string Liquid;

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
		Actor = null;
		Target = null;
		Liquid = null;
	}

	public static string GetFor(GameObject Actor, GameObject Target)
	{
		string text = Target.GetPropertyOrTag("WaterRitualLiquid", "water");
		bool flag = true;
		if (flag && Target.HasRegisteredEvent("GetWaterRitualLiquid"))
		{
			Event obj = Event.New("GetWaterRitualLiquid");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Target", Target);
			obj.SetParameter("Liquid", text);
			flag = Target.FireEvent(obj);
			text = obj.GetStringParameter("Liquid");
		}
		if (flag && Target.WantEvent(PooledEvent<GetWaterRitualLiquidEvent>.ID, CascadeLevel))
		{
			GetWaterRitualLiquidEvent getWaterRitualLiquidEvent = PooledEvent<GetWaterRitualLiquidEvent>.FromPool();
			getWaterRitualLiquidEvent.Actor = Actor;
			getWaterRitualLiquidEvent.Target = Target;
			getWaterRitualLiquidEvent.Liquid = text;
			flag = Target.HandleEvent(getWaterRitualLiquidEvent);
			text = getWaterRitualLiquidEvent.Liquid;
		}
		int num = text.IndexOf('-');
		if (num != -1)
		{
			string text2 = text.Substring(0, num);
			if (text2 == "water")
			{
				int num2 = text.IndexOf(',');
				if (num2 != -1)
				{
					text2 = text.Substring(num2 + 1);
					num = text2.IndexOf('-');
					if (num != -1)
					{
						text2 = text2.Substring(0, num);
					}
				}
			}
			text = text2;
		}
		return text;
	}
}
