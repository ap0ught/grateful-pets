using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using ConsoleLib.Console;
using Occult.Engine.CodeGeneration;
using Qud.API;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
[HasWishCommand]
[GenerateSerializationPartial]
public class TerrainTravel : IPart
{
	public int LostChance = 12;

	public int Segments = 50;

	public string TravelClass = "";

	public List<EncounterEntry> Encounters;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.WriteOptimized(LostChance);
		Writer.WriteOptimized(Segments);
		Writer.WriteOptimized(TravelClass);
		Writer.WriteComposite(Encounters);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		LostChance = Reader.ReadOptimizedInt32();
		Segments = Reader.ReadOptimizedInt32();
		TravelClass = Reader.ReadOptimizedString();
		Encounters = Reader.ReadCompositeList<EncounterEntry>();
	}

	public override bool SameAs(IPart Part)
	{
		TerrainTravel terrainTravel = Part as TerrainTravel;
		if (terrainTravel.LostChance != LostChance)
		{
			return false;
		}
		if (terrainTravel.Segments != Segments)
		{
			return false;
		}
		if (terrainTravel.TravelClass != TravelClass)
		{
			return false;
		}
		if (terrainTravel.Encounters != null || Encounters != null)
		{
			return false;
		}
		return base.SameAs(Part);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ObjectEnteredCellEvent.ID)
		{
			return ID == ObjectLeavingCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (E.Object.IsPlayer())
		{
			if (!ParentObject.FireEvent("CheckLostChance"))
			{
				return false;
			}
			if (!Encounters.IsNullOrEmpty() && TutorialManager.AllowOverlandEncounters())
			{
				int num = 10;
				int num2 = num;
				if (Options.DebugEncounterChance)
				{
					int num3 = EncounterChanceEvent.GetFor(E.Object, TravelClass);
					num2 = num * (100 + num3) / 100;
					IComponent<GameObject>.AddPlayerMessage("Base encounter chance: " + num2 + "%");
				}
				foreach (EncounterEntry encounter in Encounters)
				{
					if (!encounter.Enabled)
					{
						continue;
					}
					int num4 = EncounterChanceEvent.GetFor(E.Object, TravelClass, 0, encounter);
					int num5 = num;
					if (num4 != 0)
					{
						num5 = num5 * (100 + num4) / 100;
					}
					if (num5 != num2 && Options.DebugEncounterChance)
					{
						IComponent<GameObject>.AddPlayerMessage("Modified encounter chance: " + num5 + "%");
					}
					if (num5.in100())
					{
						if (Options.DebugEncounterChance)
						{
							IComponent<GameObject>.AddPlayerMessage("Triggered encounter chance: " + num5 + "%");
						}
						if (encounter.secretID.IsNullOrEmpty() || !JournalAPI.GetMapNote(encounter.secretID).Revealed)
						{
							SoundManager.PlayUISound("ui_worldmap_encounter");
							if (!encounter.Optional || Popup.ShowYesNo(encounter.Text) == DialogResult.Yes)
							{
								if (!encounter.Optional && !encounter.Text.IsNullOrEmpty())
								{
									Popup.Show(encounter.Text);
								}
								Zone zone = The.ZoneManager.SetActiveZone(encounter.Zone);
								zone.CheckWeather();
								E.Object.SystemMoveTo(zone.GetPullDownLocation(E.Object));
								The.ZoneManager.ProcessGoToPartyLeader();
								RemoveEncounter(encounter);
								return false;
							}
							break;
						}
						break;
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectLeavingCellEvent E)
	{
		if (E.Object != null && E.Object.IsPlayer() && !E.Forced && (E.Type == null || E.Type == "SystemLongDistanceMove") && !ParentObject.CurrentZone.ZoneID.Contains("Serenity"))
		{
			int TotalSegments = 0;
			if (E.Object == null)
			{
				MetricsManager.LogError(new Exception("no object in ObjectLeavingCellEvent"));
				return base.HandleEvent(E);
			}
			if (!HandleLeavingCell(E.Object, ref TotalSegments))
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public int GetTravelSegments(GameObject Object)
	{
		int num = TravelSpeedEvent.GetFor(Object, TravelClass);
		return (int)(300f / ((float)Object.Speed * (100f + (float)num) / 100f) * (float)Segments);
	}

	private bool HandleLeavingCell(GameObject Object, ref int TotalSegments)
	{
		Cell cell = The.Player.CurrentCell;
		XRLGame game = The.Game;
		_ = game.ZoneManager;
		ActionManager actionManager = game.ActionManager;
		if (Object.IsPlayer())
		{
			game.SetStringGameState("LastLocationOnSurface", "");
		}
		int num = GetLostChanceEvent.GetFor(The.Player, TravelClass);
		TotalSegments = GetTravelSegments(Object);
		int chance = Math.Max(LostChance * (100 - num) / 100, 0);
		if (The.Core.IDKFA || !TutorialManager.AllowOverlandEncounters())
		{
			chance = 0;
		}
		if (Object.IsPlayer() && Options.DebugGetLostChance)
		{
			IComponent<GameObject>.AddPlayerMessage("Get lost chance: " + chance + "%");
		}
		if (chance.in100())
		{
			string zoneWorld = cell.ParentZone.GetZoneWorld();
			int x = cell.X;
			int y = cell.Y;
			int zoneZ = 10;
			string text = ZoneID.Assemble(zoneWorld, x, y, Stat.Random(0, 2), Stat.Random(0, 2), zoneZ);
			if (!The.ZoneManager.IsZoneBuilt(text))
			{
				Zone zone = The.ZoneManager.GetZone(text);
				Cell pullDownLocation = zone.GetPullDownLocation(The.Player);
				if (pullDownLocation.IsPassable(The.Player))
				{
					Lost e = new Lost(9999, text, zoneWorld);
					if (Object.ApplyEffect(e))
					{
						zone.CheckWeather();
						The.Player.SystemMoveTo(pullDownLocation);
						Popup.ShowSpace("You're lost! Regain your bearings by exploring your surroundings.");
						The.Player.FireEvent(Event.New("AfterLost", "FromCell", cell));
						return false;
					}
				}
			}
		}
		Cell cell2 = Object.CurrentCell;
		if (Object.IsPlayer() && Options.DebugTravelSpeed)
		{
			IComponent<GameObject>.AddPlayerMessage("Travel speed: " + TotalSegments + " segments/parasang");
		}
		bool TravelMessagesSuppressed = false;
		XRLCore core = The.Core;
		int amount = Math.Max(1, TotalSegments / 10);
		for (int i = 0; i < TotalSegments; i++)
		{
			actionManager.RunCommands();
			if (cell2 != Object.CurrentCell)
			{
				return false;
			}
			if (i % 10 != 0)
			{
				continue;
			}
			game.TimeTicks++;
			if (!BeforeBeginTakeActionEvent.Check(Object) || !BeginTakeActionEvent.Check(Object, Traveling: true, ref TravelMessagesSuppressed))
			{
				return false;
			}
			if (cell2 != Object.CurrentCell)
			{
				return false;
			}
			if (!CommandTakeActionEvent.Check(Object))
			{
				return false;
			}
			if (cell2 != Object.CurrentCell)
			{
				return false;
			}
			actionManager.ProcessIndependentEndTurn(Object);
			if (cell2 != Object.CurrentCell)
			{
				return false;
			}
			if (core.HPWarning)
			{
				core.HPWarning = false;
				if (Popup.ShowYesNo("{{R|Your health has dropped below {{C|" + Globals.HPWarningThreshold + "%}}!}} Do you want to stop travelling?") == DialogResult.Yes)
				{
					return false;
				}
			}
			MinEvent.ResetPools();
		}
		actionManager.ProcessTurnTick(game.TimeTicks, amount);
		actionManager.TickAbilityCooldowns(TotalSegments);
		Object.Energy.BaseValue = 2200;
		Object.CleanEffects();
		AfterTravelEvent.Send(Object, ParentObject, TotalSegments);
		Object.FireEvent(Event.New("AfterTravel", "Segments", TotalSegments));
		if (Object.IsPlayer())
		{
			The.Game.Player.Messages.BeginPlayerTurn();
		}
		return true;
	}

	public void AddEncounter(EncounterEntry Entry)
	{
		if (Encounters == null)
		{
			Encounters = new List<EncounterEntry>();
		}
		Encounters.Add(Entry);
		if (ParentObject.HasTag("Immutable") || ParentObject.HasTag("ImmutableWhenUnexplored"))
		{
			ParentObject.SetIntProperty("ForceMutableSave", 1);
		}
	}

	public void RemoveEncounter(EncounterEntry Entry)
	{
		if (Encounters != null)
		{
			Encounters.Remove(Entry);
			if (Encounters.Count == 0)
			{
				Encounters = null;
			}
		}
	}

	public void ClearEncounters()
	{
		Encounters = null;
	}

	[WishCommand("terrainencounters", null)]
	public static void ShowTerrainEncounters()
	{
		Keys keys = Keys.None;
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		Zone zone = The.ZoneManager.GetZone(The.ActiveZone.ZoneWorld);
		List<EncounterEntry> list = new List<EncounterEntry>();
		do
		{
			list.Clear();
			int num = -1;
			int num2 = -1;
			if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "LeftClick")
			{
				num = Keyboard.CurrentMouseEvent.x;
				num2 = Keyboard.CurrentMouseEvent.y;
			}
			for (int i = 0; i < zone.Height; i++)
			{
				for (int j = 0; j < zone.Width; j++)
				{
					int k = 0;
					int num3 = 0;
					for (int count = zone.Map[j][i].Objects.Count; k < count; k++)
					{
						TerrainTravel firstPartDescendedFrom = zone.Map[j][i].Objects[k].GetFirstPartDescendedFrom<TerrainTravel>();
						if (firstPartDescendedFrom != null)
						{
							num3 += firstPartDescendedFrom.Encounters.Count;
							scrapBuffer.WriteAt(j, i, (num3 < 10) ? num3.ToString() : "+");
							if (num == j && num2 == i)
							{
								list.AddRange(firstPartDescendedFrom.Encounters);
							}
						}
					}
				}
			}
			int num4 = ((num2 >= 12) ? 1 : 23);
			foreach (EncounterEntry item in list)
			{
				scrapBuffer.WriteAt(1, (num2 < 12) ? num4-- : num4++, "{{W|" + item.Text + "}}");
			}
			scrapBuffer.Draw();
		}
		while ((keys = Keyboard.getvk(MapDirectionToArrows: false)) != Keys.Escape);
	}
}
