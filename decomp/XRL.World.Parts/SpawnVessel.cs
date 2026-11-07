using System;
using XRL.World.AI;

namespace XRL.World.Parts;

[Serializable]
public class SpawnVessel : IPart
{
	public int SpawnTurns = int.MinValue;

	public string SpawnBlueprint;

	public string ReplaceBlueprint;

	public string SpawnMessage;

	public string SpawnVerb;

	public string SpawnAmount = "1";

	public string SpawnTime = "10";

	public string SpawnSound;

	public GameObject SpawnedBy;

	public bool AdjustAttitude;

	public bool SlimesplatterOnSpawn;

	public bool SpawnOnEmpty;

	public int SpawnChance = 100;

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell != null)
		{
			Spawn(cell);
		}
	}

	public void Spawn(Cell Cell)
	{
		if (SpawnTurns == int.MinValue)
		{
			SpawnTurns = SpawnTime.RollCached();
		}
		SpawnTurns--;
		if (SpawnTurns > 0)
		{
			return;
		}
		if (!SpawnSound.IsNullOrEmpty())
		{
			Cell.PlayWorldSound(SpawnSound);
		}
		if (!SpawnMessage.IsNullOrEmpty())
		{
			EmitMessage(GameText.VariableReplace(SpawnMessage, ParentObject, SpawnedBy));
		}
		if (!SpawnVerb.IsNullOrEmpty())
		{
			DidX(SpawnVerb);
		}
		if (!ReplaceBlueprint.IsNullOrEmpty())
		{
			Cell.AddObject(ReplaceBlueprint);
		}
		if (SlimesplatterOnSpawn)
		{
			ParentObject.Slimesplatter(SelfSplatter: false);
		}
		ParentObject.Destroy();
		int num = SpawnAmount.RollCached();
		for (int i = 0; i < num; i++)
		{
			if (SpawnChance.in100())
			{
				GameObject gameObject = GameObject.Create(SpawnBlueprint);
				if (AdjustAttitude && GameObject.Validate(ref SpawnedBy))
				{
					gameObject.TakeAllegiance<AllyBirth>(SpawnedBy);
				}
				Cell connectedSpawnLocation = Cell.GetConnectedSpawnLocation();
				if (connectedSpawnLocation == null)
				{
					break;
				}
				connectedSpawnLocation.AddObject(gameObject);
				if (gameObject.IsValid())
				{
					gameObject.PlayWorldSoundTag("AmbientIdleSound");
					gameObject.MakeActive();
				}
			}
		}
	}
}
