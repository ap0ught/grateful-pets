using System;

namespace XRL.World.Conversations;

/// <summary>Marks a class for generation of conversation event partials.</summary>
[AttributeUsage(AttributeTargets.Class)]
public class ConversationEventAttribute : Attribute
{
	public bool Base;

	public ConversationEvent.Action Action;

	public ConversationEvent.Instantiation Instantiation = ConversationEvent.Instantiation.Pooling;
}
