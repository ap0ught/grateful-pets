using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class MessageWhenFirstSeen : IPart
{
	public string Message = "[unset]";

	public bool Seen;

	public string Mode = "Log";

	public override bool SameAs(IPart p)
	{
		MessageWhenFirstSeen messageWhenFirstSeen = p as MessageWhenFirstSeen;
		if (messageWhenFirstSeen.Message != Message)
		{
			return false;
		}
		if (messageWhenFirstSeen.Mode != Mode)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool Render(RenderEvent E)
	{
		if (!Seen)
		{
			Seen = true;
			string message = GameText.VariableReplace(Message, ParentObject, null, StripColors: true);
			if (Mode == "Popup")
			{
				Popup.ShowSpace(message);
			}
			if (Mode == "Log")
			{
				IComponent<GameObject>.AddPlayerMessage(message);
			}
		}
		return base.Render(E);
	}
}
