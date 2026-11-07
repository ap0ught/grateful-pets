using System;
using System.Collections.Generic;
using System.Linq;
using Qud.API;
using XRL.World.AI.GoalHandlers;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class AIPilgrim : AIBehaviorPart
{
	public bool FoundTarget;

	public int StiltWx = 5;

	public int StiltWy = 2;

	public int StiltXx = 1;

	public int StiltYx = 1;

	public int StiltZx = 10;

	public string StiltZoneID = "JoppaWorld.5.2.1.1.10";

	public string StiltEntranceZoneID = "JoppaWorld.5.2.1.2.10";

	public string TargetObject = "StiltWell";

	public string MapNoteAttributes;

	public int Chance = 100;

	public bool Ignore;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<AIBoredEvent>.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		if (CheckStartPilgrimage())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("travel", 1);
		}
		return base.HandleEvent(E);
	}

	public override void Initialize()
	{
		if (!Chance.in100())
		{
			Ignore = true;
		}
		base.Initialize();
	}

	public bool CheckStartPilgrimage()
	{
		if (Ignore)
		{
			return false;
		}
		if (!ParentObject.FireEvent("CanAIDoIndependentBehavior"))
		{
			return false;
		}
		if (ParentObject.PartyLeader != null)
		{
			return false;
		}
		if (ParentObject.HasTagOrProperty("ExcludeFromDynamicEncounters"))
		{
			return false;
		}
		if (FoundTarget)
		{
			return false;
		}
		if (ParentObject.HasIntProperty("LairOwner"))
		{
			return false;
		}
		if (ParentObject.HasEffect<Lost>() || (ParentObject.HasEffect<XRL.World.Effects.Confused>() && !ParentObject.HasEffect<FuriouslyConfused>()))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(MapNoteAttributes))
		{
			if (MapNoteAttributes == "invalid-tag")
			{
				return false;
			}
			if (ParentObject.CurrentZone == null)
			{
				return false;
			}
			IEnumerable<JournalMapNote> mapNotesWithAllAttributes = JournalAPI.GetMapNotesWithAllAttributes(MapNoteAttributes);
			MapNoteAttributes = "invalid-tag";
			if (mapNotesWithAllAttributes.Count() == 0)
			{
				return false;
			}
			JournalMapNote randomElement = mapNotesWithAllAttributes.ToList().GetRandomElement();
			StiltWx = randomElement.ParasangX;
			StiltWy = randomElement.ParasangY;
			StiltXx = randomElement.ZoneX;
			StiltYx = randomElement.ZoneY;
			StiltZx = randomElement.ZoneZ;
			StiltZoneID = randomElement.ZoneID;
			StiltEntranceZoneID = randomElement.ZoneID;
		}
		ParentObject.Brain.PushGoal(new GoOnAPilgrimage(StiltWx, StiltWy, StiltXx, StiltYx, StiltZx, TargetObject, StiltZoneID, StiltEntranceZoneID));
		return true;
	}
}
