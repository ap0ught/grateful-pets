using System;
using System.Collections.Generic;
using HistoryKit;
using Newtonsoft.Json.Linq;

namespace XRL.World.Conversations.Parts;

public class SpiceContext : IConversationPart
{
	public Dictionary<string, string> Variables = new Dictionary<string, string>();

	public Dictionary<string, JToken> Nodes = new Dictionary<string, JToken>();

	public SpiceContext()
	{
		Priority = -1000;
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation))
		{
			return ID == PrepareTextEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(PrepareTextEvent E)
	{
		for (int num = E.Text.IndexOf("=spice"); num >= 0; num = E.Text.IndexOf("=spice"))
		{
			int num2 = E.Text.IndexOf('=', num + 5);
			int num3 = E.Text.IndexOf(".store");
			if (num3 < num2)
			{
				E.Text.Remove(num3, 6);
				num2 -= 6;
			}
			while (true)
			{
				Random r = new Random(The.Speaker.ID.GetHashCode());
				string text = HistoricStringExpander.ExpandQuery(E.Text.ToString(num + 1, num2 - num - 1), null, null, Variables, Nodes, r);
				E.Text.Remove(num, num2 - num + 1);
				E.Text.Insert(num, text);
				int num4 = text.IndexOf('<');
				if (num4 < 0)
				{
					break;
				}
				num2 = num + text.IndexOf('>', num4 + 1);
				num += num4;
			}
		}
		return base.HandleEvent(E);
	}
}
