using System;

namespace XRL.World.Parts;

[Serializable]
public class SoupSludgeSpawner : IPart
{
	public string Rank = "1d20";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade))
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		int num = Math.Min(Rank.RollCached(), LiquidVolume.Liquids.Count);
		GameObject gameObject = GameObjectFactory.Factory.CreateObject("SoupSludge");
		gameObject.SetStringProperty("SpawnedFrom", ParentObject.Blueprint);
		int num2 = 0;
		SoupSludge soupSludge = gameObject.AddPart(new SoupSludge());
		while (soupSludge.ComponentLiquids.Count < num && num2++ < 100)
		{
			string blueprint = PopulationManager.GenerateOne("RandomLiquidWithRaresNoLava").Blueprint;
			if (!soupSludge.ComponentLiquids.Contains(blueprint))
			{
				soupSludge.CatalyzeLiquid(blueprint);
				num2 = 0;
			}
		}
		soupSludge.CatalyzeName();
		E.ReplacementObject = gameObject;
		return base.HandleEvent(E);
	}
}
