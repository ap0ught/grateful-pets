using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using XRL.UI;

namespace XRL.World.Conversations;

public abstract class IConversationElement : IComparable<IConversationElement>, IDisposable
{
	public const int END_SORT_ORDINAL = 999999;

	public static HashSet<Type> ConvertTypes = new HashSet<Type>
	{
		typeof(object),
		typeof(DBNull),
		typeof(bool),
		typeof(char),
		typeof(sbyte),
		typeof(byte),
		typeof(short),
		typeof(ushort),
		typeof(int),
		typeof(uint),
		typeof(long),
		typeof(ulong),
		typeof(float),
		typeof(double),
		typeof(decimal),
		typeof(DateTime),
		typeof(string)
	};

	public string ID;

	public string Text;

	public int Priority;

	public bool Awoken;

	public IConversationElement Parent;

	public List<IConversationPart> Parts;

	public List<IConversationElement> Elements;

	public List<ConversationText> Texts;

	public Dictionary<string, string> Predicates;

	public Dictionary<string, string> Actions;

	public Dictionary<string, string> Attributes;

	public int Ordinal
	{
		set
		{
			Priority = -value;
		}
	}

	public string PathID
	{
		get
		{
			if (Parent != null)
			{
				return Parent.PathID + "." + ID;
			}
			return ID;
		}
	}

	public virtual int Propagation => 3;

	public IConversationElement GetText()
	{
		if (Texts.IsNullOrEmpty())
		{
			return null;
		}
		GetTextElementEvent E = GetTextElementEvent.FromPool();
		int num = int.MinValue;
		foreach (ConversationText text in Texts)
		{
			E.Texts.Add(text);
			if (text.IsVisible())
			{
				E.Visible.Add(text);
				if (text.Priority > num)
				{
					num = text.Priority;
				}
			}
		}
		if (E.Visible.Count == 0)
		{
			return null;
		}
		foreach (ConversationText item in E.Visible)
		{
			if (item.Priority == num)
			{
				E.Group.Add(item);
			}
		}
		E.Selected = E.Group.GetRandomElement();
		E.Element = this;
		HandleEvent(E);
		ConversationText selected = E.Selected;
		GetTextElementEvent.ResetTo(ref E);
		if (!selected.Text.IsNullOrEmpty())
		{
			return selected;
		}
		return selected.GetText();
	}

	public IConversationElement GetAncestor(Predicate<IConversationElement> Predicate, bool IncludeSelf = true)
	{
		IConversationElement conversationElement = (IncludeSelf ? this : Parent);
		for (int i = 0; i < 100; i++)
		{
			if (conversationElement == null)
			{
				break;
			}
			if (Predicate(conversationElement))
			{
				return conversationElement;
			}
			conversationElement = conversationElement.Parent;
		}
		return null;
	}

	public virtual void Awake()
	{
		if (Awoken)
		{
			return;
		}
		if (Parts != null)
		{
			foreach (IConversationPart part in Parts)
			{
				part.Awake();
			}
		}
		Awoken = true;
	}

	public virtual void Prepare()
	{
		if (Text == null)
		{
			IConversationElement text = GetText();
			if (text != null)
			{
				Text = text.Text.GetRandomSubstring('~', Trim: true);
				StringBuilder stringBuilder = Event.NewStringBuilder(Text.Trim());
				GameObject Subject = The.Speaker;
				GameObject Object = null;
				PrepareTextEvent.Send(text, stringBuilder, ref Subject, ref Object, out var ExplicitSubject, out var ExplicitSubjectPlural, out var ExplicitObject, out var ExplicitObjectPlural);
				GameObject subject = Subject;
				GameObject gameObject = Object;
				Text = GameText.VariableReplace(stringBuilder, subject, ExplicitSubject, ExplicitSubjectPlural, gameObject, ExplicitObject, ExplicitObjectPlural);
				PrepareTextLateEvent.Send(text, Subject, Object, ref Text);
			}
		}
	}

	public virtual string GetDisplayText(bool WithColor = false)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder(Text);
		GameObject Subject = The.Speaker;
		GameObject Object = null;
		DisplayTextEvent.Send(this, stringBuilder, ref Subject, ref Object, out var ExplicitSubject, out var ExplicitSubjectPlural, out var ExplicitObject, out var ExplicitObjectPlural, out var VariableReplace);
		if (VariableReplace)
		{
			GameObject subject = Subject;
			GameObject gameObject = Object;
			GameText.VariableReplace(stringBuilder, subject, ExplicitSubject, ExplicitSubjectPlural, gameObject, ExplicitObject, ExplicitObjectPlural);
		}
		if (WithColor)
		{
			stringBuilder.Insert(0, '|');
			stringBuilder.Insert(0, GetTextColor());
			stringBuilder.Insert(0, "{{");
			stringBuilder.Append("}}");
		}
		return stringBuilder.ToString();
	}

	public virtual string GetTextColor()
	{
		string Color = "y";
		ColorTextEvent.Send(this, ref Color);
		return Color;
	}

	public bool WantEvent(int ID)
	{
		return WantEvent(ID, Propagation);
	}

	private bool WantEvent(int ID, int Propagation)
	{
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].PropagateTo(Propagation) && Parts[i].WantEvent(ID, Propagation))
				{
					return true;
				}
			}
		}
		if (Parent != null)
		{
			return Parent.WantEvent(ID, Propagation);
		}
		return false;
	}

	public bool HandleEvent(ConversationEvent E)
	{
		return HandleEvent(E, Propagation);
	}

	private bool HandleEvent(ConversationEvent E, int Propagation)
	{
		if (Parts != null)
		{
			for (int i = 0; i < Parts.Count; i++)
			{
				if (Parts[i].PropagateTo(Propagation) && Parts[i].WantEvent(E.ID, Propagation))
				{
					if (!E.HandlePartDispatch(Parts[i]))
					{
						return false;
					}
					Parts[i].HandleEvent(E);
				}
			}
		}
		if (Parent != null)
		{
			return Parent.HandleEvent(E, Propagation);
		}
		return true;
	}

	public virtual bool Enter()
	{
		if (!EnterElementEvent.Check(this))
		{
			return false;
		}
		return true;
	}

	public virtual void Entered()
	{
		if (!Actions.IsNullOrEmpty())
		{
			foreach (KeyValuePair<string, string> action in Actions)
			{
				if (!ConversationDelegates.Actions.TryGetValue(action.Key, out var value))
				{
					continue;
				}
				try
				{
					string value2 = action.Value;
					if (!value2.StartsWith('(') || !Expression.Evaluate(value2, this, value).HasValue)
					{
						value(this, value2);
					}
				}
				catch (Exception x)
				{
					MetricsManager.LogException("Exception when executing action '" + action.Key + "' of '" + PathID + "'", x);
				}
			}
		}
		EnteredElementEvent.Send(this);
	}

	public virtual bool Leave()
	{
		if (!LeaveElementEvent.Check(this))
		{
			return false;
		}
		return true;
	}

	public virtual void Left()
	{
		Reset();
		LeftElementEvent.Send(this);
	}

	public virtual void Reset()
	{
		if (ConversationUI.StartNode != this && !Texts.IsNullOrEmpty())
		{
			Text = null;
		}
	}

	public bool FireEvent(Event E)
	{
		return FireEvent(E, Propagation);
	}

	private bool FireEvent(Event E, int Propagation)
	{
		if (Parts != null)
		{
			for (int i = 0; i < Parts.Count; i++)
			{
				if (Parts[i].PropagateTo(Propagation) && !Parts[i].FireEvent(E))
				{
					return false;
				}
			}
		}
		if (Parent != null)
		{
			return Parent.FireEvent(E, Propagation);
		}
		return true;
	}

	public bool CheckPredicates()
	{
		return CheckPredicates(Predicates);
	}

	public bool CheckPredicates(Dictionary<string, string> Predicates)
	{
		if (Predicates.IsNullOrEmpty())
		{
			return true;
		}
		foreach (KeyValuePair<string, string> Predicate in Predicates)
		{
			if (!CheckPredicate(Predicate.Key, Predicate.Value))
			{
				return false;
			}
		}
		return true;
	}

	public bool CheckPredicate(string Key, string Value, bool Default = true)
	{
		if (ConversationDelegates.Predicates.TryGetValue(Key, out var value))
		{
			try
			{
				if (Value.StartsWith('(') || Value.StartsWith("NOT (", StringComparison.Ordinal))
				{
					bool? flag = Expression.Evaluate(Value, this, value);
					if (flag.HasValue)
					{
						return flag.Value;
					}
				}
				return value(this, Value);
			}
			catch (Exception x)
			{
				MetricsManager.LogException("Exception when executing predicate '" + Key + "' of '" + PathID + "'", x);
			}
		}
		return Default;
	}

	public virtual bool IsVisible()
	{
		if (CheckPredicates())
		{
			return IsElementVisibleEvent.Check(this);
		}
		return false;
	}

	public IConversationElement GetElement(params string[] Path)
	{
		IConversationElement conversationElement = this;
		for (int i = 0; i < Path.Length; i++)
		{
			if (conversationElement == null)
			{
				break;
			}
			conversationElement = conversationElement.GetElementByID(Path[i]);
		}
		return conversationElement;
	}

	public IConversationElement GetElementByID(string ID)
	{
		if (Elements != null)
		{
			int i = 0;
			for (int count = Elements.Count; i < count; i++)
			{
				if (Elements[i].ID == ID)
				{
					return Elements[i];
				}
			}
		}
		if (Texts != null)
		{
			int j = 0;
			for (int count2 = Texts.Count; j < count2; j++)
			{
				if (Texts[j].ID == ID)
				{
					return Texts[j];
				}
			}
		}
		return null;
	}

	public bool TryGetAttribute(string Key, out string Value)
	{
		if (Attributes == null)
		{
			Value = null;
			return false;
		}
		return Attributes.TryGetValue(Key, out Value);
	}

	public virtual void LoadAttributes(Dictionary<string, string> Attributes)
	{
		if (Attributes == null || Attributes.Count == 0)
		{
			return;
		}
		Type type = GetType();
		foreach (KeyValuePair<string, string> Attribute in Attributes)
		{
			if (ConversationDelegates.Predicates.ContainsKey(Attribute.Key))
			{
				if (Predicates == null)
				{
					Predicates = new Dictionary<string, string>();
				}
				Predicates[Attribute.Key] = Attribute.Value;
				continue;
			}
			if (ConversationDelegates.Actions.ContainsKey(Attribute.Key))
			{
				if (Actions == null)
				{
					Actions = new Dictionary<string, string>();
				}
				Actions[Attribute.Key] = Attribute.Value;
				continue;
			}
			if (ConversationDelegates.PartGenerators.TryGetValue(Attribute.Key, out var value))
			{
				IConversationPart conversationPart = value(this, Attribute.Value);
				if (conversationPart != null)
				{
					AddPart(conversationPart, Sort: false);
				}
				continue;
			}
			FieldInfo field = type.GetField(Attribute.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if ((object)field != null && !field.IsInitOnly && ConvertTypes.Contains(field.FieldType))
			{
				field.SetValue(this, Convert.ChangeType(Attribute.Value, field.FieldType));
				continue;
			}
			PropertyInfo property = type.GetProperty(Attribute.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if ((object)property != null && property.CanWrite && ConvertTypes.Contains(property.PropertyType))
			{
				property.SetValue(this, Convert.ChangeType(Attribute.Value, property.PropertyType));
				continue;
			}
			if (this.Attributes == null)
			{
				this.Attributes = new Dictionary<string, string>();
			}
			this.Attributes[Attribute.Key] = Attribute.Value;
		}
	}

	public T Create<T>(ConversationXMLBlueprint Blueprint) where T : IConversationElement, new()
	{
		T val = new T();
		val.Parent = this;
		val.Load(Blueprint);
		return val;
	}

	public virtual bool LoadChild(ConversationXMLBlueprint Blueprint)
	{
		switch (Blueprint.Name)
		{
		case "Node":
			if (Elements == null)
			{
				Elements = new List<IConversationElement>();
			}
			Elements.Add(Create<Node>(Blueprint));
			break;
		case "Choice":
			if (Elements == null)
			{
				Elements = new List<IConversationElement>();
			}
			Elements.Add(Create<Choice>(Blueprint));
			break;
		case "Text":
			if (Texts == null)
			{
				Texts = new List<ConversationText>();
			}
			Texts.Add(Create<ConversationText>(Blueprint));
			break;
		case "Part":
		{
			if (IConversationPart.TryCreate(Blueprint.Attributes["Name"], Blueprint, out var Part))
			{
				AddPart(Part, Sort: false);
			}
			break;
		}
		default:
			return false;
		}
		return true;
	}

	public bool HasPart(IConversationPart Part)
	{
		if (Parts == null)
		{
			return false;
		}
		return Parts.Contains(Part);
	}

	public bool HasPart<T>() where T : IConversationPart
	{
		if (Parts == null)
		{
			return false;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if ((object)Parts[i].GetType() == typeof(T))
			{
				return true;
			}
		}
		return false;
	}

	public void AddPart(IConversationPart Part, bool Sort = true)
	{
		if (Parts == null)
		{
			Parts = new List<IConversationPart>();
		}
		Parts.Add(Part);
		Part.ParentElement = this;
		Part.Initialize();
		if (Sort)
		{
			Parts.Sort();
		}
		if (Awoken)
		{
			Part.Awake();
		}
	}

	public T GetPart<T>() where T : IConversationPart
	{
		if (Parts == null)
		{
			return null;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if ((object)Parts[i].GetType() == typeof(T))
			{
				return Parts[i] as T;
			}
		}
		return null;
	}

	public bool TryGetPart<T>(out T Part) where T : IConversationPart
	{
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if ((object)Parts[i].GetType() == typeof(T))
				{
					Part = Parts[i] as T;
					return true;
				}
			}
		}
		Part = null;
		return false;
	}

	public Choice AddChoice(string ID = null, string Text = null, string Target = null)
	{
		Choice choice = new Choice
		{
			ID = (ID ?? default(Guid).ToString()),
			Parent = this,
			Text = Text,
			Target = Target
		};
		if (Elements == null)
		{
			Elements = new List<IConversationElement>();
		}
		Elements.Add(choice);
		return choice;
	}

	public Node AddNode(string ID = null, string Text = null)
	{
		Node node = new Node
		{
			ID = (ID ?? default(Guid).ToString()),
			Parent = this,
			Text = Text
		};
		if (Elements == null)
		{
			Elements = new List<IConversationElement>();
		}
		Elements.Add(node);
		return node;
	}

	public virtual void LoadText(ConversationXMLBlueprint Blueprint)
	{
		if (!string.IsNullOrWhiteSpace(Blueprint.Text))
		{
			Text = Blueprint.Text;
		}
	}

	public virtual void Load(ConversationXMLBlueprint Blueprint)
	{
		ID = Blueprint.CardinalID;
		LoadAttributes(Blueprint.Attributes);
		LoadText(Blueprint);
		if (Blueprint.Children == null)
		{
			return;
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
		Parts?.Sort();
	}

	public int CompareTo(IConversationElement Other)
	{
		return Other?.Priority.CompareTo(Priority) ?? (-1);
	}

	public virtual void Dispose()
	{
		if (Parts != null)
		{
			foreach (IConversationPart part in Parts)
			{
				part.Dispose();
			}
		}
		if (Elements != null)
		{
			foreach (IConversationElement element in Elements)
			{
				element.Dispose();
			}
		}
		if (Texts == null)
		{
			return;
		}
		foreach (ConversationText text in Texts)
		{
			text.Dispose();
		}
	}
}
