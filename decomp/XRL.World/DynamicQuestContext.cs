using System;
using System.Collections.Generic;
using HistoryKit;
using XRL.World.WorldBuilders;

namespace XRL.World;

[Serializable]
public abstract class DynamicQuestContext : IComposite
{
	public int questNumber;

	public int questChainNumber;

	public int tier = 1;

	public abstract HistoricEntity originEntity();

	public abstract string originZoneId();

	public abstract string getQuestOriginZone();

	public abstract string questTargetZone(int minDistance, int maxDistance);

	public abstract GeneratedLocationInfo getNearbyUndiscoveredLocation();

	public abstract DynamicQuestReward getQuestReward();

	public abstract string getQuestGiverZone();

	public abstract Func<GameObject, bool> getQuestGiverFilter();

	public abstract Func<GameObject, bool> getQuestActorFilter();

	public abstract List<string> getSacredThings();

	public abstract DynamicQuestDeliveryTarget getQuestDeliveryTarget();

	public abstract GameObject getQuestRemoteInteractable();

	public abstract GameObject getQuestGenericRemoteInteractable();

	public abstract GameObject getQuestDeliveryItem();

	public abstract string getQuestItemNameMutation(string input);

	public abstract string assassinationTargetId();

	public abstract GameObject getALostItem();

	public virtual void Write(SerializationWriter Writer)
	{
	}

	public virtual void Read(SerializationReader Reader)
	{
	}
}
