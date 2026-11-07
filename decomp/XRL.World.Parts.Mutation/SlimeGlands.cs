using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class SlimeGlands : BaseMutation
{
	public static readonly string COMMAND_NAME = "CommandSpitSlime";

	public static readonly int RADIUS = 1;

	public static readonly int RANGE = 8;

	public SlimeGlands()
	{
		Type = "Physical";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("Range", RANGE);
		stats.Set("Area", "3x3");
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
	}

	public int GetCooldown(int Level)
	{
		return 40;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID)
		{
			return ID == PooledEvent<CommandEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance <= RANGE && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && !E.Actor.IsFrozen() && !E.Actor.OnWorldMap() && GameObject.Validate(E.Target) && E.Actor.HasLOSTo(E.Target, IncludeSolid: true, BlackoutStops: false, UseTargetability: true))
		{
			E.Add(COMMAND_NAME);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			if (!ParentObject.CheckFrozen())
			{
				return false;
			}
			if (ParentObject.OnWorldMap())
			{
				return ParentObject.Fail("You cannot do that on the world map.");
			}
			List<Cell> list = PickBurst(RADIUS, RANGE, Locked: false, AllowVis.OnlyVisible, "Spit Slime");
			if (list == null || list.Count == 0)
			{
				return false;
			}
			if (ParentObject.DistanceTo(list[0]) > RANGE)
			{
				return ParentObject.Fail("That is out of range! (" + RANGE.Things("square") + ")");
			}
			ParentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_spitSlime_spit");
			SlimeAnimation("&g", ParentObject.CurrentCell, list[0]);
			int num = 0;
			foreach (Cell item in list)
			{
				if (num == 0 || 80.in100())
				{
					item.AddObject("SlimePuddle");
				}
				num++;
			}
			UseEnergy(1000, "Physical Mutation SlimeGlands");
			CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown(Level));
			DidX("spit", "a pool of slime", "!", null, null, ParentObject);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override string GetDescription()
	{
		return string.Concat(string.Concat(string.Concat(string.Concat(string.Concat("" + "You produce a viscous slime that you can spit at things.\n\n", "Covers an area with slime\n"), "Range: 8\n"), "Area: 3x3\n"), "Cooldown: 40 rounds\n"), "You can walk over slime without slipping.");
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public static void SlimeAnimation(string Color, Cell StartCell, Cell EndCell)
	{
		if (StartCell == null || EndCell == null || StartCell.ParentZone == null)
		{
			return;
		}
		List<Point> list = Zone.Line(StartCell.X, StartCell.Y, EndCell.X, EndCell.Y);
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1(bLoadFromCurrent: true);
		TextConsole textConsole = Popup._TextConsole;
		CleanQueue<Point> cleanQueue = new CleanQueue<Point>();
		Point point = null;
		foreach (Point item in list)
		{
			point = item;
			bool flag = false;
			cleanQueue.Enqueue(item);
			while (cleanQueue.Count > 3)
			{
				cleanQueue.Dequeue();
			}
			int num = 0;
			for (int i = 0; i < cleanQueue.Items.Count; i++)
			{
				Point point2 = cleanQueue.Items[i];
				if (StartCell.ParentZone.GetCell(point2.X, point2.Y).IsVisible())
				{
					if (!flag)
					{
						flag = true;
						XRLCore.Core.RenderBaseToBuffer(scrapBuffer);
					}
					scrapBuffer.Goto(point2.X, point2.Y);
					if (cleanQueue.Count == 1)
					{
						scrapBuffer.Write(Color + "\a");
					}
					else if (cleanQueue.Count == 2)
					{
						if (num == 0)
						{
							scrapBuffer.Write(Color + "ú");
						}
						if (num == 1)
						{
							scrapBuffer.Write(Color + "\a");
						}
					}
					else
					{
						if (num == 0)
						{
							scrapBuffer.Write(Color + "ù");
						}
						if (num == 1)
						{
							scrapBuffer.Write(Color + "ú");
						}
						if (num == 2)
						{
							scrapBuffer.Write(Color + "\a");
						}
					}
				}
				num++;
			}
			if (flag)
			{
				textConsole.DrawBuffer(scrapBuffer);
				Thread.Sleep(30);
			}
		}
		if (point != null)
		{
			for (int j = 0; j < 5; j++)
			{
				float num2 = 0f;
				float num3 = 0f;
				float num4 = (float)Stat.Random(0, 359) / 58f;
				num2 = (float)Math.Sin(num4) / 2f;
				num3 = (float)Math.Cos(num4) / 2f;
				XRLCore.ParticleManager.Add(Color + ".", point.X, point.Y, num2, num3, 5, 0f, 0f, 0L);
			}
		}
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		GO.Slimewalking = true;
		ActivatedAbilityID = AddMyActivatedAbility("Spit Slime", COMMAND_NAME, "Physical Mutations", null, "\u00ad");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		GO.Slimewalking = false;
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
