using System;
using Qud.API;
using XRL.Core;
using XRL.World;
using XRL.World.Parts;

namespace XRL;

[Serializable]
public class WanderSystem : IPlayerSystem
{
	public static int WXU
	{
		get
		{
			int level = The.Player.Level;
			int num = Leveler.GetXPForLevel(level + 1) - Leveler.GetXPForLevel(level);
			if (level <= 2)
			{
				num /= 2;
			}
			else if (level <= 5)
			{
				num /= 3;
			}
			else if (level <= 10)
			{
				num /= 5;
			}
			else if (level <= 20)
			{
				num /= 6;
			}
			else
			{
				num = ((level > 30) ? (num / 10) : (num / 8));
			}
			num = (int)Math.Round((float)num / 10f) * 10;
			return Math.Max(10, num);
		}
	}

	public static bool WanderEnabled()
	{
		XRLGame game = XRLCore.Core.Game;
		if (!(game.GetStringGameState("GameMode") == "Wander"))
		{
			return game.GetStringGameState("WanderEnabled") == "yes";
		}
		return true;
	}

	public static int AwardWXU(int n)
	{
		if (The.Player == null)
		{
			return 0;
		}
		if (WanderEnabled())
		{
			int num = WXU * n;
			The.Player.AwardXP(num);
			return num;
		}
		return 0;
	}

	public override void Register(XRLGame Game, IEventRegistrar Registrar)
	{
		Registrar.Register(SingletonEvent<EmbarkEvent>.ID);
		Registrar.Register(QuestFinishedEvent.ID);
		Registrar.Register(PooledEvent<WaterRitualStartEvent>.ID);
		Registrar.Register(PooledEvent<SecretVisibilityChangedEvent>.ID);
	}

	public override void RegisterPlayer(GameObject Player, IEventRegistrar Registrar)
	{
		Registrar.Register(AwardingXPEvent.ID);
	}

	public override bool HandleEvent(EmbarkEvent E)
	{
		if (E.EventID == "BootGame" && WanderEnabled())
		{
			foreach (Faction item in Factions.Loop())
			{
				if (The.Game.PlayerReputation.Get(item) < 0 && !item.HatesPlayer)
				{
					The.Game.PlayerReputation.Set(item, 0);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QuestFinishedEvent E)
	{
		if (WanderEnabled())
		{
			AwardWXU(2);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AwardingXPEvent E)
	{
		if (WanderEnabled() && E.Kill != null)
		{
			E.Amount = 0;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(SecretVisibilityChangedEvent E)
	{
		if (WanderEnabled() && E.Entry is JournalMapNote { Revealed: not false, Tradable: not false })
		{
			AwardWXU(1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(WaterRitualStartEvent E)
	{
		if (WanderEnabled() && GameObject.Validate(E.SpeakingWith) && !E.SpeakingWith.HasIntProperty("WaterRitualWXU"))
		{
			E.SpeakingWith.SetIntProperty("WaterRitualWXU", 1);
			AwardWXU(1);
		}
		return base.HandleEvent(E);
	}
}
