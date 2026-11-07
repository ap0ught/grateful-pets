using System;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class Chat : IPart
{
	public string Says = "Hi!";

	public bool ShowInShortDescription;

	public Chat()
	{
	}

	public Chat(string Says)
		: this()
	{
		this.Says = Says;
	}

	public Chat(string Says, bool ShowInShortDescription)
		: this(Says)
	{
		this.ShowInShortDescription = ShowInShortDescription;
	}

	public override bool SameAs(IPart p)
	{
		Chat chat = p as Chat;
		if (chat.Says != Says)
		{
			return false;
		}
		if (chat.ShowInShortDescription != ShowInShortDescription)
		{
			return false;
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID)
		{
			if (ID == GetShortDescriptionEvent.ID)
			{
				return ShowInShortDescription;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (!E.Actor.IsPlayerControlled())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			if (!ParentObject.IsPlayerLed())
			{
				PerformChat(E.Actor, Dialog: true);
				return false;
			}
			return base.HandleEvent(E);
		}
		PerformChat(E.Actor, Dialog: false);
		return false;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (ShowInShortDescription)
		{
			string text = GameText.VariableReplace(Says, ParentObject);
			if (!text.IsNullOrEmpty())
			{
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.Append(E.Infix);
				int length = stringBuilder.Length;
				if (text[0] == '*')
				{
					stringBuilder.Append('\n').Append(text.Substring(1));
				}
				else if (text[0] == '@')
				{
					string[] array = text.Substring(1).Split('~');
					foreach (string text2 in array)
					{
						if (text2[0] == '[')
						{
							stringBuilder.Append('\n').Append(ParentObject.It).Append(ParentObject.GetVerb("bear"))
								.Append(' ')
								.Append(text2.Replace("[", "").Replace("]", ""));
							continue;
						}
						stringBuilder.Append('\n').Append(ParentObject.It).Append(ParentObject.GetVerb("say"))
							.Append(", '")
							.Append(text2)
							.Append('\'');
						if (!text2.EndsWith(".") && !text2.EndsWith("!") && !text2.EndsWith("?"))
						{
							stringBuilder.Append('.');
						}
					}
				}
				else
				{
					string text3 = (text.Contains("~") ? text.Split('~').GetRandomElement() : text);
					if (!text3.IsNullOrEmpty())
					{
						if (text3[0] == '[')
						{
							stringBuilder.Append('\n').Append(ParentObject.It).Append(ParentObject.GetVerb("bear"))
								.Append(' ')
								.Append(text3.Replace("[", "").Replace("]", ""));
						}
						else
						{
							stringBuilder.Append('\n').Append(ParentObject.It).Append(ParentObject.GetVerb("read"))
								.Append(", '")
								.Append(text3)
								.Append('\'');
							if (!text3.EndsWith(".") && !text3.EndsWith("!") && !text3.EndsWith("?"))
							{
								stringBuilder.Append('.');
							}
						}
						stringBuilder.Append('\n');
					}
				}
				if (stringBuilder.Length != length)
				{
					E.Infix.Clear().Append(stringBuilder);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ObjectTalking");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ObjectTalking")
		{
			PerformChat(IComponent<GameObject>.ThePlayer, Dialog: true);
			return false;
		}
		return base.FireEvent(E);
	}

	public void PerformChat(GameObject who, bool Dialog)
	{
		string text = GameText.VariableReplace(Says, ParentObject);
		if (text.IsNullOrEmpty() || (ParentObject.Brain != null && !ConversationScript.IsPhysicalConversationPossible(ParentObject, ShowPopup: true)))
		{
			return;
		}
		GameObject gameObject = who ?? ParentObject;
		ParentObject.PlayWorldSoundTag("ChatSound", "sfx_interact_chat", gameObject.CurrentCell);
		if (text[0] == '*')
		{
			IComponent<GameObject>.EmitMessage(gameObject, text.Substring(1), ' ', Dialog);
		}
		else if (text[0] == '@')
		{
			string[] array = text.Substring(1).Split('~');
			foreach (string text2 in array)
			{
				if (text2[0] == '[')
				{
					IComponent<GameObject>.EmitMessage(gameObject, text2.Replace("[", "").Replace("]", ""), ' ', Dialog);
				}
				else
				{
					IComponent<GameObject>.EmitMessage(gameObject, ParentObject.Does("say") + ", '{{|" + text2 + "}}'.", ' ', Dialog);
				}
			}
		}
		else
		{
			string text3 = (text.Contains("~") ? text.Split('~').GetRandomElement() : text);
			if (!text3.IsNullOrEmpty())
			{
				if (text3[0] == '[')
				{
					IComponent<GameObject>.EmitMessage(gameObject, text3.Replace("[", "").Replace("]", ""), ' ', Dialog);
				}
				else
				{
					IComponent<GameObject>.EmitMessage(gameObject, ParentObject.Does("say") + ", '{{|" + text3 + "}}'.", ' ', Dialog);
				}
			}
		}
		if (who.IsPlayer())
		{
			ParentObject.FireEvent("ChattingWithPlayer");
		}
	}

	public void PerformChat(bool Dialog)
	{
		PerformChat(null, Dialog);
	}
}
