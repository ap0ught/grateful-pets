using System;

namespace XRL.World.Conversations;

/// <summary>
/// Indicates that a method is a conversation delegate.
/// Depending on the return type it is either registered as a Predicate (bool), Action (void) or Generator (IConversationPart).
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ConversationDelegate : Attribute
{
	/// <summary>
	/// The key used to invoke this delegate, defaults to method name.
	/// </summary>
	public string Key;

	/// <summary>
	/// If this delegate is a conversation predicate, create another delegate returning the negated result of the first.
	/// </summary>
	public bool Inverse = true;

	/// <summary>
	/// The key used to call the inverse delegate, defaults to IfNotXYZ should the original key follow the IfXYZ PascalCase pattern.
	/// E.g. IfHaveQuest -&gt; IfNotHaveQuest.
	/// </summary>
	public string InverseKey;

	/// <summary>
	/// If this delegate is a conversation predicate or action, create another delegate with the speaker as the target instead of the player.
	/// </summary>
	public bool Speaker;

	/// <summary>
	/// The key used to call the speaker delegate, defaults to IfSpeakerXYZ should the original key follow the IfXYZ PascalCase pattern.
	/// E.g. IfHavePart -&gt; IfSpeakerHavePart, SetIntProperty -&gt; SetSpeakerIntProperty.
	/// </summary>
	public string SpeakerKey;

	/// <summary>
	/// The key used to call the inverse speaker delegate, defaults to IfSpeakerNotXYZ should the original key follow the IfXYZ PascalCase pattern.
	/// E.g. IfHavePart -&gt; IfSpeakerNotHavePart.
	/// </summary>
	public string SpeakerInverseKey;

	/// <todo>
	/// If this delegate is a conversation predicate, create another delegate which greys out and prevents navigation instead of visibility.
	/// </todo>
	public bool Require = true;

	/// <todo>
	/// The key used to call the requirement delegate, defaults to RequireXYZ should the original key follow the IfXYZ PascalCase pattern.
	/// E.g. IfGenotype -&gt; RequireGenotype, RequireNotGenotype, RequireSpeakerNotGenotype.
	/// </todo>
	public string RequireKey;
}
