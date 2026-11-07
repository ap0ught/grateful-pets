using System;
using System.Collections.Generic;
using HistoryKit;
using Qud.API;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
public class CultistSpawner : IPart
{
	public int Tier = 1;

	public int Period = 5;

	public string CultistType = "Tomb Cultist";

	public bool bTierExact;

	public int TierBumpChance = 15;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	private GameObject GetLikedFactionMember(HistoricEntitySnapshot SultanSnapshot, Predicate<GameObjectBlueprint> filter)
	{
		List<string> list = SultanSnapshot.GetList("likedFactions");
		GameObject gameObject = null;
		list.Shuffle();
		foreach (string item in list)
		{
			gameObject = EncountersAPI.GetACreatureFromFaction(item, filter);
			if (gameObject != null)
			{
				break;
			}
		}
		return gameObject;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell" && CultistType == "Tomb Cultist")
		{
			Cell cell = ParentObject.CurrentCell;
			HistoricEntitySnapshot currentSnapshot = XRLCore.Core.Game.sultanHistory.GetEntitiesByDelegate((HistoricEntity e) =>
			{
				HistoricEntitySnapshot currentSnapshot2 = e.GetCurrentSnapshot();
				return currentSnapshot2.GetProperty("type") == "sultan" && int.Parse(currentSnapshot2.GetProperty("period")) == Period;
			}).GetRandomElement().GetCurrentSnapshot();
			string cultFaction = ((Period == 6) ? "Resheph" : ("SultanCult" + Period));
			if (!bTierExact && If.Chance(TierBumpChance))
			{
				Tier = (If.CoinFlip() ? (Tier - 1) : (Tier + 1));
			}
			GameObject gameObject = GetLikedFactionMember(currentSnapshot, (GameObjectBlueprint c) => c.Tier == Tier && !c.HasTag("Merchant"));
			if (gameObject == null)
			{
				gameObject = EncountersAPI.GetACreature((GameObjectBlueprint c) => c.Tier == Tier && !c.HasTag("Merchant"));
			}
			if (gameObject == null)
			{
				ParentObject.Destroy();
				return false;
			}
			CultistTemplate.Apply(gameObject, cultFaction);
			TombCultistTemplate.Apply(gameObject, currentSnapshot);
			gameObject.MakeActive();
			cell.AddObject(gameObject);
			ParentObject.Destroy();
		}
		return base.FireEvent(E);
	}
}
