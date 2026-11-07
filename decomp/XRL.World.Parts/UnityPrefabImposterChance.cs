using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class UnityPrefabImposterChance : IPart
{
	public bool VisibleOnly = true;

	public int Chance;

	public string PrefabID;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			if (Stat.RandomCosmetic(1, 100) <= Chance)
			{
				BasePrefabImposter basePrefabImposter = (VisibleOnly ? ((BasePrefabImposter)new PrefabImposter()) : ((BasePrefabImposter)new PrefabImposterFinal()));
				basePrefabImposter.Prefab = PrefabID;
				ParentObject.AddPart(basePrefabImposter);
			}
			ParentObject.RemovePart(this);
		}
		return true;
	}
}
