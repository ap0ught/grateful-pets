using System;
using XRL.Wish;
using XRL.World.ZoneBuilders;

namespace XRL.World.Quests;

[Serializable]
[HasWishCommand]
public class FrayingFavoritesSystem : IQuestSystem
{
	public override void Register(XRLGame Game, IEventRegistrar Registrar)
	{
		Registrar.Register(AfterConversationEvent.ID);
	}

	public override bool HandleEvent(AfterConversationEvent E)
	{
		CheckChatted();
		return base.HandleEvent(E);
	}

	public void CheckChatted()
	{
		if (Player.HasProperty("DoyobaChat") && Player.HasProperty("DadogomChat") && Player.HasProperty("GyamyoChat") && Player.HasProperty("YonaChat"))
		{
			The.Game.FinishQuestStep("Fraying Favorites", "Speak with the Watchers");
		}
	}

	public override void Start()
	{
		CheckChatted();
		if (The.Game.GetIntGameState("FinishedFrayingFavorites") == 1)
		{
			The.Game.FinishQuest("Fraying Favorites");
		}
	}

	public override GameObject GetInfluencer()
	{
		return GameObject.FindByBlueprint("Lebah");
	}

	[WishCommand("fraying", null)]
	public static void Wish(string Biomech)
	{
		if (Biomech.EqualsNoCase("nacham"))
		{
			ChildrenOfTheTombQuestHandler.ChooseNacham();
		}
		else if (Biomech.EqualsNoCase("vaam") || Biomech.EqualsNoCase("va'am"))
		{
			ChildrenOfTheTombQuestHandler.ChooseVaam();
		}
		else if (Biomech.EqualsNoCase("dagasha"))
		{
			ChildrenOfTheTombQuestHandler.ChooseDagasha();
		}
		else if (Biomech.EqualsNoCase("kah"))
		{
			ChildrenOfTheTombQuestHandler.ChooseKah();
		}
		else
		{
			if (!Biomech.EqualsNoCase("all"))
			{
				return;
			}
			ChildrenOfTheTombQuestHandler.ChooseNacham();
			ChildrenOfTheTombQuestHandler.ChooseVaam();
			ChildrenOfTheTombQuestHandler.ChooseDagasha();
			ChildrenOfTheTombQuestHandler.ChooseKah();
		}
		ChildrenOfTheTombQuestHandler.FinishFrayingFavorites();
		The.Game.CompleteQuest("Fraying Favorites");
	}
}
