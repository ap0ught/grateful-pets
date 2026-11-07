using Qud.API;

namespace XRL.World.ZoneBuilders;

public static class ChildrenOfTheTombQuestHandler
{
	public const int NACHAM = 1;

	public const int VAAM = 2;

	public const int DAGASHA = 4;

	public const int KAH = 8;

	public static void ChooseNacham()
	{
		The.Game.SetIntGameState("ChoseNacham", 1);
		Achievement.STAT_GAVE_REPULSIVE_DEVICE_NACHAM.Increment();
		AddAccomplishment("Nacham");
	}

	public static void ChooseVaam()
	{
		The.Game.SetIntGameState("ChoseVaam", 1);
		Achievement.STAT_GAVE_REPULSIVE_DEVICE_VAAM.Increment();
		AddAccomplishment("Vaam");
	}

	public static void ChooseDagasha()
	{
		The.Game.SetIntGameState("ChoseDagasha", 1);
		Achievement.STAT_GAVE_REPULSIVE_DEVICE_DAGASHA.Increment();
		AddAccomplishment("Dagasha");
	}

	public static void ChooseKah()
	{
		The.Game.SetIntGameState("ChoseKah", 1);
		Achievement.STAT_GAVE_REPULSIVE_DEVICE_KAH.Increment();
		AddAccomplishment("Kah");
	}

	public static void FinishFrayingFavorites()
	{
		The.Game.SetIntGameState("FinishedFrayingFavorites", 1);
	}

	public static void AddAccomplishment(string EntityName)
	{
		JournalAPI.AddAccomplishment("You restored functionality to one of the four strange entities in the Tomb.", "Remember the kindness of =name=, who absolved =player.possessive= treacherous brother of sin and steered him to salvation!", "In a moss-caked tomb under the light of the Spindle, =name= cemented their relationship with enigmatic beings by marrying the entity " + EntityName + ".", null, "general", MuralCategory.DoesSomethingHumble, MuralWeight.Medium, null, -1L);
	}
}
