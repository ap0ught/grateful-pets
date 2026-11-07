using System.Collections.Generic;
using ConsoleLib.Console;

namespace XRL.World.Conversations;

/// <summary>Fired before the current node is rendered to screen.</summary>
[ConversationEvent(Action = Action.Send)]
public class RenderNodeEvent : ConversationEvent
{
	public new static readonly int ID = ConversationEvent.RegisterEvent(typeof(RenderNodeEvent), null, CountPool, ResetPool);

	private static List<RenderNodeEvent> Pool;

	private static int PoolCounter;

	[Parameter(Reference = true)]
	public string Title;

	[Parameter(Reference = true)]
	public IRenderable Icon;

	public RenderNodeEvent()
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

	public static void ResetTo(ref RenderNodeEvent E)
	{
		ConversationEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static RenderNodeEvent FromPool()
	{
		return ConversationEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static RenderNodeEvent FromPool(IConversationElement Element, string Title, IRenderable Icon)
	{
		RenderNodeEvent renderNodeEvent = FromPool();
		renderNodeEvent.Element = Element;
		renderNodeEvent.Title = Title;
		renderNodeEvent.Icon = Icon;
		return renderNodeEvent;
	}

	public override void Reset()
	{
		base.Reset();
		Title = null;
		Icon = null;
	}

	public override bool HandlePartDispatch(IConversationPart Part)
	{
		return Part.HandleEvent(this);
	}

	public static void Send(IConversationElement Element, ref string Title, ref IRenderable Icon)
	{
		if (Element.WantEvent(ID))
		{
			RenderNodeEvent renderNodeEvent = FromPool(Element, Title, Icon);
			Element.HandleEvent(renderNodeEvent);
			Title = renderNodeEvent.Title;
			Icon = renderNodeEvent.Icon;
		}
	}
}
