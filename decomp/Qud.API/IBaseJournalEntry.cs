using System;
using System.Collections.Generic;
using System.Text;
using XRL.Messages;
using XRL.UI;
using XRL.World;

namespace Qud.API;

[Serializable]
public class IBaseJournalEntry : IComposite
{
	public const int BASE_WEIGHT = 100;

	protected static StringBuilder SB = new StringBuilder();

	public string ID;

	public string History = "";

	public string Text;

	public string LearnedFrom;

	public int Weight = 100;

	public bool Revealed;

	public bool Tradable = true;

	public List<string> Attributes = new List<string>();

	[NonSerialized]
	public static bool NotedPrompt = true;

	public virtual bool WantFieldReflection => false;

	[Obsolete("Use Revealed")]
	public bool revealed
	{
		get
		{
			return Revealed;
		}
		set
		{
			Revealed = value;
		}
	}

	[Obsolete("Use !Tradable")]
	public bool secretSold
	{
		get
		{
			return !Tradable;
		}
		set
		{
			Tradable = !value;
		}
	}

	[Obsolete("Use ID")]
	public string secretid
	{
		get
		{
			return ID;
		}
		set
		{
			ID = value;
		}
	}

	[Obsolete("Use History")]
	public string history
	{
		get
		{
			return History;
		}
		set
		{
			History = value;
		}
	}

	[Obsolete("Use Text")]
	public string text
	{
		get
		{
			return Text;
		}
		set
		{
			Text = value;
		}
	}

	[Obsolete("Use Attributes")]
	public List<string> attributes
	{
		get
		{
			return Attributes;
		}
		set
		{
			Attributes = value;
		}
	}

	public bool Has(string att)
	{
		return Attributes.Contains(att);
	}

	public bool TryGetAttribute(string Prefix, out string Value)
	{
		foreach (string attribute in Attributes)
		{
			if (attribute.StartsWith(Prefix))
			{
				Value = attribute.Substring(Prefix.Length);
				return true;
			}
		}
		Value = null;
		return false;
	}

	public virtual string GetShortText()
	{
		return Text;
	}

	public virtual string GetDisplayText()
	{
		if (History.Length > 0)
		{
			return Text + "\n" + History;
		}
		return Text;
	}

	public virtual string GetShareText()
	{
		return GetShortText();
	}

	public virtual void AppendHistory(string Line)
	{
		if (History.Length > 0)
		{
			History += "\n";
		}
		History += Line;
	}

	public virtual void Updated()
	{
	}

	public virtual void Forget(bool fast = false)
	{
		if (Revealed && Forgettable())
		{
			Revealed = false;
			SecretVisibilityChangedEvent.Send(this);
			Updated();
		}
	}

	public virtual void Reveal(string LearnedFrom = null, bool Silent = false)
	{
		if (!Revealed)
		{
			this.LearnedFrom = LearnedFrom;
			Revealed = true;
			SecretVisibilityChangedEvent.Send(this);
			Updated();
		}
	}

	public virtual bool Forgettable()
	{
		return true;
	}

	public virtual bool CanSell()
	{
		if (Tradable)
		{
			return Revealed;
		}
		return false;
	}

	public virtual bool CanBuy()
	{
		if (Tradable)
		{
			return !Revealed;
		}
		return false;
	}

	public virtual void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(ID);
		Writer.WriteOptimized(History);
		Writer.WriteOptimized(Text);
		Writer.WriteOptimized(LearnedFrom);
		Writer.WriteOptimized(Weight);
		Writer.Write(Revealed);
		Writer.Write(Tradable);
		Writer.Write(Attributes);
	}

	public virtual void Read(SerializationReader Reader)
	{
		ID = Reader.ReadOptimizedString();
		History = Reader.ReadOptimizedString();
		Text = Reader.ReadOptimizedString();
		LearnedFrom = Reader.ReadOptimizedString();
		Weight = Reader.ReadOptimizedInt32();
		Revealed = Reader.ReadBoolean();
		Tradable = Reader.ReadBoolean();
		Attributes = Reader.ReadList<string>();
	}

	public static void DisplayMessage(string Message, string Sound = null)
	{
		if (NotedPrompt)
		{
			Popup.Show(Message, null, Sound);
			return;
		}
		SoundManager.PlayUISound(Sound);
		MessageQueue.AddPlayerMessage(Message);
	}
}
