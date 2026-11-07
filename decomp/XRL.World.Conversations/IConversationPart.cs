using System;
using System.Collections.Generic;
using System.Reflection;
using XRL.UI;

namespace XRL.World.Conversations;

public abstract class IConversationPart : IComparable<IConversationPart>, IDisposable
{
	public const string DEF_NAMESPACE = "XRL.World.Conversations.Parts";

	public const int PROPAGATE_UNINITIALIZED = -1;

	public const int PROPAGATE_NONE = 0;

	public const int PROPAGATE_LISTENER = 1;

	public const int PROPAGATE_SPEAKER = 2;

	public IConversationElement ParentElement;

	/// <summary>Controls the part's order of execution for events, higher values preceding lower ones.</summary>
	public int Priority;

	public int Propagation = -1;

	public string Register
	{
		set
		{
			Propagation = value.ToLowerInvariant() switch
			{
				"listener" => 1, 
				"player" => 1, 
				"speaker" => 2, 
				"all" => 3, 
				_ => 0, 
			};
		}
	}

	protected static string LastChoiceID => ConversationUI.LastChoice?.ID;

	protected static Choice LastChoice => ConversationUI.LastChoice;

	protected static Dictionary<string, object> State => The.Conversation.State;

	public virtual bool HandleEvent(ColorTextEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(DisplayTextEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EnterElementEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(EnteredElementEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetChoiceTagEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetTargetElementEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(GetTextElementEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(HideElementEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(IsElementVisibleEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(LeaveElementEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(LeftElementEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(PredicateEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(PrepareTextEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(PrepareTextLateEvent E)
	{
		return true;
	}

	public virtual bool HandleEvent(RenderNodeEvent E)
	{
		return true;
	}

	/// <summary>Called once when the part is constructed and added to its parent element.</summary>
	public virtual void Initialize()
	{
		if (Propagation == -1)
		{
			if (ParentElement == null)
			{
				Propagation = 0;
			}
			else
			{
				Propagation = ParentElement.Propagation;
			}
		}
	}

	/// <summary>Called once before the parent element is first queried for any activity.</summary>
	public virtual void Awake()
	{
	}

	public virtual bool PropagateTo(int Propagation)
	{
		return this.Propagation.HasBit(Propagation);
	}

	public virtual bool WantEvent(int ID, int Propagation)
	{
		return false;
	}

	public virtual bool HandleEvent(ConversationEvent E)
	{
		return true;
	}

	public virtual void Enter()
	{
	}

	public virtual bool Load(ConversationXMLBlueprint Blueprint)
	{
		if (!Blueprint.Text.IsNullOrEmpty())
		{
			LoadText(Blueprint.Text);
		}
		if (!Blueprint.Attributes.IsNullOrEmpty())
		{
			LoadAttributes(Blueprint.Attributes);
		}
		if (Blueprint.Children == null)
		{
			return true;
		}
		foreach (ConversationXMLBlueprint child in Blueprint.Children)
		{
			try
			{
				LoadChild(child);
			}
			catch (Exception x)
			{
				MetricsManager.LogError("Error adding " + child.Name + " by ID " + child.ID, x);
			}
		}
		return true;
	}

	public virtual void LoadText(string Text)
	{
	}

	public virtual void LoadAttributes(Dictionary<string, string> Attributes)
	{
		Type type = GetType();
		foreach (KeyValuePair<string, string> Attribute in Attributes)
		{
			try
			{
				FieldInfo field = type.GetField(Attribute.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if ((object)field != null && !field.IsInitOnly)
				{
					field.SetValue(this, Convert.ChangeType(Attribute.Value, field.FieldType));
					continue;
				}
				PropertyInfo property = type.GetProperty(Attribute.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if ((object)property != null && property.CanWrite)
				{
					property.SetValue(this, Convert.ChangeType(Attribute.Value, property.PropertyType));
				}
			}
			catch (Exception ex)
			{
				MetricsManager.LogAssemblyError(type.Assembly, $"Error setting field '{type.FullName}.{Attribute.Key}' to '{Attribute.Value}': {ex}");
			}
		}
	}

	public virtual bool LoadChild(ConversationXMLBlueprint Blueprint)
	{
		return true;
	}

	public virtual bool FireEvent(Event E)
	{
		return true;
	}

	public static bool CanResolve(string Name)
	{
		return (object)ModManager.ResolveType(Name, IgnoreCase: false, ThrowOnError: false, Cache: false) != null;
	}

	public static bool TryResolve(string Name, out Type Type)
	{
		if (Name.IndexOf('.') == -1)
		{
			Name = "XRL.World.Conversations.Parts." + Name;
		}
		Type = ModManager.ResolveType(Name);
		return (object)Type != null;
	}

	public static bool TryCreate(string Name, ConversationXMLBlueprint Blueprint, out IConversationPart Part)
	{
		if (!TryResolve(Name, out var Type))
		{
			Part = null;
			MetricsManager.LogError("No part of name '" + Name + "' could be resolved.");
			return false;
		}
		try
		{
			Part = Activator.CreateInstance(Type) as IConversationPart;
			if (Part == null)
			{
				MetricsManager.LogAssemblyError(Type, "Type of name '" + Name + "' is not a valid instance of IConversationPart.");
				return false;
			}
		}
		catch (Exception ex)
		{
			MetricsManager.LogAssemblyError(Type, "Exception when instantiating part '" + Name + "': " + ex);
			Part = null;
			return false;
		}
		return Part.Load(Blueprint);
	}

	/// <summary>Retrieve and cast state associated with specified key.</summary>
	protected static bool TryGetState<T>(string Key, out T Value)
	{
		if (Key != null && State.TryGetValue(Key, out var value))
		{
			Value = (T)value;
			return true;
		}
		Value = default;
		return false;
	}

	public int CompareTo(IConversationPart other)
	{
		return Priority.CompareTo(other.Priority);
	}

	public virtual void Dispose()
	{
	}
}
