using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class HexacherubimSpawner : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeObjectCreatedEvent E)
	{
		Random seededRandomGenerator = Stat.GetSeededRandomGenerator("SheshCherubim");
		List<string> list = new List<string>(CherubimSpawner.Elements);
		GameObject gameObject = GameObject.Create(CherubimSpawner.Factions.GetRandomElement(seededRandomGenerator) + " Cherub");
		string features = "the " + string.Join(", the ", gameObject.GetxTag("TextFragments", "PoeticFeatures").Split(','));
		gameObject.Render.DisplayName = gameObject.Render.DisplayName.Replace("cherub", "hexacherub");
		gameObject.GetStat("Level").BaseValue = 50;
		gameObject.GetStat("Hitpoints").BaseValue = 2000;
		gameObject.RequirePart<HeatSelfOnFreeze>();
		gameObject.RequirePart<ReflectProjectiles>();
		gameObject.SetIntProperty("CherubimLock", 1);
		gameObject.Brain.Wanders = false;
		gameObject.Brain.WandersRandomly = false;
		gameObject.Brain.Allegiance.Clear();
		gameObject.Brain.Allegiance.Add("Cherubim", 100);
		CherubimSpawner.ReplaceDescription(gameObject, CherubimSpawner.BaseDescription, features);
		int i = 0;
		for (int num = Math.Min(list.Count, 6); i < num; i++)
		{
			string element = list.RemoveRandomElement(seededRandomGenerator);
			CherubimSpawner.BestowElement(gameObject, element, PrependName: false);
		}
		gameObject.Render.ColorString = (gameObject.Render.TileColor = "&W");
		gameObject.Render.DetailColor = "m";
		E.ReplacementObject = gameObject;
		return base.HandleEvent(E);
	}
}
