using System;
using System.Collections.Generic;
using System.Text;
using Wintellect.PowerCollections;
using XRL.World.Anatomy;

namespace XRL.World.Units;

[Serializable]
public class GameObjectCyberneticsUnit : GameObjectUnit
{
	public string Blueprint;

	public string Slot;

	public string LicenseStat;

	public bool Removable = true;

	[NonSerialized]
	private GameObjectBlueprint _Entry;

	public GameObjectBlueprint Entry => _Entry ?? (_Entry = (GameObjectFactory.Factory.Blueprints.TryGetValue(Blueprint, out _Entry) ? _Entry : null));

	public override void Apply(GameObject Object)
	{
		Implant(Object);
	}

	public GameObject Implant(GameObject Object)
	{
		BodyPart bodyPart = null;
		if (!Slot.IsNullOrEmpty())
		{
			bodyPart = Object.Body.GetPartByName(Slot);
		}
		if (bodyPart == null)
		{
			string[] array = Entry?.GetPartParameter<string>("CyberneticsBaseItem", "Slots").Split(',');
			if (!array.IsNullOrEmpty())
			{
				List<BodyPart> list = new List<BodyPart>();
				string[] preferred = Entry.GetTag("CyberneticsPreferredSlots")?.Split(',');
				BodyPart body = Object.Body.GetBody();
				Algorithms.RandomShuffleInPlace(array);
				if (!preferred.IsNullOrEmpty())
				{
					Algorithms.StableSortInPlace(array, (string a, string b) => Array.IndexOf(preferred, b).CompareTo(Array.IndexOf(preferred, a)));
				}
				string[] array2 = array;
				foreach (string requiredType in array2)
				{
					body.GetPart(requiredType, list);
					list.RemoveAll((BodyPart x) => x.Cybernetics != null);
					bodyPart = list.GetRandomElement();
					if (bodyPart != null)
					{
						break;
					}
				}
			}
		}
		if (bodyPart == null)
		{
			MetricsManager.LogError($"Unable to implant {Object} with {Blueprint}.");
			return null;
		}
		GameObject gameObject = GameObjectFactory.Factory.CreateObject(Entry);
		if (!Removable)
		{
			gameObject.SetIntProperty("CyberneticsNoRemove", 1);
		}
		if (Object.IsPlayer())
		{
			gameObject.MakeUnderstood();
		}
		bodyPart.Implant(gameObject);
		_ = Entry?.GetPartParameter("CyberneticsBaseItem", "Cost", 0) ?? 0;
		_ = 0;
		return gameObject;
	}

	public override void Remove(GameObject Object)
	{
		foreach (BodyPart item in Object.Body.LoopParts())
		{
			if (item.Cybernetics?.Blueprint == Blueprint)
			{
				item.Unimplant(MoveToInventory: false);
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		Blueprint = null;
		Slot = null;
		_Entry = null;
		Removable = true;
	}

	public override bool CanInscribe()
	{
		return true;
	}

	public override string GetDescription(bool Inscription = false)
	{
		if (LicenseStat.IsNullOrEmpty())
		{
			if (!Inscription)
			{
				return "Cybernetic implant installed";
			}
			return "";
		}
		StringBuilder sB = Event.NewStringBuilder(Inscription ? "" : "Cybernetic implant installed");
		_ = Entry?.GetPartParameter("CyberneticsBaseItem", "Cost", 0) ?? 0;
		_ = 0;
		return Event.FinalizeString(sB);
	}
}
