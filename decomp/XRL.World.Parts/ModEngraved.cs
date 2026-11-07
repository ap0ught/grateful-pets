using System;
using System.Collections.Generic;
using System.Text;
using HistoryKit;
using Qud.API;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class ModEngraved : IModification
{
	public long EngravedEventID = -1L;

	public bool LookedAt;

	public string Engraving;

	public string Sultan = "";

	[NonSerialized]
	private HistoricEvent _EngravedEvent;

	public HistoricEvent EngravedEvent
	{
		get
		{
			if (_EngravedEvent == null && EngravedEventID != -1)
			{
				_EngravedEvent = The.Game.sultanHistory.GetEvent(EngravedEventID);
			}
			return _EngravedEvent;
		}
		set
		{
			_EngravedEvent = value;
			EngravedEventID = value?.id ?? (-1);
		}
	}

	public ModEngraved()
	{
	}

	public ModEngraved(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.ModIntProperty("NoCostMods", 1);
	}

	public override void Remove()
	{
		ParentObject.ModIntProperty("NoCostMods", 1, RemoveIfZero: true);
		base.Remove();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AutoexploreObjectEvent.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetShortDescriptionEvent.ID && ID != GetUnknownShortDescriptionEvent.ID)
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if (!E.AutogetOnlyMode && E.Command != "Look" && !LookedAt && HasUnrevealedSecret())
		{
			E.Command = "Look";
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.Understood() || !E.Object.HasProperName)
		{
			E.AddAdjective("{{engraved|engraved}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		AddEngraving(E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetUnknownShortDescriptionEvent E)
	{
		AddEngraving(E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		GenerateEngraving();
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterLookedAt");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterLookedAt" && !LookedAt)
		{
			LookedAt = true;
			EngravedEvent?.Reveal(ParentObject.DisplayName);
			if (HasUnrevealedSecret())
			{
				MetricsManager.LogError("after reveal still had unrevealed secret from " + EngravedEvent?.GetUnrevealedSecretSource());
			}
		}
		return base.FireEvent(E);
	}

	public bool HasUnrevealedSecret()
	{
		if (EngravedEvent != null)
		{
			return EngravedEvent.HasUnrevealedSecret();
		}
		return false;
	}

	private void GenerateEngraving()
	{
		if (Engraving != null)
		{
			return;
		}
		History history = The.Game?.sultanHistory;
		if (history == null)
		{
			return;
		}
		HistoricEntity randomElement = history.GetEntitiesWherePropertyEquals("type", "sultan").GetRandomElement(Stat.Rand);
		Sultan = randomElement.GetCurrentSnapshot().GetProperty("name");
		List<HistoricEvent> list = new List<HistoricEvent>();
		for (int i = 0; i < randomElement.events.Count; i++)
		{
			if (randomElement.events[i].HasEventProperty("gospel"))
			{
				list.Add(randomElement.events[i]);
			}
		}
		if (list.Count > 0)
		{
			EngravedEvent = list.GetRandomElement();
			Engraving = EngravedEvent.GetEventProperty("gospel");
		}
		else
		{
			Engraving = "<marred and unreadable>";
		}
		string propertyOrTag = ParentObject.GetPropertyOrTag("Mods");
		if (propertyOrTag != null && !propertyOrTag.Contains("PotteryMods"))
		{
			string property = randomElement.GetCurrentSnapshot().GetProperty("period", "0");
			StringBuilder stringBuilder = Event.NewStringBuilder();
			stringBuilder.Append(Faction.GetSultanFactionName(property)).Append(":").Append(Stat.Random(8, 12) * 5);
			AddsRep.AddModifier(ParentObject, stringBuilder.ToString());
		}
	}

	private void AddEngraving(IShortDescriptionEvent E)
	{
		if (EngravedEvent == null)
		{
			GenerateEngraving();
		}
		E.Postfix.Append("\n{{cyan|Engraved: This item is engraved with a scene from the life of the ancient ").Append(HistoryAPI.GetSultanTerm()).Append(" {{magenta|")
			.Append(Sultan)
			.Append("}}:\n\n")
			.Append(Engraving.Coalesce("<marred and unreadable>"))
			.Append("}}\n");
	}
}
