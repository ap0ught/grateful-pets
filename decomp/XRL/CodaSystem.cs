using System;
using System.Threading;
using Qud.UI;
using XRL.UI;
using XRL.World;

namespace XRL;

[Serializable]
public class CodaSystem : IPlayerSystem
{
	[NonSerialized]
	public GameObject Sultan;

	[NonSerialized]
	public long EndTime;

	public int EndYear => Calendar.GetYear(EndTime);

	public static bool InCoda()
	{
		if (The.Game.GetSystem<CodaSystem>() != null)
		{
			return true;
		}
		return false;
	}

	public override void Write(SerializationWriter Writer)
	{
		Writer.WriteGameObject(Sultan);
		Writer.WriteOptimized(EndTime);
	}

	public override void Read(SerializationReader Reader)
	{
		Sultan = Reader.ReadGameObject();
		EndTime = Reader.ReadOptimizedInt64();
	}

	public override void Register(XRLGame Game, IEventRegistrar Registrar)
	{
		Registrar.Register(AwardingXPEvent.ID);
	}

	public override void RegisterPlayer(GameObject Player, IEventRegistrar Registrar)
	{
		Registrar.Register(EnteringZoneEvent.ID);
	}

	public override bool HandleEvent(AwardingXPEvent E)
	{
		return false;
	}

	public override bool HandleEvent(EnteringZoneEvent E)
	{
		if (E.Cell.ParentZone is InteriorZone { Schema: "Coda", X: 1, Y: 1 })
		{
			return true;
		}
		EndGamePrompt();
		return false;
	}

	public static void EndGamePrompt()
	{
		string text = "END GAME";
		if (Popup.AskString("Leaving the village will end the game.\n\nType " + text + " to confirm.", "", "Sounds/UI/ui_notification", null, text, text.Length, 0, ReturnNullForEscape: false, EscapeNonMarkupFormatting: true, false).ToUpper() == text)
		{
			SoundManager.StopMusic("music", Crossfade: true, 3f);
			if (The.Game.DontSaveThisIsAReplay)
			{
				The.Game.DeathReason = "<nodeath>";
				The.Game.DeathCategory = "exit";
				The.Game.Running = false;
				return;
			}
			FadeToBlackHigh.FadeOut(10f);
			Thread.Sleep(3000);
			SoundManager.PlayMusic("Music/CodaOut", "music", Crossfade: true, 2f);
			Thread.Sleep(7000);
			The.Game.DeathReason = "You ended the game.";
			The.Game.Running = false;
		}
	}
}
