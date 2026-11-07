using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace XRL.World.Conversations;

public class BuildContext
{
	public class DiagnosticInfo
	{
		public string URI;

		public int LineNumber;

		public ModInfo Mod;
	}

	public Dictionary<string, ConversationXMLBlueprint> Finalized = new Dictionary<string, ConversationXMLBlueprint>();

	public Dictionary<ConversationXMLBlueprint, DiagnosticInfo> Diagnostics = new Dictionary<ConversationXMLBlueprint, DiagnosticInfo>();

	public List<ConversationXMLBlueprint> Current = new List<ConversationXMLBlueprint>();

	public List<ConversationXMLBlueprint> Next = new List<ConversationXMLBlueprint>();

	public List<string> Errors = new List<string>();

	public Stack<ConversationXMLBlueprint> Lineage = new Stack<ConversationXMLBlueprint>();

	public StringBuilder Text = new StringBuilder();

	public DataFile File;

	public ModInfo Mod;

	public string Namespace;

	public void Advance()
	{
		List<ConversationXMLBlueprint> current = Current;
		Current = Next;
		Next = current;
		Next.Clear();
		Errors.Clear();
	}

	public void Clear()
	{
		Current.Clear();
		Next.Clear();
		Lineage.Clear();
	}

	public void Push(ConversationXMLBlueprint Blueprint)
	{
		Lineage.Push(Blueprint);
	}

	public ConversationXMLBlueprint Pop()
	{
		return Lineage.Pop();
	}

	public ConversationXMLBlueprint Peek()
	{
		return Lineage.Peek();
	}

	public void Record(ConversationXMLBlueprint Blueprint, XmlTextReader Reader)
	{
	}

	public bool Missing(ConversationXMLBlueprint Blueprint, string ID)
	{
		if (Diagnostics.TryGetValue(Blueprint, out var value))
		{
			Text.Clear().Append(value.URI).Append(" line ")
				.Append(value.LineNumber)
				.Append(": No finalized element by ID '")
				.Append(ID)
				.Append("' could be found to inherit.");
			Errors.Add(Text.ToString());
		}
		else
		{
			if (Lineage.Contains(Blueprint))
			{
				AssemblePathID();
			}
			else
			{
				AssemblePathID(Blueprint);
			}
			Text.Append(": No finalized element by ID '").Append(ID).Append("' could be found to inherit.");
			Errors.Add(Text.ToString());
		}
		return false;
	}

	public string AssemblePathID(string ID, int Skip = 0)
	{
		Text.Clear().Append(ID);
		foreach (ConversationXMLBlueprint item in Lineage)
		{
			if (Skip-- <= 0)
			{
				Text.Insert(0, '.');
				if (item.Cardinal > 1)
				{
					Text.Insert(0, item.Cardinal);
				}
				Text.Insert(0, item.ID);
			}
		}
		return Text.ToString();
	}

	public string AssemblePathID(ConversationXMLBlueprint Blueprint)
	{
		Text.Clear().Append(Blueprint.ID);
		if (Blueprint.Cardinal > 1)
		{
			Text.Append(Blueprint.Cardinal);
		}
		foreach (ConversationXMLBlueprint item in Lineage)
		{
			Text.Insert(0, '.');
			if (item.Cardinal > 1)
			{
				Text.Insert(0, item.Cardinal);
			}
			Text.Insert(0, item.ID);
		}
		return Text.ToString();
	}

	public string AssemblePathID()
	{
		Text.Clear();
		foreach (ConversationXMLBlueprint item in Lineage)
		{
			if (Text.Length != 0)
			{
				Text.Insert(0, '.');
			}
			if (item.Cardinal > 1)
			{
				Text.Insert(0, item.Cardinal);
			}
			Text.Insert(0, item.ID);
		}
		return Text.ToString();
	}

	public string AssembleNamedID(ConversationXMLBlueprint Blueprint, string Of = null)
	{
		Text.Clear().Append(Blueprint.Name).Append('.')
			.Append(Of ?? Blueprint.ID);
		ConversationXMLBlueprint conversationXMLBlueprint = Lineage.LastOrDefault();
		if (conversationXMLBlueprint != null && conversationXMLBlueprint != Blueprint)
		{
			Text.Insert(0, '.');
			Text.Insert(0, conversationXMLBlueprint.ID);
		}
		return Text.ToString();
	}

	public ConversationXMLBlueprint Parent(ConversationXMLBlueprint Blueprint = null)
	{
		bool flag = false;
		foreach (ConversationXMLBlueprint item in Lineage)
		{
			if (flag)
			{
				return item;
			}
			flag = Blueprint == item || Blueprint == null;
		}
		return null;
	}
}
