using UnityEngine;

public class CombatJuiceEntryPrefabAnimation : CombatJuiceEntry
{
	public Vector3 location;

	public string animation;

	public string objectId;

	public string configurationString;

	public object configurationObject;

	public CombatJuiceEntryPrefabAnimation(Vector3 location, string animation)
	{
		this.location = location;
		this.animation = animation;
	}

	public CombatJuiceEntryPrefabAnimation(Vector3 location, string animation, string objectId)
	{
		this.location = location;
		this.animation = animation;
		this.objectId = objectId;
	}

	public CombatJuiceEntryPrefabAnimation(Vector3 location, string animation, string objectId, string configurationString, object configurationObject)
	{
		this.location = location;
		this.animation = animation;
		this.objectId = objectId;
		this.configurationString = configurationString;
		this.configurationObject = configurationObject;
	}

	public override void start()
	{
		CombatJuice._playPrefabAnimation(location, animation, objectId, configurationString, configurationObject);
	}
}
