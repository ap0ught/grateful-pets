using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Language;
using XRL.Rules;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class StoneGaze : IDelayedLineMutation
{
	[NonSerialized]
	private static GameObject _Projectile;

	public override string Command => "CommandStoneGaze";

	public override bool CanRefract => true;

	public override GameObject Projectile
	{
		get
		{
			if (!GameObject.Validate(ref _Projectile))
			{
				_Projectile = GameObject.CreateUnmodified("ProjectileStoneGaze");
			}
			return _Projectile;
		}
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("Range", GetRange(Level), !stats.mode.Contains("ability"));
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
	}

	public static GameObject CreateStatueOf(GameObject source)
	{
		GameObject gameObject = GameObject.Create("Random Stone Statue");
		gameObject.RequirePart<RandomStatue>().SetCreature(source.DeepCopy());
		if (Stat.Random(1, 100) <= 50)
		{
			gameObject.SetStringProperty("QuestVerb", "pray at");
			gameObject.SetStringProperty("QuestEvent", "Prayed");
		}
		else
		{
			gameObject.SetStringProperty("QuestVerb", "desecrate");
			gameObject.SetStringProperty("QuestEvent", "Desecrated");
		}
		gameObject.RequirePart<Shrine>();
		return gameObject;
	}

	public override void FireLine(List<Cell> Path)
	{
		PlayWorldSound("burn_blast", 1f, 0f, Combat: true);
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		List<GameObject> list = Event.NewGameObjectList();
		bool flag = false;
		int i = 0;
		for (int count = Path.Count; i < count; i++)
		{
			Cell cell = Path[i];
			if (cell.IsBlackedOut())
			{
				break;
			}
			foreach (GameObject @object in cell.Objects)
			{
				if (IsValidTarget(@object) && !@object.MakeSave("Toughness", 20, ParentObject, "Ego", "Lithofex Stoning Gaze Beam"))
				{
					list.Add(@object);
				}
			}
			int num = 0;
			while (num < list.Count)
			{
				GameObject gameObject = list[num];
				if (gameObject.IsPlayer())
				{
					Achievement.TURNED_STONE.Unlock();
				}
				gameObject.SetIntProperty("SuppressCorpseDrops", 1);
				if (gameObject.Die(ParentObject, null, "You were turned to stone by the gaze of " + ParentObject.an() + ".", gameObject.It + gameObject.GetVerb("were", PrependSpace: true, PronounAntecedent: true) + " @@turned to stone by the gaze of " + ParentObject.an() + ".") && gameObject.IsValid())
				{
					flag = gameObject.IsPlayer();
					cell.AddObject(CreateStatueOf(gameObject));
				}
				list.RemoveAt(0);
			}
			if (cell.IsVisible())
			{
				scrapBuffer.RenderBase();
				if (i > 0)
				{
					scrapBuffer.WriteAt(Path[i - 1], "&b*");
				}
				if (i > 1)
				{
					scrapBuffer.WriteAt(Path[i - 2], "&K*");
				}
				scrapBuffer.WriteAt(cell, "&B*");
				scrapBuffer.Draw();
				Thread.Sleep(10);
			}
		}
		if (flag)
		{
			The.Core.RenderDelay(3000);
		}
	}

	public override string GetDescription()
	{
		return "You turn things to stone with your gaze.";
	}

	public override int GetRange(int Level)
	{
		return 7 + Level;
	}

	public override int GetCooldown(int Level)
	{
		return 50;
	}

	public override int GetDelay(int Level)
	{
		return 3;
	}

	public override string GetLevelText(int Level)
	{
		return "You can gaze {{rules|" + GetRange(Level) + "}} squares after a " + Grammar.Cardinal(GetDelay(Level)) + "-turn warmup and turn targets to stone.\nCooldown: {{rules|" + GetCooldown(Level) + "}} rounds";
	}
}
