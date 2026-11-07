using System.Collections.Generic;
using System.Linq;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetCompanionStatusEvent : PooledEvent<GetCompanionStatusEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

	public GameObject ForLeader;

	public Dictionary<string, int> Status = new Dictionary<string, int>();

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
		Object = null;
		ForLeader = null;
		Status.Clear();
	}

	public void AddStatus(string Description, int Priority = 0)
	{
		Status[Description] = Priority;
	}

	public static string GetFor(GameObject Object, GameObject ForLeader)
	{
		if (GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetCompanionStatusEvent>.ID, CascadeLevel))
		{
			GetCompanionStatusEvent E = PooledEvent<GetCompanionStatusEvent>.FromPool();
			E.Object = Object;
			E.ForLeader = ForLeader;
			E.Status.Clear();
			Object.HandleEvent(E);
			if (E.Status.Count == 1)
			{
				using Dictionary<string, int>.KeyCollection.Enumerator enumerator = E.Status.Keys.GetEnumerator();
				if (enumerator.MoveNext())
				{
					return enumerator.Current;
				}
			}
			else if (E.Status.Count > 0)
			{
				return string.Join(", ", (from d in E.Status.Keys
					orderby E.Status[d], d
					select d).ToArray());
			}
		}
		return null;
	}
}
