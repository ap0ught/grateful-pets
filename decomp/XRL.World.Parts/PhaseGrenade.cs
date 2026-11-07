using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class PhaseGrenade : IGrenade
{
	public int Radius;

	public string Duration;

	public override bool SameAs(IPart p)
	{
		PhaseGrenade phaseGrenade = p as PhaseGrenade;
		if (phaseGrenade.Radius != Radius)
		{
			return false;
		}
		if (phaseGrenade.Duration != Duration)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetComponentNavigationWeightEvent.ID)
		{
			return ID == GetComponentAdjacentNavigationWeightEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetComponentNavigationWeightEvent E)
	{
		E.MinWeight(3);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetComponentAdjacentNavigationWeightEvent E)
	{
		E.MinWeight(2);
		return base.HandleEvent(E);
	}

	protected override bool DoDetonate(Cell C, GameObject Actor = null, GameObject ApparentTarget = null, bool Indirect = false)
	{
		if (!IComponent<GameObject>.CheckRealityDistortionUsability(ParentObject, null, null, ParentObject))
		{
			return false;
		}
		DidX("explode", null, "!");
		List<Cell> list = new List<Cell>((Radius + 2) * (Radius + 2));
		C.GetAdjacentCells(Radius, list, LocalOnly: false);
		int phase = ParentObject.GetPhase();
		List<GameObject> list2 = Event.NewGameObjectList();
		foreach (Cell item in list)
		{
			if (!IComponent<GameObject>.CheckRealityDistortionAccessibility(null, item, null, ParentObject))
			{
				continue;
			}
			list2.Clear();
			list2.AddRange(item.Objects);
			foreach (GameObject item2 in list2)
			{
				if (item2.HasEffect<Phased>())
				{
					item2.RemoveAllEffects<Phased>();
				}
				else
				{
					item2.ForceApplyEffect(new Phased(Duration.RollCached()));
				}
			}
			if (item.ParentZone.IsActive())
			{
				switch (phase)
				{
				case 1:
					The.ParticleManager.Add("&M" + (char)Stat.RandomCosmetic(191, 198), item.X, item.Y, 0f, 0f, Stat.Random(4, 9), 0f, 0f, 0L);
					The.ParticleManager.Add("&m" + (char)Stat.RandomCosmetic(191, 198), item.X, item.Y, 0f, 0f, Stat.Random(2, 6), 0f, 0f, 0L);
					The.ParticleManager.Add("&K" + (char)Stat.RandomCosmetic(191, 198), item.X, item.Y, 0f, 0f, Stat.Random(0, 3), 0f, 0f, 0L);
					break;
				case 2:
					The.ParticleManager.Add("&B" + (char)Stat.RandomCosmetic(191, 198), item.X, item.Y, 0f, 0f, Stat.Random(4, 9), 0f, 0f, 0L);
					The.ParticleManager.Add("&Y" + (char)Stat.RandomCosmetic(191, 198), item.X, item.Y, 0f, 0f, Stat.Random(2, 6), 0f, 0f, 0L);
					The.ParticleManager.Add("&b" + (char)Stat.RandomCosmetic(191, 198), item.X, item.Y, 0f, 0f, Stat.Random(0, 3), 0f, 0f, 0L);
					break;
				case 3:
					The.ParticleManager.Add("&G" + (char)Stat.RandomCosmetic(191, 198), item.X, item.Y, 0f, 0f, Stat.Random(4, 9), 0f, 0f, 0L);
					The.ParticleManager.Add("&c" + (char)Stat.RandomCosmetic(191, 198), item.X, item.Y, 0f, 0f, Stat.Random(2, 6), 0f, 0f, 0L);
					The.ParticleManager.Add("&M" + (char)Stat.RandomCosmetic(191, 198), item.X, item.Y, 0f, 0f, Stat.Random(0, 3), 0f, 0f, 0L);
					break;
				case 4:
					The.ParticleManager.Add("&K" + (char)Stat.RandomCosmetic(191, 198), item.X, item.Y, 0f, 0f, Stat.Random(4, 9), 0f, 0f, 0L);
					The.ParticleManager.Add("&y" + (char)Stat.RandomCosmetic(191, 198), item.X, item.Y, 0f, 0f, Stat.Random(2, 6), 0f, 0f, 0L);
					The.ParticleManager.Add("&b" + (char)Stat.RandomCosmetic(191, 198), item.X, item.Y, 0f, 0f, Stat.Random(0, 3), 0f, 0f, 0L);
					break;
				}
			}
		}
		ParentObject.Destroy(null, Silent: true);
		return true;
	}
}
