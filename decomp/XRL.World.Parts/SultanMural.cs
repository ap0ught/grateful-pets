using System;
using HistoryKit;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class SultanMural : IPart
{
	public long MuralEventID = -1L;

	public string secretID;

	[NonSerialized]
	private HistoricEvent _secretEvent;

	public HistoricEvent secretEvent
	{
		get
		{
			if (_secretEvent == null && MuralEventID != -1)
			{
				_secretEvent = The.Game.sultanHistory.GetEvent(MuralEventID);
			}
			return _secretEvent;
		}
		set
		{
			_secretEvent = value;
			MuralEventID = value?.id ?? (-1);
		}
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.WriteOptimized(MuralEventID);
		Writer.WriteOptimized(secretID);
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		MuralEventID = Reader.ReadOptimizedInt64();
		secretID = Reader.ReadOptimizedString();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AutoexploreObjectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if (!E.AutogetOnlyMode && E.Command != "Look" && HasUnrevealedSecret())
		{
			E.Command = "Look";
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterLookedAt");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterLookedAt" && !string.IsNullOrEmpty(secretID))
		{
			JournalAPI.RevealSultanEventBySecretID(secretID, ParentObject.DisplayName);
		}
		return true;
	}

	public bool HasUnrevealedSecret()
	{
		if (secretID != null)
		{
			return JournalAPI.HasUnrevealedSultanEvent(secretID);
		}
		return false;
	}
}
