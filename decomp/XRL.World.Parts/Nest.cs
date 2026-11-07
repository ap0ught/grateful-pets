using System;
using XRL.World.AI;

namespace XRL.World.Parts;

[Serializable]
public class Nest : IPart
{
	public int NumberToSpawn = 30;

	public int ChancePerSpawn = 100;

	public float XPFactor = 0.25f;

	public bool CollapseAfterSpawn = true;

	public string TurnsPerSpawn = "15-20";

	public string NumberSpawned = "1";

	public string SpawnMessage = "A giant centipede crawls out of the nest.";

	public string CollapseMessage = "The nest collapses.";

	public string SpawnParticle = "&w.";

	public string BlueprintSpawned = "Giant Centipede";

	public int SpawnCooldown = int.MinValue;

	public override bool WantTurnTick()
	{
		return true;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("VillageInit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "VillageInit")
		{
			CollapseAfterSpawn = false;
		}
		return base.FireEvent(E);
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (NumberToSpawn <= 0)
		{
			if (!CollapseAfterSpawn)
			{
				ParentObject.RemovePart(this);
				return;
			}
			if (Visible())
			{
				IComponent<GameObject>.AddPlayerMessage(CollapseMessage);
			}
			ParentObject.Destroy();
			return;
		}
		if (SpawnCooldown == int.MinValue)
		{
			SpawnCooldown = TurnsPerSpawn.RollCached();
		}
		if (ParentObject.CurrentCell == null)
		{
			return;
		}
		SpawnCooldown--;
		if (SpawnCooldown > 0)
		{
			return;
		}
		int i = 0;
		for (int num = NumberSpawned.RollCached(); i < num; i++)
		{
			Cell randomLocalAdjacentCell = ParentObject.CurrentCell.GetRandomLocalAdjacentCell();
			if (randomLocalAdjacentCell != null && randomLocalAdjacentCell.IsEmpty() && ChancePerSpawn.in100())
			{
				if (NumberToSpawn > 0)
				{
					NumberToSpawn--;
				}
				GameObject gameObject = GameObject.Create(BlueprintSpawned);
				if (gameObject.HasStat("XPValue"))
				{
					gameObject.GetStat("XPValue").BaseValue = (int)Math.Round((float)gameObject.GetStat("XPValue").BaseValue * XPFactor / 5f) * 5;
				}
				gameObject.TakeAllegiance<AllyBirth>(ParentObject);
				gameObject.MakeActive();
				randomLocalAdjacentCell.AddObject(gameObject);
				if (Visible())
				{
					IComponent<GameObject>.AddPlayerMessage(SpawnMessage);
				}
				ParentObject.Slimesplatter(SelfSplatter: false, SpawnParticle);
				SpawnCooldown = TurnsPerSpawn.RollCached();
			}
		}
	}
}
