using System;
using System.Collections.Generic;
using System.Linq;
using Qud.API;
using XRL.Core;
using XRL.Rules;

namespace XRL.World;

[Serializable]
[GameStateSingleton]
public class KithAndKinGameState : IGameStateSingleton, IComposite
{
	public static string HINT_CATEGORY = "Kith and Kin";

	public List<HindrenClueLook> lookClues = new List<HindrenClueLook>();

	public List<HindrenClueRumor> rumors = new List<HindrenClueRumor>();

	public List<string> clueItems = new List<string>();

	public static KithAndKinGameState Instance
	{
		get
		{
			if (XRLCore.Core == null)
			{
				return null;
			}
			if (XRLCore.Core.Game == null)
			{
				return null;
			}
			return XRLCore.Core.Game.GetObjectGameState("KithAndKinGameState") as KithAndKinGameState;
		}
	}

	public bool WantFieldReflection => false;

	public void Write(SerializationWriter Writer)
	{
		Writer.WriteComposite(lookClues);
		Writer.WriteComposite(rumors);
		Writer.Write(clueItems);
	}

	public void Read(SerializationReader Reader)
	{
		lookClues = Reader.ReadCompositeList<HindrenClueLook>();
		rumors = Reader.ReadCompositeList<HindrenClueRumor>();
		clueItems = Reader.ReadStringList();
	}

	public List<JournalObservation> getKnownFreeClues()
	{
		return JournalAPI.Observations.FindAll((JournalObservation o) => o.Revealed && o.Has("hindrenclue") && o.Has("free"));
	}

	public List<JournalObservation> getKnownMotiveClues()
	{
		return JournalAPI.Observations.FindAll((JournalObservation o) => o.Revealed && o.Has("hindrenclue") && o.Attributes.Any((string s) => s.StartsWith("motive:")));
	}

	public List<string> getKnownPotentialCircumstances(string thief)
	{
		List<string> list = new List<string>();
		foreach (JournalObservation knownFreeClue in getKnownFreeClues())
		{
			list.Add(knownFreeClue.Text);
		}
		return list;
	}

	public List<string> getKnownPotentialThieves()
	{
		List<JournalObservation> list = JournalAPI.Observations.FindAll((JournalObservation o) => o.Revealed && o.Has("hindrenclue") && o.Attributes.Any((string s) => s.StartsWith("motive:")));
		List<string> list2 = new List<string>();
		foreach (JournalObservation item in list)
		{
			foreach (string attribute in item.Attributes)
			{
				if (attribute.StartsWith("motive:"))
				{
					string text = attribute.Split(':')[1];
					if (!list2.Contains(text))
					{
						list2.Add(text + " [" + item.Text + "]");
					}
				}
			}
		}
		return list2;
	}

	public void foundClue()
	{
		XRLCore.Core.Game.SetIntGameState("FoundClue", 1);
		if (!XRLCore.Core.Game.HasGameState("FoundAccusationClueSet") && getKnownFreeClues().Count > 0 && getKnownMotiveClues().Count > 0)
		{
			XRLCore.Core.Game.SetIntGameState("FoundAccusationClueSet", 1);
		}
		if (XRLCore.Core.Game.HasQuest("Kith and Kin") && !JournalAPI.Observations.Any((JournalObservation o) => !o.Revealed && o.Has("hindrenclue")))
		{
			XRLCore.Core.Game.FinishQuestStep("Kith and Kin", "Discover all the evidence");
		}
	}

	public HindrenClueRumor getRumorForVillagerCategory(string villagerCategory)
	{
		if (rumors.Any((HindrenClueRumor r) => r.villagerCategory == villagerCategory))
		{
			List<HindrenClueRumor> list = rumors.Where((HindrenClueRumor r) => r.villagerCategory == villagerCategory).ToList();
			if (list.Count == 0)
			{
				return null;
			}
			int index = Stat.Random(0, list.Count - 1);
			HindrenClueRumor hindrenClueRumor = list[index];
			rumors.Remove(hindrenClueRumor);
			return hindrenClueRumor;
		}
		return null;
	}

	public void initRumorClue(string journaltext, string conversationText, string villagerCategory, string secret_id, string influence, string evidenceType)
	{
		JournalAPI.AddObservation(journaltext, secret_id, HINT_CATEGORY, secret_id, new string[4]
		{
			"hindrenclue",
			influence,
			evidenceType,
			"influence:" + influence
		}, revealed: false, -1L, null, initCapAsFragment: false, Tradable: false);
		HindrenClueRumor hindrenClueRumor = new HindrenClueRumor();
		hindrenClueRumor.text = conversationText;
		hindrenClueRumor.secret = secret_id;
		hindrenClueRumor.villagerCategory = villagerCategory;
		rumors.Add(hindrenClueRumor);
	}

	public void initLookClue(string text, string revealText, string lookText, string blueprint, string influence, string evidenceType)
	{
		JournalAPI.AddObservation(text, blueprint, HINT_CATEGORY, blueprint, new string[4]
		{
			"hindrenclue",
			influence,
			evidenceType,
			"influence:" + influence
		}, revealed: false, -1L, revealText, initCapAsFragment: false, Tradable: false);
		HindrenClueLook item = new HindrenClueLook(blueprint, lookText);
		lookClues.Add(item);
	}

	public void initItemClue(string text, string revealText, string blueprint, string influence, string evidenceType)
	{
		JournalAPI.AddObservation(text, blueprint, HINT_CATEGORY, blueprint, new string[4]
		{
			"hindrenclue",
			influence,
			evidenceType,
			"influence:" + influence
		}, revealed: false, -1L, revealText, initCapAsFragment: false, Tradable: false);
		clueItems.Add(blueprint);
	}

	public Zone getBeyLahZone()
	{
		return ZoneManager.instance.GetZone(XRLCore.Core.Game.GetStringGameState("BeyLahZoneID"));
	}

	public void Initialize()
	{
		List<string> potentialRumorHavers = new List<string> { "*villager", "*faundren", "*scout" };
		List<Action> list = new List<Action>
		{
			() =>
			{
				initItemClue("I found a bloody patch of fur near the village paths, a sign of recent violence.", "Is that a patch of hindren fur? There's blood on it...  ", "Clue_BloodyPatchOfFur", "violence", "free");
			},
			() =>
			{
				initItemClue("I discovered a patch of watervine stained red with blood, a sign of recent violence.", "How odd, this watervine is stained red with blood.  ", "Clue_BloodyWatervine", "violence", "free");
			},
			() =>
			{
				initItemClue("I found a weapon haft, broken from use, on the paths, a sign of recent violence.", "How odd, the broken haft of a weapon, a sign of recent violence.  ", "Clue_SplinteredWeaponHaft", "violence", "free");
			},
			() =>
			{
				initItemClue("Someone left a sealed merchant's waterskin on the paths, a sign of recent trade.", "Hmm, this waterskin is still sealed with wax.  ", "Clue_BulgingWaterskin", "trade", "free");
			},
			() =>
			{
				initItemClue("Someone dropped a copper nugget on the paths, a sign of recent trade.", "How odd, there's a copper nugget just lying on the ground.  ", "Clue_CopperNugget", "trade", "free");
			},
			() =>
			{
				initItemClue("I discovered a footprint from a great saltback on the paths, a sign of recent trade.", "Now that's a huge footprint.  ", "Clue_GreatSaltbackPrint", "trade", "free");
			},
			() =>
			{
				initItemClue("I found a detached tongue on the ground, rotted with infection: a sign of recent illness.", "Oh gross, there's a rotten tongue on the ground!  ", "Clue_SeveredTongue", "illness", "free");
			},
			() =>
			{
				initItemClue("I found the discarded husks of the curative yuckwheat plant, a sign of recent illness.", "How odd, some discarded chaff from a yuckwheat plant.  ", "Clue_YuckwheatChaff", "illness", "free");
			},
			() =>
			{
				initItemClue("I found a few scraps from some kind of leatherworking project, a sign of recent craft.", "How odd, someone dropped some scraps of cut leather here.  ", "Clue_LeatherScraps", "craft", "free");
			},
			() =>
			{
				initItemClue("Someone discarded a worn but still functional leather bracer on the paths, a sign of recent craft.", "This leather bracer has seen some use. Why is it out here?  ", "Clue_DiscardedBracer", "craft", "free");
			},
			() =>
			{
				initItemClue("I found a small leatherworking hammer on the paths, a sign of recent craft.", "Someone has left a small hammer on the path.  ", "Clue_LeatherworkingHammer", "craft", "free");
			},
			() =>
			{
				initRumorClue("One villager was frightened by sounds of fighting at night, indicating recent violence.", "I heard the sounds of fighting the other night! I stayed in the next day, so afeared was I.", "*villager", "Clue_HeardFighting", "violence", "free");
			},
			() =>
			{
				initRumorClue("One of Bey Lah's scouts heard a fight in the watervine paddies, indicating recent violence.", "I thought I heard splashing from a fight in the marsh grids, but I couldn't find anyone.", "*scout", "Clue_HeardLoudSplashing", "violence", "free");
			},
			() =>
			{
				initRumorClue("Faundren saw a merchant from outside the village sneaking around, indicating recent trade.", "I saw a kendren merchant the other day! They were being very sneaky, so I think they were meeting someone in secret! Isn't that interesting?", "*faundren", "Clue_SawATrader", "trade", "free");
			},
			() =>
			{
				initRumorClue("The faundren heard the voice of a hindren and a kendren discussing goods late at night, indicating recent trade.", "I heard a hindren talking about buying and selling to a kendren late at night! I thought that all kendren had to report to Grand-Doe?", "*faundren", "Clue_TalkingLateAtNight", "trade", "free");
			},
			() =>
			{
				initRumorClue("The villagers are on edge due to rumors of sore throats in the village, indicating recent illness.", "I heard that someone in the village might have a sore throat, so I am staying at least ten handspan away from everyone today.", "*villager", "Clue_SoreThroats", "illness", "free");
			},
			() =>
			{
				initRumorClue("The faundren are complaining about the smell of yuckwheat tea, indicating recent illness.", "I smelled yuckwheat tea the other day! That smell is so terrible! Is someone sick?", "*faundren", "Clue_YuckwheatStench", "illness", "free");
			},
			() =>
			{
				initRumorClue("The scouts report that someone has taken leather from the village stores, indicating recent craft.", "I went to do some repairs to my boots, but someone's been in our stores of boar leather! We have far less than we should.", "*scout", "Clue_LeatherGoneMissing", "craft", "free");
			},
			() =>
			{
				initRumorClue("The scouts heard someone doing work with tools late at night, indicating recent craft.", "I heard someone doing some kind of toolwork during a night watch shift. I didn't interrupt, but it seemed peculiar.", "*scout", "Clue_SoundOfToolsAtNight", "craft", "free");
			}
		};
		List<Action> list2 = new List<Action>
		{
			() =>
			{
				initItemClue("I found an empty injector near the Hindriarch's bedroll. Why doesn't anyone know she was sick?", "Hmm. This injector has been used. ", "Clue_EmptyInjector", "illness", "motive:keh");
			},
			() =>
			{
				initItemClue("I found a bloody arrow on the paths of Bey Lah. Its fletching resembles the ones in Hindriarch Keh's quiver.", "Why, this arrow is covered with blood!  ", "Clue_BloodyArrow", "violence", "motive:keh");
			},
			() =>
			{
				initRumorClue("Apparently, the Hindriarch isolated herself to work on a mystery project, possibly to hide Kindrish.", "You know, Grand-Doe spent some late nights working on something she wouldn't talk about. I wonder what that was.", potentialRumorHavers.RemoveRandomElement(), Guid.NewGuid().ToString(), "craft", "motive:keh");
			},
			() =>
			{
				initRumorClue("Rumor has it that the village is flush with water, perhaps because Keh made a big trade.", "By the Eaters, how prosperous we've been. I've never seen the village so flush with fresh water.", "*villager", Guid.NewGuid().ToString(), "trade", "motive:keh");
			}
		};
		List<Action> list3 = new List<Action>
		{
			() =>
			{
				initRumorClue("The faundren claim that Eskhind left the village sporting a poorly-made leather bracer, possibly to conceal Kindrish.", "Just before auntie Eskhind left the village, I saw her wearing a new bracer. It looked terrible!", "*faundren", Guid.NewGuid().ToString(), "craft", "motive:esk");
			},
			() =>
			{
				initRumorClue("The faundren tell me that Eskhind's sister had a period of complete silence; she may have been ill.", "You know auntie Eskhind's little sister, Liihart? She's always been quiet, but she didn't talk at all for, uh, two whole weeks before they left!", "*faundren", Guid.NewGuid().ToString(), "illness", "motive:esk");
			},
			() =>
			{
				initItemClue("I found the village stores of Bey Lah untouched; Eskhind must have traded something for her basics.", "Whoever stole Kindrish didn't steal from the village stores. These chests are simply packed with dried lah, vinewafers, and waterskins.", "Clue_VillageStores", "trade", "motive:esk");
			},
			() =>
			{
				initLookClue("Eskhind is showing signs of a leg injury. Perhaps she was in a fight. ", "Eskhind appears to be favoring a leg, though she has concealed any injury. ", "Eskhind is favoring a leg.", "Eskhind", "violence", "motive:esk");
			}
		};
		List<Action> list4 = new List<Action>
		{
			() =>
			{
				initRumorClue("The faundren tell me that Kesehind went quiet for a few weeks, only to become louder than ever again.", "For weeks and weeks, Kesehind was so quiet, but now he's talking more than ever! He's so loud!", "*faundren", Guid.NewGuid().ToString(), "illness", "motive:kese");
			},
			() =>
			{
				initRumorClue("Rumor has it that Kesehind has always worn hand-me-down armor, but he was recently seen with a brand new set.", "Did you see Kesehind's armor? He always used to wear hand-me-downs from the scouts, but what he's wearing now looks brand new!", potentialRumorHavers.RemoveRandomElement(), Guid.NewGuid().ToString(), "trade", "motive:kese");
			},
			() =>
			{
				initItemClue("I found a worn-out bracer abandoned near Kesehind's bedroll.", "That's strange. This bracer is heavily worn, but surprisingly clean.  ", "Clue_WornBracer", "craft", "motive:kese");
			},
			() =>
			{
				initLookClue("Kesehind tried to clean blood off his armor, but missed a spot. ", "Carmine streaks on Kesehind's armor suggest that he tried and failed to wipe away spilled blood. ", "Kesehind's armor has bloodstains that he failed to remove. ", "Kesehind", "violence", "motive:kese");
			}
		};
		List<Action> list5 = new List<Action>
		{
			() =>
			{
				initRumorClue("The Bey Lah scouts have had trouble with violent kendren. What if one slipped by?", "The kendren are becoming more brazen in their hostility. I've had to misdirect and fight them off several times this month alone.", "*scout", Guid.NewGuid().ToString(), "violence", "motive:kendren");
			},
			() =>
			{
				initRumorClue("The scouts have seen kendren treasure hunters about. That type will take anything not nailed down.", "I came across some kendren who claimed to be treasure hunters. They said that they would pay good water for unique artifacts or treasures.", "*scout", Guid.NewGuid().ToString(), "trade", "motive:kendren");
			},
			() =>
			{
				initRumorClue("Theft by kendren is very common during an outbreak of disease like the one the villagers describe.", "I've heard that there is an outbreak of glotrot in the kendren villages outside of Bey Lah. Now I want to leave here even less!", "*villager", Guid.NewGuid().ToString(), "illness", "motive:kendren");
			},
			() =>
			{
				initItemClue("I found a set of foreign leatherworking tools on the village paths. A kendren crafter has been here.", "This set of leatherworking tools is shockingly well-made.  ", "Clue_ForeignLeatherworkingTools", "craft", "motive:kendren");
			}
		};
		list.ShuffleInPlace();
		int num = 4;
		for (int num2 = 0; num2 < num; num2++)
		{
			list[num2]();
		}
		list2.GetRandomElement()();
		list3.GetRandomElement()();
		list4.GetRandomElement()();
		list5.GetRandomElement()();
	}
}
