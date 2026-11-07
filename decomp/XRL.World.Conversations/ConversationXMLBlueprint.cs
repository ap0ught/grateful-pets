using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Wintellect.PowerCollections;
using XRL.Language;

namespace XRL.World.Conversations;

[Serializable]
public class ConversationXMLBlueprint : IEquatable<ConversationXMLBlueprint>
{
	public const byte LOAD_MERGE = 0;

	public const byte LOAD_REPLACE = 1;

	public const byte LOAD_ADD = 2;

	public const byte LOAD_REMOVE = 3;

	public const byte DIST_NONE = 0;

	public const byte DIST_TYPE = 1;

	public const byte DIST_ID = 2;

	public const byte DIST_ESCAPE = 4;

	public const byte DIST_LOCAL = 8;

	/// <summary>
	/// An ID for this element, used for navigation, merging, and inheritance.
	/// Not required to be unique, though still recommended.
	/// </summary>
	/// <see cref="F:XRL.World.Conversations.ConversationXMLBlueprint.Cardinal" />
	public string ID;

	/// <summary>The element's type, 'Node', 'Choice', 'Text', etc.</summary>
	public string Name;

	/// <summary>The plain text content of this element.</summary>
	public string Text;

	/// <summary>The ID of one or more base blueprints to inherit.</summary>
	/// <example>Inherit a choice from a node in another conversation: 'ConversationID.NodeID.ChoiceID' -&gt; 'Barathrum.TombExplain3.QuestionsChoice'.</example>
	/// <example>Inherit a choice from a node in this conversation: 'NodeID.ChoiceID' -&gt; 'TombExplain3.QuestionsChoice'.</example>
	public string Inherits;

	/// <summary>An incrementing suffix to distinguish elements with non-unique IDs.</summary>
	/// <remarks>The default value of '1' is omitted from the final ID.</remarks>
	public int Cardinal = 1;

	/// <summary>A count of references to this element from inheritors.</summary>
	public int References;

	/// <summary>Distribute this element to be inherited by its sibling elements of specified type.</summary>
	public string Distribute;

	/// <summary>Controls distribution behavior.</summary>
	public byte Qualifier = 1;

	/// <summary>Controls the behavior on a merge/inherit conflict.</summary>
	public byte Load;

	/// <summary>
	/// A dictionary of attributes defined on this element.
	/// These are typically consumed by delegates or parsed and assigned to writeable members.
	/// </summary>
	public Dictionary<string, string> Attributes;

	/// <summary>A list of child elements.</summary>
	public List<ConversationXMLBlueprint> Children;

	public string CardinalID
	{
		get
		{
			if (Cardinal <= 1)
			{
				return ID;
			}
			return ID + Cardinal;
		}
	}

	public string this[string Key]
	{
		get
		{
			return GetAttribute(Key);
		}
		set
		{
			SetAttribute(Key, value);
		}
	}

	public ConversationXMLBlueprint()
	{
	}

	/// <summary>Shallow copy of original blueprint.</summary>
	public ConversationXMLBlueprint(ConversationXMLBlueprint Original)
		: this()
	{
		ID = Original.ID;
		Name = Original.Name;
		Text = Original.Text;
		Inherits = Original.Inherits;
		Distribute = Original.Distribute;
		Load = Original.Load;
		if (!Original.Attributes.IsNullOrEmpty())
		{
			Attributes = new Dictionary<string, string>(Original.Attributes);
		}
		if (!Original.Children.IsNullOrEmpty())
		{
			Children = new List<ConversationXMLBlueprint>(Original.Children);
		}
	}

	public void Merge(ConversationXMLBlueprint Blueprint)
	{
		if (!string.IsNullOrWhiteSpace(Blueprint.Text))
		{
			Text = Blueprint.Text;
		}
		if (!Blueprint.Distribute.IsNullOrEmpty())
		{
			Distribute = Blueprint.Distribute;
		}
		if (Blueprint.Attributes != null)
		{
			if (Attributes == null)
			{
				Attributes = new Dictionary<string, string>();
			}
			foreach (KeyValuePair<string, string> attribute in Blueprint.Attributes)
			{
				Attributes[attribute.Key] = attribute.Value;
			}
		}
		if (Blueprint.Children == null)
		{
			return;
		}
		if (Children == null)
		{
			Children = new List<ConversationXMLBlueprint>();
		}
		foreach (ConversationXMLBlueprint child in Blueprint.Children)
		{
			if (child.Load == 2)
			{
				AddChild(child);
				continue;
			}
			int num = Children.IndexOf(child);
			if (num != -1)
			{
				if (child.Load == 1)
				{
					Children[num] = child;
				}
				else if (child.Load == 3)
				{
					Children.RemoveAt(num);
				}
				else
				{
					Children[num].Merge(child);
				}
			}
			else
			{
				Children.Add(child);
			}
		}
	}

	public void Recardinate(ConversationXMLBlueprint Blueprint)
	{
		Blueprint.Cardinal = 1;
		if (Children != null)
		{
			while (Enumerable.Count(Children, Equals) > 1)
			{
				Blueprint.Cardinal++;
			}
		}
	}

	public ConversationXMLBlueprint Reference(int Mod = 1)
	{
		References += Mod;
		if (Children.IsNullOrEmpty())
		{
			return this;
		}
		for (int i = 0; i < Children.Count; i++)
		{
			Children[i].Reference(Mod);
		}
		return this;
	}

	public bool IsEmpty()
	{
		if (Attributes.IsNullOrEmpty() && Children.IsNullOrEmpty())
		{
			return Text.IsNullOrEmpty();
		}
		return false;
	}

	public void Inherit(ConversationXMLBlueprint Blueprint, BuildContext Context)
	{
		if (References > 0 && !Blueprint.IsEmpty())
		{
			ConversationXMLBlueprint conversationXMLBlueprint = new ConversationXMLBlueprint(this);
			if (Context.Peek() == this)
			{
				Context.Pop();
				Context.Push(conversationXMLBlueprint);
			}
			conversationXMLBlueprint.Inherit(Blueprint, Context);
			References--;
			return;
		}
		if (ID != Blueprint.ID && (string.IsNullOrWhiteSpace(ID) || ID == Name))
		{
			ConversationXMLBlueprint conversationXMLBlueprint2 = Context.Parent(this);
			if (conversationXMLBlueprint2 != null && IsEmpty())
			{
				int num = conversationXMLBlueprint2.Children.IndexOf(this);
				if (num == -1)
				{
					throw new Exception("Orphaned child by ID: " + Context.AssemblePathID(this));
				}
				conversationXMLBlueprint2.Children[num] = Blueprint.Reference();
				conversationXMLBlueprint2.Recardinate(Blueprint);
				if (Context.Peek() == this)
				{
					Context.Pop();
					Context.Push(Blueprint);
				}
				return;
			}
			ID = Blueprint.ID;
			conversationXMLBlueprint2?.Recardinate(this);
		}
		if (string.IsNullOrWhiteSpace(Text))
		{
			Text = Blueprint.Text;
		}
		if (Distribute == null)
		{
			Distribute = Blueprint.Distribute;
		}
		if (Blueprint.Attributes != null)
		{
			if (Attributes == null)
			{
				Attributes = new Dictionary<string, string>();
			}
			foreach (KeyValuePair<string, string> attribute in Blueprint.Attributes)
			{
				if (!Attributes.ContainsKey(attribute.Key))
				{
					Attributes[attribute.Key] = attribute.Value;
				}
			}
		}
		if (Blueprint.Children != null)
		{
			if (Children == null)
			{
				Children = new List<ConversationXMLBlueprint>();
			}
			for (int num2 = Blueprint.Children.Count - 1; num2 >= 0; num2--)
			{
				InheritChild(Blueprint.Children[num2], Context);
			}
		}
	}

	public void InheritChild(ConversationXMLBlueprint Child, BuildContext Context, bool Insert = true)
	{
		if (Children == null)
		{
			Children = new List<ConversationXMLBlueprint>();
		}
		int num = Children.IndexOf(Child);
		if (num != -1)
		{
			ConversationXMLBlueprint conversationXMLBlueprint = Children[num];
			if (conversationXMLBlueprint.Load == 2)
			{
				Children.Insert((!Insert) ? Children.Count : 0, Child.Reference());
				Recardinate(conversationXMLBlueprint);
			}
			else if (conversationXMLBlueprint.Load == 3)
			{
				Children.RemoveAt(num);
			}
			else if (conversationXMLBlueprint.Load != 1)
			{
				Children[num].Inherit(Child, Context);
			}
		}
		else
		{
			Children.Insert((!Insert) ? Children.Count : 0, Child.Reference());
		}
	}

	public bool ResolveBlueprint(string ID, BuildContext Context)
	{
		ConversationXMLBlueprint value;
		if (ID.Split('.').Length == 1)
		{
			if (!Context.Finalized.TryGetValue(Context.AssembleNamedID(this, ID), out value) && !Context.Finalized.TryGetValue(Context.AssemblePathID(ID, 1), out value))
			{
				return Context.Missing(this, ID);
			}
		}
		else if (!Context.Finalized.TryGetValue(ID, out value))
		{
			ConversationXMLBlueprint conversationXMLBlueprint = Context.Lineage.LastOrDefault();
			if (conversationXMLBlueprint == null || conversationXMLBlueprint == this)
			{
				return Context.Missing(this, ID);
			}
			Context.Text.Clear().Append(conversationXMLBlueprint.ID).Append('.')
				.Append(ID);
			if (!Context.Finalized.TryGetValue(Context.Text.ToString(), out value))
			{
				return Context.Missing(this, ID);
			}
		}
		Inherit(value, Context);
		return true;
	}

	public void DistributeChildren(BuildContext Context)
	{
		Context.Push(this);
		if (!Distribute.IsNullOrEmpty())
		{
			string[] self = Distribute.Split(',');
			bool flag = Qualifier == 2;
			bool flag2 = !flag && Distribute == "Start";
			ConversationXMLBlueprint conversationXMLBlueprint = Context.Parent();
			if (conversationXMLBlueprint?.Children == null)
			{
				return;
			}
			foreach (ConversationXMLBlueprint child in conversationXMLBlueprint.Children)
			{
				if (child == this || !child.Distribute.IsNullOrEmpty())
				{
					continue;
				}
				if (flag)
				{
					if (self.Contains(child.ID))
					{
						child.InheritChild(this, Context, Insert: false);
					}
				}
				else if (self.Contains(child.Name) || (flag2 && child.Name == "Node" && child.ID == "Start"))
				{
					child.InheritChild(this, Context, Insert: false);
				}
			}
			conversationXMLBlueprint.Children.Remove(this);
		}
		else if (!Children.IsNullOrEmpty())
		{
			Algorithms.StableSortInPlace(Children, CompareDistributable);
			for (int num = Children.Count - 1; num >= 0; num--)
			{
				Children[num].DistributeChildren(Context);
			}
		}
		Context.Pop();
	}

	private int CompareDistributable(ConversationXMLBlueprint A, ConversationXMLBlueprint B)
	{
		if (A.Distribute.IsNullOrEmpty())
		{
			if (!B.Distribute.IsNullOrEmpty())
			{
				return 1;
			}
			return 0;
		}
		if (!B.Distribute.IsNullOrEmpty())
		{
			return 0;
		}
		return -1;
	}

	public bool Bake(BuildContext Context)
	{
		if (!ID.IsNullOrEmpty() && Context.Finalized.ContainsKey(Context.AssemblePathID(this)))
		{
			return true;
		}
		Context.Push(this);
		bool flag = true;
		if (!Children.IsNullOrEmpty())
		{
			for (int i = 0; i < Children.Count; i++)
			{
				flag &= Children[i].Bake(Context);
			}
		}
		if (!Inherits.IsNullOrEmpty())
		{
			if (Inherits.Contains(','))
			{
				string[] array = Inherits.Split(',');
				foreach (string iD in array)
				{
					flag &= ResolveBlueprint(iD, Context);
				}
			}
			else
			{
				flag &= ResolveBlueprint(Inherits, Context);
			}
		}
		ConversationXMLBlueprint blueprint = Context.Pop();
		if (flag)
		{
			Context.Finalized[Context.AssemblePathID(blueprint)] = this;
			Context.Finalized.TryAdd(Context.AssembleNamedID(blueprint), this);
			if (!Children.IsNullOrEmpty())
			{
				for (int num = Children.Count - 1; num >= 0; num--)
				{
					if (TrySortAt(num) < num)
					{
						num++;
					}
				}
			}
		}
		return flag;
	}

	public string GetAttribute(string Key)
	{
		Dictionary<string, string> attributes = Attributes;
		if (attributes == null)
		{
			return null;
		}
		return attributes.GetValue(Key);
	}

	public void SetAttribute(string Key, string Value)
	{
		if (Attributes == null)
		{
			Attributes = new Dictionary<string, string>();
		}
		Attributes[Key] = Value;
	}

	public void RemoveAttribute(string Key)
	{
		Attributes?.Remove(Key);
	}

	public void AddChild(ConversationXMLBlueprint Child)
	{
		if (Children == null)
		{
			Children = new List<ConversationXMLBlueprint>();
		}
		while (Children.IndexOf(Child) >= 0)
		{
			Child.Cardinal++;
		}
		Children.Add(Child);
	}

	public ConversationXMLBlueprint GetChild(params string[] Path)
	{
		ConversationXMLBlueprint conversationXMLBlueprint = this;
		for (int i = 0; i < Path.Length; i++)
		{
			if (conversationXMLBlueprint == null)
			{
				break;
			}
			conversationXMLBlueprint = conversationXMLBlueprint.GetChild(Path[i]);
		}
		return conversationXMLBlueprint;
	}

	public ConversationXMLBlueprint GetChild(string First, string Second)
	{
		return GetChild(First)?.GetChild(Second);
	}

	public ConversationXMLBlueprint GetChild(string First, string Second, string Third)
	{
		return GetChild(First)?.GetChild(Second)?.GetChild(Third);
	}

	public ConversationXMLBlueprint GetChild(string CardinalID)
	{
		int childIndex = GetChildIndex(CardinalID);
		if (childIndex == -1)
		{
			return null;
		}
		return Children[childIndex];
	}

	public ConversationXMLBlueprint GetChild(ReadOnlySpan<char> CardinalID)
	{
		int childIndex = GetChildIndex(CardinalID);
		if (childIndex == -1)
		{
			return null;
		}
		return Children[childIndex];
	}

	public ConversationXMLBlueprint GetChild(ReadOnlySpan<char> ID, int Cardinal)
	{
		int childIndex = GetChildIndex(ID, Cardinal);
		if (childIndex == -1)
		{
			return null;
		}
		return Children[childIndex];
	}

	public ConversationXMLBlueprint GetChild(string ID, int Cardinal)
	{
		int childIndex = GetChildIndex(ID, Cardinal);
		if (childIndex == -1)
		{
			return null;
		}
		return Children[childIndex];
	}

	public int GetChildIndex(ReadOnlySpan<char> CardinalID)
	{
		int num = -1;
		int num2 = CardinalID.Length - 1;
		while (num2 >= 0 && char.IsNumber(CardinalID[num2]))
		{
			num = num2;
			num2--;
		}
		if (num < 0)
		{
			return GetChildIndex(CardinalID, 1);
		}
		int childIndex = GetChildIndex(CardinalID.Slice(0, num), int.Parse(CardinalID.Slice(num)));
		if (childIndex == -1)
		{
			return GetChildIndex(CardinalID, 1);
		}
		return childIndex;
	}

	public int GetChildIndex(ReadOnlySpan<char> ID, int Cardinal)
	{
		if (Children == null)
		{
			return -1;
		}
		int i = 0;
		for (int count = Children.Count; i < count; i++)
		{
			ConversationXMLBlueprint conversationXMLBlueprint = Children[i];
			if (conversationXMLBlueprint.Cardinal == Cardinal && ID.SequenceEqual(conversationXMLBlueprint.ID))
			{
				return i;
			}
		}
		return -1;
	}

	public int GetChildIndex(string ID, int Cardinal)
	{
		if (Children == null)
		{
			return -1;
		}
		int i = 0;
		for (int count = Children.Count; i < count; i++)
		{
			ConversationXMLBlueprint conversationXMLBlueprint = Children[i];
			if (conversationXMLBlueprint.Cardinal == Cardinal && !(conversationXMLBlueprint.ID != ID))
			{
				return i;
			}
		}
		return -1;
	}

	private int TrySortAt(int Index)
	{
		ConversationXMLBlueprint conversationXMLBlueprint = Children[Index];
		if (conversationXMLBlueprint.Attributes.IsNullOrEmpty())
		{
			return Index;
		}
		int num = 0;
		if (!conversationXMLBlueprint.Attributes.TryGetValue("Before", out var value))
		{
			if (!conversationXMLBlueprint.Attributes.TryGetValue("After", out value))
			{
				return Index;
			}
			num = 1;
		}
		int num2 = -1;
		DelimitedEnumeratorChar enumerator = value.DelimitedBy(',').GetEnumerator();
		while (enumerator.MoveNext())
		{
			ReadOnlySpan<char> current = enumerator.Current;
			num2 = GetChildIndex(current);
			if (num2 != -1)
			{
				break;
			}
		}
		if (num2 != -1 && num2 != Index)
		{
			Children.RemoveAt(Index);
			if (num2 > Index)
			{
				num2--;
			}
			num2 = TrySortAt(num2) + num;
			Children.Insert(num2, conversationXMLBlueprint);
			return num2;
		}
		return Index;
	}

	public void ReadAttributes(XmlTextReader Reader, BuildContext Context)
	{
		if (!Reader.HasAttributes)
		{
			ID = Name;
			if (Name == "Choice" && Context.Parent()?.Name == "Conversation")
			{
				Distribute = "Start";
			}
			return;
		}
		Attributes = new Dictionary<string, string>();
		while (Reader.MoveToNextAttribute())
		{
			string Value = Reader.Value;
			string text = ReadAttribute(Reader, ref Value, Context);
			if (!text.IsNullOrEmpty())
			{
				Attributes[string.Intern(text)] = string.Intern(Value);
			}
		}
		if (Distribute == null && Name == "Choice" && Context.Parent()?.Name == "Conversation")
		{
			Distribute = "Start";
		}
	}

	public string ReplaceText(string Text)
	{
		string text = "";
		if (!Children.IsNullOrEmpty())
		{
			foreach (ConversationXMLBlueprint child in Children)
			{
				if (child.Name == "Text")
				{
					text = child.Text;
					child.Text = Text;
					return text;
				}
			}
		}
		text = this.Text;
		this.Text = Text;
		return text;
	}

	public string ReadAttribute(XmlTextReader Reader, ref string Value, BuildContext Context)
	{
		string name = Reader.Name;
		switch (name)
		{
		case "GotoID":
			if (ID == null)
			{
				ID = string.Intern(Value + Name);
			}
			return "Target";
		case "ID":
			ID = string.Intern(Value);
			return null;
		case "Target":
			if (ID == null)
			{
				ID = string.Intern(Value + Name);
			}
			return name;
		case "Name":
			if (ID == null)
			{
				ID = string.Intern(Value);
			}
			if (Name == "Part")
			{
				if (!Context.Namespace.IsNullOrEmpty())
				{
					string text = Context.Namespace + "." + Value;
					if (IConversationPart.CanResolve(text))
					{
						Value = string.Intern(text);
						return name;
					}
				}
				if (Value.IndexOf('.') < 0)
				{
					string text2 = "XRL.World.Conversations.Parts." + Value;
					if (IConversationPart.CanResolve(text2))
					{
						Value = string.Intern(text2);
						return name;
					}
				}
				if (!IConversationPart.CanResolve(Value))
				{
					MetricsManager.LogFileError(Context.File, "No part of name '" + Value + "' could be resolved.", Reader);
				}
			}
			return name;
		case "Namespace":
			return null;
		case "UseID":
			if (Inherits == null)
			{
				Inherits = string.Intern(Value);
			}
			return null;
		case "Inherits":
			Inherits = string.Intern(Value);
			return null;
		case "Distribute":
			Distribute = string.Intern(Value);
			return null;
		case "Cardinal":
		{
			if (int.TryParse(Value, out var result))
			{
				Cardinal = result;
			}
			else
			{
				MetricsManager.LogFileError(Context.File, "Invalid merge cardinal: " + Value, Reader);
			}
			return null;
		}
		case "IfHavePart":
		case "IfNotHavePart":
		case "IfSpeakerHavePart":
		case "IfSpeakerNotHavePart":
		{
			if (CompatManager.TryGetPart(Value, out var NewPart, out var Type))
			{
				MetricsManager.LogFileWarning(Context.File, Context.AssemblePathID() + "." + name + ": Referencing deprecated " + Type + " '" + Value + "' which has been replaced by '" + NewPart + "'.", Reader);
				Value = NewPart;
			}
			return name;
		}
		case "Qualifier":
			Qualifier = Value.ToLowerInvariant() switch
			{
				"id" => (byte)2, 
				"name" => (byte)1, 
				"type" => (byte)1, 
				_ => (byte)0, 
			};
			return null;
		case "Load":
			Load = Value.ToLowerInvariant() switch
			{
				"replace" => (byte)1, 
				"add" => (byte)2, 
				"remove" => (byte)3, 
				_ => (byte)0, 
			};
			return null;
		default:
			return name;
		}
	}

	public void Read(XmlTextReader Reader, BuildContext Context)
	{
		Context.Push(this);
		Context.Record(this, Reader);
		Name = string.Intern(Grammar.InitCap(Reader.Name));
		if (Reader.IsEmptyElement || Reader.NodeType == XmlNodeType.EndElement)
		{
			ReadAttributes(Reader, Context);
			Context.Pop();
			return;
		}
		ReadAttributes(Reader, Context);
		while (Reader.Read())
		{
			switch (Reader.NodeType)
			{
			case XmlNodeType.Element:
			{
				ConversationXMLBlueprint conversationXMLBlueprint = new ConversationXMLBlueprint();
				conversationXMLBlueprint.Read(Reader, Context);
				AddChild(conversationXMLBlueprint);
				break;
			}
			case XmlNodeType.EndElement:
				Context.Pop();
				return;
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
				Context.Text.Clear().Append(Reader.Value).Unindent();
				if (Name == "Text" || Name == "Part")
				{
					Text = Context.Text.ToString();
					break;
				}
				AddChild(new ConversationXMLBlueprint
				{
					Name = "Text",
					ID = "Text",
					Text = Context.Text.ToString()
				});
				break;
			}
		}
		Context.Pop();
	}

	public bool Equals(ConversationXMLBlueprint Other)
	{
		if (Other == null)
		{
			return false;
		}
		if (this == Other)
		{
			return true;
		}
		if (Cardinal == Other.Cardinal && Name == Other.Name)
		{
			return ID == Other.ID;
		}
		return false;
	}
}
