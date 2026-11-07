using XRL.World.Conversations;

namespace XRL.World;

[GameEvent(Base = true, Cascade = 17)]
public abstract class IConversationMinEvent : MinEvent
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject SpeakingWith;

	public GameObject Transmitter;

	public GameObject Receiver;

	public Conversation Conversation;

	public bool CanTrade;

	public bool Physical;

	public bool Mental;

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		SpeakingWith = null;
		Transmitter = null;
		Receiver = null;
		Conversation = null;
		CanTrade = false;
		Physical = false;
		Mental = false;
	}

	public bool IsParticipant(GameObject Object)
	{
		if (Object != null)
		{
			if (Object != Actor)
			{
				return Object == SpeakingWith;
			}
			return true;
		}
		return false;
	}

	public bool IsDevice(GameObject Object)
	{
		if (Object != null)
		{
			if (Object != Transmitter)
			{
				return Object == Receiver;
			}
			return true;
		}
		return false;
	}

	public static bool DispatchAll<T>(T E) where T : IConversationMinEvent
	{
		GameObject actor = E.Actor;
		GameObject speakingWith = E.SpeakingWith;
		GameObject transmitter = E.Transmitter;
		GameObject receiver = E.Receiver;
		XRLGame game = The.Game;
		if ((game == null || game.HandleEvent(E)) && (actor == null || actor.HandleEvent(E)) && (speakingWith == null || speakingWith.HandleEvent(E)) && (transmitter == null || transmitter.HandleEvent(E)))
		{
			return receiver?.HandleEvent(E) ?? true;
		}
		return false;
	}
}
