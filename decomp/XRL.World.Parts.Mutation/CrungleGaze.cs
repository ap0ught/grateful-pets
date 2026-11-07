using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Language;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class CrungleGaze : IDelayedLineMutation
{
	[NonSerialized]
	private static GameObject _Projectile;

	public override string Command => "CommandCrungleGaze";

	public override bool CanRefract => true;

	public override GameObject Projectile
	{
		get
		{
			if (!GameObject.Validate(ref _Projectile))
			{
				_Projectile = GameObject.CreateUnmodified("ProjectileCrungleGaze");
			}
			return _Projectile;
		}
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("Range", GetRange(Level), !stats.mode.Contains("ability"));
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
	}

	public override void FireLine(List<Cell> Path)
	{
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		bool flag = false;
		int i = 0;
		for (int count = Path.Count; i < count; i++)
		{
			Cell cell = Path[i];
			foreach (GameObject @object in cell.Objects)
			{
				if (IsValidTarget(@object))
				{
					if (@object.IsPlayer())
					{
						SoundManager.PlaySound("Sounds/StatusEffects/sfx_statusEffect_spacetimeWeirdness");
						Popup.Show("You feel drowsy.");
						flag = true;
					}
					else
					{
						@object.ApplyEffect(new Asleep(20));
					}
				}
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
			The.Player.ApplyEffect(new DeepDream(ParentObject));
		}
	}

	public override string GetDescription()
	{
		return "You provoke waking dreams with your gaze.";
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
		return "You can gaze {{rules|" + GetRange(Level) + "}} squares after a " + Grammar.Cardinal(GetDelay(Level)) + "-turn warmup and send your target to a waking dream.\nCooldown: {{rules|" + GetCooldown(Level) + "}} rounds";
	}
}
