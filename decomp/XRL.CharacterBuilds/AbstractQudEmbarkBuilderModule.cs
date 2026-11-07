using System;
using XRL.World;
using XRL.World.Parts;

namespace XRL.CharacterBuilds;

/// <summary>
///     Abstract base class for Qud EmbarkBuilder modules.
/// </summary>
public abstract class AbstractQudEmbarkBuilderModule : AbstractEmbarkBuilderModule
{
	protected static void PostprocessPlayerItem(GameObject Object)
	{
		Object.Seen();
		Object.MakeUnderstood();
		Object.GetPart<EnergyCellSocket>()?.Cell?.MakeUnderstood();
		Object.GetPart<MagazineAmmoLoader>()?.Ammo?.MakeUnderstood();
	}

	protected static void AddItem(GameObject Object, GameObject Player)
	{
		PostprocessPlayerItem(Object);
		Player.ReceiveObject(Object);
	}

	protected static void AddItem(string Blueprint, GameObject Player)
	{
		try
		{
			Player.ReceiveObject(Blueprint, NoStack: false, 0, 0, null, null, PostprocessPlayerItem);
		}
		catch (Exception x)
		{
			MetricsManager.LogError("exception creating " + Blueprint, x);
		}
	}

	protected static void AddItem(string Blueprint, int Number, GameObject Player)
	{
		try
		{
			Player.ReceiveObject(Blueprint, Number, NoStack: false, 0, 0, null, null, PostprocessPlayerItem);
		}
		catch (Exception x)
		{
			MetricsManager.LogError("exception creating " + Blueprint, x);
		}
	}
}
