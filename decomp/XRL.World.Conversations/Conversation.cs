using System.Collections.Generic;
using System.Linq;
using Wintellect.PowerCollections;
using XRL.UI;

namespace XRL.World.Conversations;

[HasModSensitiveStaticCache]
public class Conversation : IConversationElement
{
	[ModSensitiveStaticCache(false)]
	public static Dictionary<string, ConversationXMLBlueprint> _Blueprints;

	public List<Node> Starts;

	public Dictionary<string, object> State = new Dictionary<string, object>();

	public static Dictionary<string, ConversationXMLBlueprint> Blueprints
	{
		get
		{
			if (_Blueprints == null)
			{
				ConversationLoader.CheckInit();
			}
			return _Blueprints;
		}
	}

	public static Conversation Current => ConversationUI.CurrentConversation;

	public static GameObject Speaker => ConversationUI.Speaker;

	public static GameObject Listener => ConversationUI.Listener;

	public static GameObject Transmitter => ConversationUI.Transmitter;

	public static GameObject Receiver => ConversationUI.Receiver;

	/// <summary>Retrieve or set state associated with specified key.</summary>
	/// <param name="Key">A nullable string.</param>
	/// <returns><c>null</c> if Key not found.</returns>
	public object this[string Key]
	{
		get
		{
			if (Key == null || !State.TryGetValue(Key, out var value))
			{
				return null;
			}
			return value;
		}
		set
		{
			State[Key] = value;
		}
	}

	public override int Propagation => 2;

	public Conversation()
	{
	}

	public Conversation(ConversationXMLBlueprint Blueprint)
	{
		Load(Blueprint);
	}

	/// <summary>Retrieve and cast state associated with specified key.</summary>
	public bool TryGetState<T>(string Key, out T Value)
	{
		if (Key != null && State.TryGetValue(Key, out var value))
		{
			Value = (T)value;
			return true;
		}
		Value = default;
		return false;
	}

	public bool HasState(string Key)
	{
		if (Key != null)
		{
			return State.ContainsKey(Key);
		}
		return false;
	}

	public bool HasDelimitedState(string Key, char Separator, string Value)
	{
		if (State.TryGetValue(Key, out var value) && value is string text)
		{
			return text.HasDelimitedSubstring(Separator, Value);
		}
		return false;
	}

	public void SetState(string Key, object Value)
	{
		State[Key] = Value;
	}

	public bool RemoveState(string Key)
	{
		if (Key != null)
		{
			return State.Remove(Key);
		}
		return false;
	}

	public override bool LoadChild(ConversationXMLBlueprint Blueprint)
	{
		if (Blueprint.Name == "Start" || (Blueprint.ID == "Start" && Blueprint.Name == "Node"))
		{
			if (Elements == null)
			{
				Elements = new List<IConversationElement>();
			}
			if (Starts == null)
			{
				Starts = new List<Node>();
			}
			Node item = Create<Node>(Blueprint);
			Elements.Add(item);
			Starts.Add(item);
			return true;
		}
		return base.LoadChild(Blueprint);
	}

	public Node GetStart(string ID = null)
	{
		if (ID != null && !Elements.IsNullOrEmpty() && Elements.FirstOrDefault((IConversationElement x) => x.ID == ID) is Node result)
		{
			return result;
		}
		if (Starts.IsNullOrEmpty())
		{
			return null;
		}
		Starts.ForEach((Node x) =>
		{
			x.Awake();
		});
		Algorithms.StableSortInPlace(Starts);
		return Starts.FirstOrDefault((Node x) => x.IsVisible());
	}
}
