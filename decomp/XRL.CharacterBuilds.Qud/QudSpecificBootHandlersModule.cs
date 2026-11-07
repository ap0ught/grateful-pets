using System;
using XRL.Annals;
using XRL.UI;
using XRL.World;
using XRL.World.AI;
using XRL.World.Encounters;
using XRL.World.Parts;

namespace XRL.CharacterBuilds.Qud;

/// <summary>
/// Contains all the caves of qud specific boot handler logic
/// </summary>
public class QudSpecificBootHandlersModule : AbstractEmbarkBuilderModule
{
	public override void InitFromSeed(string seed)
	{
	}

	public override object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element = null)
	{
		if (id == QudGameBootModule.BOOTEVENT_AFTERINITIALIZEWORLDS)
		{
			The.Core.CreateCures();
			The.Core.CreateMarkOfDeath();
			DimensionManager.Init();
			BookUI.InitBooks();
		}
		else if (id == QudGameBootModule.BOOTEVENT_INITIALIZEHISTORY)
		{
			game.sultanHistory = QudHistoryFactory.GenerateNewSultanHistory();
			game.sultanHistory = QudHistoryFactory.GenerateVillageEraHistory(game.sultanHistory);
			game.sultanHistory = info.fireBootEvent(QudGameBootModule.BOOTEVENT_INITIALIZESULTANHISTORY, game, game.sultanHistory);
			game.sultanHistory = info.fireBootEvent(QudGameBootModule.BOOTEVENT_AFTERINITIALIZESULTANHISTORY, game, game.sultanHistory);
		}
		else if (id == QudGameBootModule.BOOTEVENT_INITIALIZESYSTEMS)
		{
			game.AddSystem(new AmbientSoundsSystem());
			game.AddSystem(new CheckpointingSystem());
			game.AddSystem(new WanderSystem());
			game.AddSystem(new HolyPlaceSystem());
			game.AddSystem(new PsychicHunterSystem());
			game.AddSystem(new SvardymSystem());
			game.AddSystem(new ChavvahSystem());
			game.AddSystem(new NorthShevaSystem());
		}
		else if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYEROBJECT)
		{
			GameObject gameObject = (GameObject)element;
			Body body = gameObject.Body;
			if (body != null)
			{
				body.GetPart("Hand").GetRandomElement()?.SetAsPreferredDefault(force: true);
			}
			gameObject.Brain.PerformEquip(Silent: true, DoPrimaryChoice: false);
			gameObject.Brain.DoReequip = false;
		}
		else if (id == QudGameBootModule.BOOTEVENT_GAMESTARTING)
		{
			try
			{
				The.Player.SetStringProperty("OriginalPlayerBody", "1");
				Event e = Event.New("GameStart");
				The.Player.FireEvent(e);
				The.Player.Body.FireEventOnBodyparts(e);
				foreach (GameObject item in The.Player.Inventory.GetObjectsDirect())
				{
					item.FireEvent(e);
				}
				string text = info.getData<QudCustomizeCharacterModuleData>()?.pet;
				if (!string.IsNullOrEmpty(text))
				{
					GameObject gameObject2 = GameObject.Create(text);
					Cell playerCell = The.PlayerCell;
					gameObject2.SetIntProperty("NoXP", 1);
					gameObject2.SetAlliedLeader<AllyPet>(The.Player);
					gameObject2.MakeActive();
					gameObject2.Brain?.GoToPartyLeader();
					if (gameObject2.CurrentCell == null)
					{
						Cell targetCell = playerCell.GetFirstEmptyAdjacentCell(1, 1) ?? playerCell.GetFirstEmptyAdjacentCell(1, 2) ?? playerCell.GetFirstEmptyAdjacentCell(1, 3) ?? playerCell.GetFirstEmptyAdjacentCell(1, 4) ?? playerCell.GetRandomLocalAdjacentCell() ?? playerCell;
						gameObject2.SystemLongDistanceMoveTo(targetCell);
					}
				}
			}
			catch (Exception ex)
			{
				MetricsManager.LogError("Error spawning pet: " + ex.ToString());
			}
			GameObject player = The.Player;
			if (player != null && player.GetPart<Mutations>()?.MutationList?.Count >= 10)
			{
				Achievement.HAVE_10_MUTATIONS.Unlock();
			}
			Popup.Show("You embark for the caves of Qud.", null, "Sounds/Misc/sfx_newLocation_discovered");
		}
		return base.handleBootEvent(id, game, info, element);
	}
}
