using System.Collections.Generic;

namespace XRL.World.Conversations;

/// <summary>Fired by the IfCommand predicate and controls visibility similarly to IsElementVisibleEvent.</summary>
[ConversationEvent(Action = Action.Get)]
public class PredicateEvent : IDelegateEvent
{
	public new static readonly int ID = ConversationEvent.RegisterEvent(typeof(PredicateEvent), null, CountPool, ResetPool);

	private static List<PredicateEvent> Pool;

	private static int PoolCounter;

	[Parameter(Get = true)]
	public bool Result;

	public PredicateEvent()
		: base(ID)
	{
	}

	public static int CountPool()
	{
		if (Pool != null)
		{
			return Pool.Count;
		}
		return 0;
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static void ResetTo(ref PredicateEvent E)
	{
		ConversationEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static PredicateEvent FromPool()
	{
		return ConversationEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static PredicateEvent FromPool(IConversationElement Element, string Delegate, string Command, bool Result = false)
	{
		PredicateEvent predicateEvent = FromPool();
		predicateEvent.Element = Element;
		predicateEvent.Delegate = Delegate;
		predicateEvent.Command = Command;
		predicateEvent.Result = Result;
		return predicateEvent;
	}

	public override void Reset()
	{
		base.Reset();
		Result = false;
	}

	public override bool HandlePartDispatch(IConversationPart Part)
	{
		return Part.HandleEvent(this);
	}

	public static bool GetFor(IConversationElement Element, string Delegate, string Command, bool Result = false)
	{
		if (Element.WantEvent(ID))
		{
			PredicateEvent predicateEvent = FromPool(Element, Delegate, Command, Result);
			Element.HandleEvent(predicateEvent);
			return predicateEvent.Result;
		}
		return false;
	}
}
