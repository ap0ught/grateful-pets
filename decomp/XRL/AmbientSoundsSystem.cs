using System;
using System.Collections.Generic;
using System.Diagnostics;
using XRL.Core;
using XRL.Rules;
using XRL.Wish;
using XRL.World;

namespace XRL;

[Serializable]
[HasWishCommand]
[HasOptionFlagUpdate]
public class AmbientSoundsSystem : IGameSystem
{
	[NonSerialized]
	public static Stopwatch Timer = new Stopwatch();

	[NonSerialized]
	public static float NextSound = 2000f;

	[NonSerialized]
	public static bool Enabled = false;

	[NonSerialized]
	public static string[] Sounds = null;

	[NonSerialized]
	public static List<Cell> EmptyCells = new List<Cell>(2000);

	[OptionFlagUpdate]
	public static void OnOptionUpdate()
	{
		if (Enabled != Globals.EnableAmbient)
		{
			if (Enabled)
			{
				Timer.Reset();
				PlayAmbientBeds();
			}
			else
			{
				Timer.Restart();
				PlayAmbientBeds(The.ActiveZone);
			}
			Enabled = Globals.EnableAmbient;
		}
	}

	public static void Update()
	{
		if ((float)Timer.ElapsedMilliseconds < NextSound)
		{
			return;
		}
		Timer.Restart();
		NextSound = Stat.RandomCosmetic(1000, 15000);
		if (Sounds != null && Sounds.Length != 0)
		{
			EmptyCells.Clear();
			The.ActiveZone?.GetEmptyCells(null, EmptyCells);
			if (EmptyCells.Count > 0)
			{
				EmptyCells.GetRandomElement().PlayWorldSound(Sounds.GetRandomElement(), Globals.AmbientVolume * 0.33f);
			}
			return;
		}
		Zone activeZone = The.ActiveZone;
		object obj;
		if (activeZone == null)
		{
			obj = null;
		}
		else
		{
			List<GameObject> objectsWithTagOrProperty = activeZone.GetObjectsWithTagOrProperty("AmbientIdleSound", UseEventList: true);
			obj = ((objectsWithTagOrProperty != null) ? objectsWithTagOrProperty.GetRandomElement() : null);
		}
		GameObject gameObject = (GameObject)obj;
		if (gameObject != null && !gameObject.IsPlayer())
		{
			string soundTag = gameObject.GetSoundTag("AmbientIdleSound");
			if (soundTag != null)
			{
				gameObject.PlayWorldSound(soundTag, Globals.AmbientVolume * 0.33f);
			}
		}
	}

	[WishCommand(null, null, Command = "ambient")]
	public static bool TestAmbient(string beds)
	{
		string[] array = beds.Split(' ');
		PlayAmbientBeds((array.Length >= 1) ? ("Sounds/Ambiences/" + array[0]) : null, (array.Length >= 2) ? ("Sounds/Ambiences/" + array[1]) : null, (array.Length >= 3) ? ("Sounds/Ambiences/" + array[2]) : null);
		return true;
	}

	[WishCommand(null, null, Command = "ambientcreature")]
	public static bool TestAmbientCreature(string bed)
	{
		PlayAmbientBeds(The.ActiveZone, null, "Sounds/Ambiences/amb_creature_" + bed);
		return true;
	}

	public static void PlayAmbientBeds(string First = null, string Second = null, string Third = null, float? Volume = null)
	{
		if (!Volume.HasValue)
		{
			Volume = (((First != null) ? 1 : 0) + ((Second != null) ? 1 : 0) + ((Third != null) ? 1 : 0)) switch
			{
				2 => 0.8f, 
				3 => 0.6f, 
				_ => 1f, 
			};
		}
		SoundManager.PlayMusic(First, "ambient_bed", Crossfade: true, 12f, Volume.Value);
		SoundManager.PlayMusic(Second, "ambient_bed_2", Crossfade: true, 12f, Volume.Value);
		SoundManager.PlayMusic(Third, "ambient_bed_3", Crossfade: true, 12f, Volume.Value);
	}

	public static void PlayAmbientBeds(Zone Zone, string First = null, string Second = null, string Third = null, float? Volume = null)
	{
		if (Zone == null)
		{
			PlayAmbientBeds();
			return;
		}
		if (First == null)
		{
			int z = Zone.Z;
			string text;
			if (z > 20)
			{
				if (z > 40)
				{
					text = "Sounds/Ambiences/amb_bed_caves_deepest";
				}
				else
				{
					text = ((z <= 30) ? "Sounds/Ambiences/amb_bed_caves_deep" : "Sounds/Ambiences/amb_bed_caves_deeper");
				}
			}
			else
			{
				text = ((z <= 10) ? Zone.AmbientBed : "Sounds/Ambiences/amb_bed_caves");
			}
			First = text;
		}
		if (Second == null)
		{
			Second = Zone.GetZoneProperty("ambient_bed_2", null);
		}
		if (Third == null)
		{
			Third = Zone.GetZoneProperty("ambient_bed_3", null);
		}
		if (!Volume.HasValue)
		{
			int ambientVolume = Zone.AmbientVolume;
			if (ambientVolume != -1)
			{
				Volume = (float)ambientVolume / 100f;
			}
		}
		PlayAmbientBeds(First, Second, Third, Volume);
	}

	public static void StopAmbientBeds(bool Crossfade = true)
	{
		SoundManager.PlayMusic(null, "ambient_bed", Crossfade, 1f);
		SoundManager.PlayMusic(null, "ambient_bed_2", Crossfade, 1f);
		SoundManager.PlayMusic(null, "ambient_bed_3", Crossfade, 1f);
	}

	public static void SetAmbientSounds(Zone Zone)
	{
		if (Zone == null || Zone.AmbientSounds.IsNullOrEmpty())
		{
			Sounds = null;
		}
		else
		{
			Sounds = Zone.AmbientSounds.Split(',');
		}
	}

	public override void Register(XRLGame Game, IEventRegistrar Registrar)
	{
		Registrar.Register(ZoneActivatedEvent.ID);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (Enabled)
		{
			Timer.Restart();
			PlayAmbientBeds(E.Zone);
			SetAmbientSounds(E.Zone);
		}
		return base.HandleEvent(E);
	}
}
