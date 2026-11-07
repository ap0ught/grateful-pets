using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.Wish;
using XRL.World.AI;
using XRL.World.Parts.Mutation;
using XRL.World.Quests;

namespace XRL.World.Parts;

[Serializable]
[HasWishCommand]
public class NephalProperties : IPart
{
	[NonSerialized]
	public static readonly string[] Nephilim = new string[7] { "Agolgot", "Bethsaida", "Rermadon", "Qas", "Qon", "Shugruith", "Ehalcodon" };

	[NonSerialized]
	private static Dictionary<string, Type> _Chords;

	public int Phase = 1;

	public int PhaseHealth = 50;

	public string PhaseAction = "";

	public string PhaseMessage = "";

	[NonSerialized]
	private static bool Preloaded;

	public static Dictionary<string, Type> Chords
	{
		get
		{
			if (_Chords == null)
			{
				_Chords = new Dictionary<string, Type>();
				foreach (Type item in ModManager.GetTypesAssignableFrom(typeof(INephalChord), Cache: false))
				{
					if (!item.IsAbstract)
					{
						INephalChord nephalChord = (INephalChord)Activator.CreateInstance(item, nonPublic: true);
						Chords[nephalChord.Source] = item;
					}
				}
			}
			return _Chords;
		}
	}

	public static bool AllPacified()
	{
		string[] nephilim = Nephilim;
		for (int i = 0; i < nephilim.Length; i++)
		{
			if (!IsPacified(nephilim[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static bool AnyPacified()
	{
		string[] nephilim = Nephilim;
		for (int i = 0; i < nephilim.Length; i++)
		{
			if (IsPacified(nephilim[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static bool AllDead()
	{
		string[] nephilim = Nephilim;
		for (int i = 0; i < nephilim.Length; i++)
		{
			if (!IsDead(nephilim[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static bool AnyDead()
	{
		string[] nephilim = Nephilim;
		for (int i = 0; i < nephilim.Length; i++)
		{
			if (IsDead(nephilim[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static bool AllFoiled()
	{
		string[] nephilim = Nephilim;
		for (int i = 0; i < nephilim.Length; i++)
		{
			if (!IsFoiled(nephilim[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsPacified(string Nephal)
	{
		if (The.Game.HasDelimitedGameState(Nephal, ',', "Pacified"))
		{
			return !The.Game.HasDelimitedGameState(Nephal, ',', "Dead");
		}
		return false;
	}

	public static bool IsDead(string Nephal)
	{
		return The.Game.HasDelimitedGameState(Nephal, ',', "Dead");
	}

	public static bool IsFoiled(string Nephal)
	{
		return The.Game.HasAnyDelimitedGameState(Nephal, ',', "Dead", "Pacified");
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != TookDamageEvent.ID && ID != BeforeDeathRemovalEvent.ID && ID != AfterObjectCreatedEvent.ID && ID != ZoneActivatedEvent.ID)
		{
			return ID == ReplicaCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		if (!Preloaded)
		{
			PreloadClips();
		}
		if (Phase < 2 && E.Object.GetHPPercent() <= PhaseHealth)
		{
			Phase = 2;
			ExecuteAction(PhaseAction);
			ParentObject.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_enraged_big", 0.5f, 0f, Combat: false, 0f, 0.9f);
			ParentObject.PlayWorldSound("Sounds/Creatures/VO/sfx_creature_insect_generic_vo_idle", 0.5f, 0f, Combat: false, 0f, 0.75f);
			CombatJuice.cameraShake(2f);
			if (!PhaseMessage.IsNullOrEmpty())
			{
				char color = IComponent<GameObject>.ConsequentialColorChar(ParentObject);
				EmitMessage(GameText.VariableReplace(PhaseMessage, E.Object, E.Actor), color);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (Phase < 3)
		{
			Phase = 3;
			string baseDisplayNameStripped = ParentObject.BaseDisplayNameStripped;
			Popup.Show("A sphere of light in the chord of " + baseDisplayNameStripped + " radiates away.\n\nYou feel it absorbed elsewhere.");
			CheckChords();
		}
		CheckAchievement();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		AbsorbChords();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		AbsorbChords();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplicaCreatedEvent E)
	{
		if (E.Object == ParentObject)
		{
			Phase = 3;
			if (ParentObject.TryGetPart<DropOnDeath>(out var Part))
			{
				E.WantToRemove(Part);
			}
		}
		return base.HandleEvent(E);
	}

	public void CheckAchievement()
	{
		int num = 0;
		int num2 = 0;
		string[] nephilim = Nephilim;
		foreach (string nephal in nephilim)
		{
			if (IsDead(nephal))
			{
				num++;
			}
			else if (IsPacified(nephal))
			{
				num2++;
			}
		}
		if (num >= 1)
		{
			Achievement.ALEPH.Unlock();
		}
		if (num >= 2)
		{
			Achievement.BET.Unlock();
		}
		if (num >= 3)
		{
			Achievement.GIMEL.Unlock();
		}
		if (num >= 4)
		{
			Achievement.DALET.Unlock();
		}
		if (num >= 5)
		{
			Achievement.HE.Unlock();
		}
		if (num >= 6)
		{
			Achievement.VAV.Unlock();
		}
		if (num >= 7)
		{
			Achievement.ZAYIN.Unlock();
		}
		if (num2 >= 7)
		{
			Achievement.PACIFY_ALL.Unlock();
		}
	}

	public bool TryPacify()
	{
		if (!The.Game.TryAddDelimitedGameState(The.Speaker.Blueprint, ',', "Pacified"))
		{
			return false;
		}
		ParentObject.RequirePart<Calming>();
		ParentObject.StopFighting();
		AllegianceSet allegiance = ParentObject.Brain.Allegiance;
		allegiance.Hostile = false;
		allegiance.Calm = true;
		Zone.ObjectEnumerator objectEnumerator;
		Zone.ObjectEnumerator enumerator;
		if (ParentObject.TryGetPart<Spawner>(out var Part))
		{
			Part.CombatOnly = true;
			if (Part.PassAttitudes)
			{
				objectEnumerator = ParentObject.CurrentZone.IterateObjects();
				enumerator = objectEnumerator.GetEnumerator();
				while (enumerator.MoveNext())
				{
					GameObject current = enumerator.Current;
					if (current.TryGetPart<SpawnVessel>(out var Part2) && Part2.SpawnedBy == ParentObject)
					{
						current.TakeDemeanor(ParentObject);
					}
					if (current.Blueprint == Part.SpawnCheckBlueprint)
					{
						current.TakeDemeanor(ParentObject);
					}
				}
			}
		}
		if (The.Speaker.TryGetPart<IrisdualBeam>(out var Part3))
		{
			Part3.Stop();
		}
		objectEnumerator = ParentObject.CurrentZone.IterateObjects();
		enumerator = objectEnumerator.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.StopFighting();
		}
		The.Core.RenderBase();
		CheckAchievement();
		Popup.Show(The.Speaker.Does("slouch") + " in pacification and radiates a chord of light.");
		return true;
	}

	public void CheckChords()
	{
		Zone.ObjectEnumerator enumerator = ParentObject.CurrentZone.IterateObjects().GetEnumerator();
		while (enumerator.MoveNext())
		{
			GameObject current = enumerator.Current;
			if (current.IsCombatObject() && current != ParentObject && current.TryGetPart<NephalProperties>(out var Part))
			{
				Part.AbsorbChords(ParentObject);
			}
		}
	}

	public void AbsorbChords(GameObject From = null)
	{
		if (From != null && From.Blueprint != ParentObject.Blueprint)
		{
			DidXToY("absorb", "a chord of", From, "light", null, null, null, ParentObject, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: true);
		}
		string[] nephilim = Nephilim;
		foreach (string text in nephilim)
		{
			if (IsDead(text) && !(text == ParentObject.Blueprint) && !HasChord(text))
			{
				AddChord(text);
			}
		}
	}

	public bool HasChord(string Nephal)
	{
		foreach (IPart parts in ParentObject.PartsList)
		{
			if (parts is INephalChord nephalChord && nephalChord.Source == Nephal)
			{
				return true;
			}
		}
		return false;
	}

	public bool AddChord(string Nephal)
	{
		if (!Chords.TryGetValue(Nephal, out var value))
		{
			return false;
		}
		INephalChord p = (INephalChord)Activator.CreateInstance(value, nonPublic: true);
		ParentObject.AddPart(p);
		return true;
	}

	public void PreloadClips()
	{
		SoundManager.PreloadClipSet("Sounds/StatusEffects/sfx_statusEffect_enraged_big");
		SoundManager.PreloadClipSet("Sounds/Creatures/VO/sfx_creature_insect_generic_vo_idle");
		Preloaded = true;
	}

	public void ExecuteAction(string Action)
	{
		if (Action == "Split")
		{
			Statistic stat = ParentObject.GetStat("Hitpoints");
			int num = stat.Value / 2;
			stat.Penalty -= num;
			stat.BaseValue -= num;
			GameObject gameObject = ParentObject.DeepCopy();
			ParentObject.CurrentCell.getClosestEmptyCell().AddObject(gameObject);
			WasReplicatedEvent.Send(ParentObject, ParentObject, gameObject, "NephalProperties");
			ReplicaCreatedEvent.Send(gameObject, ParentObject, ParentObject, "NephalProperties");
		}
		else if (Action == "Beam")
		{
			Mutations part = ParentObject.GetPart<Mutations>();
			IrisdualBeam part2 = ParentObject.GetPart<IrisdualBeam>();
			part.LevelMutation(part2, 10);
			part2.TakeMyActivatedAbilityOffCooldown(part2.ActivatedAbilityID);
		}
	}

	[WishCommand("nephilim:chords", null)]
	public static void WishChords()
	{
		foreach (KeyValuePair<string, Type> chord in Chords)
		{
			INephalChord p = (INephalChord)Activator.CreateInstance(chord.Value, nonPublic: true);
			The.Player.AddPart(p);
		}
	}

	[WishCommand("nephilim:pacify", null)]
	public static void WishPacify()
	{
		ReclamationSystem system = The.Game.GetSystem<ReclamationSystem>();
		string text = "";
		if (system != null && system.Stage == 1)
		{
			text = system.Quest.StepsByID["Nephal"].Value;
		}
		string[] nephilim = Nephilim;
		foreach (string text2 in nephilim)
		{
			if (!(text2 == text))
			{
				The.Game.TryAddDelimitedGameState(text2, ',', "Pacified");
			}
		}
	}

	[WishCommand("nephilim:dead", null)]
	public static void WishDead()
	{
		ReclamationSystem system = The.Game.GetSystem<ReclamationSystem>();
		string text = "";
		if (system != null && system.Stage == 1)
		{
			text = system.Quest.StepsByID["Nephal"].Value;
		}
		string[] nephilim = Nephilim;
		foreach (string text2 in nephilim)
		{
			if (!(text2 == text))
			{
				The.Game.TryAddDelimitedGameState(text2, ',', "Dead");
			}
		}
	}
}
