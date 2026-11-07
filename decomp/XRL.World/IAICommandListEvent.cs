using System.Collections.Generic;
using XRL.World.AI.GoalHandlers;

namespace XRL.World;

[GameEvent(Base = true, Cascade = 17)]
public abstract class IAICommandListEvent : MinEvent
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject Target;

	public int Distance;

	public List<AICommandList> List = new List<AICommandList>();

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
		Distance = 0;
		List.Clear();
	}

	public void Add(string Command, int Priority = 1, GameObject Object = null, bool Inv = false, bool Self = false, GameObject TargetOverride = null, Cell TargetCellOverride = null)
	{
		AICommandList item = new AICommandList(Command, Priority, Object, Inv, Self, TargetOverride, TargetCellOverride);
		if (Priority > List.Capacity)
		{
			List.Capacity += Priority;
		}
		for (int i = 0; i < Priority; i++)
		{
			List.Add(item);
		}
	}
}
