using System;
using Qud.API;
using XRL.Messages;
using XRL.UI;
using XRL.World.AI;

namespace XRL.World.Capabilities;

public static class Cloning
{
	public static bool IsCloning(string ReplicationContext)
	{
		if (!(ReplicationContext == "CloningDraught") && !(ReplicationContext == "Cloneling"))
		{
			return ReplicationContext == "Budding";
		}
		return true;
	}

	public static bool CanBeCloned(GameObject Object, GameObject Actor = null, string Context = null, bool Temporary = false)
	{
		if (Object.HasPropertyOrTag("Noclone"))
		{
			return false;
		}
		if (!Object.IsAlive)
		{
			return false;
		}
		if (!Effect.CanEffectTypeBeAppliedTo(16, Object))
		{
			return false;
		}
		if (!Object.CanBeReplicated(Actor, Context, Temporary))
		{
			return false;
		}
		return true;
	}

	private static void PostprocessClone(GameObject Original, GameObject Clone, GameObject Actor, bool DuplicateGear = false, bool BecomesCompanion = true, bool Budded = false, string Context = null)
	{
		if (!DuplicateGear)
		{
			Clone.StripContents(KeepNatural: true, Silent: true);
		}
		Clone.RestorePristineHealth();
		Clone.HasProperName = false;
		Clone.RemoveIntProperty("Renamed");
		Clone.RemoveStringProperty("OriginalPlayerBody");
		Clone.ModIntProperty("IsClone", 1);
		Clone.SetStringProperty("CloneOf", Original.ID);
		Clone.SetStringProperty("CloneOfGenes", Original.GeneID);
		if (Budded)
		{
			Clone.ModIntProperty("IsBuddedClone", 1);
		}
		if (Clone.Render != null && !Original.HasPropertyOrTag("CloneNoNameChange") && !Original.BaseDisplayName.Contains("clone of"))
		{
			if (Original.HasProperName)
			{
				Clone.Render.DisplayName = "clone of " + Original.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: false, ColorOnly: false, WithoutTitles: true, Short: false, BaseOnly: false, WithIndefiniteArticle: true);
			}
			else
			{
				Clone.Render.DisplayName = Clone.GetBlueprint().DisplayName();
			}
		}
		if (BecomesCompanion)
		{
			Clone.SetAlliedLeader<AllyClone>(Original);
		}
		WasReplicatedEvent.Send(Original, Actor, Clone, Context);
		ReplicaCreatedEvent.Send(Clone, Actor, Original, Context);
	}

	public static GameObject GenerateClone(GameObject Original, GameObject Actor = null, Cell Cell = null, bool DuplicateGear = false, bool BecomesCompanion = true, bool Budded = false, string Context = null, Func<GameObject, GameObject> MapInv = null)
	{
		GameObject gameObject = null;
		try
		{
			Original.FireEvent("BeforeBeingCloned");
			_ = Original.GeneID;
			gameObject = Original.DeepCopy(CopyEffects: false, CopyID: false, MapInv);
		}
		finally
		{
			Original.FireEvent("AfterBeingCloned");
		}
		if (gameObject == null)
		{
			return null;
		}
		PostprocessClone(Original, gameObject, Actor, DuplicateGear, BecomesCompanion, Budded, Context);
		if (Cell != null)
		{
			Cell.AddObject(gameObject);
			gameObject.MakeActive();
			if (!Achievement.CLONES_30.Achieved && Original.IsPlayer())
			{
				QueueAchievementCheck();
			}
		}
		return gameObject;
	}

	public static GameObject GenerateClone(GameObject Original, GameObject Actor = null, bool DuplicateGear = false, bool BecomesCompanion = true, bool Budded = false, string Context = null, int MaxRadius = 1)
	{
		if (Original.CurrentCell != null && !Original.OnWorldMap())
		{
			Cell firstEmptyAdjacentCell = Original.CurrentCell.GetFirstEmptyAdjacentCell(1, MaxRadius);
			if (firstEmptyAdjacentCell != null)
			{
				return GenerateClone(Original, Actor, firstEmptyAdjacentCell, DuplicateGear, BecomesCompanion, Budded, Context);
			}
		}
		return null;
	}

	public static GameObject GenerateBuddedClone(GameObject Original, Cell Cell, GameObject Actor = null, bool DuplicateGear = false, bool BecomesCompanion = true, string Context = "Budding")
	{
		return StartBuddedClone(Original, GenerateClone(Original, Actor, Cell, DuplicateGear, BecomesCompanion, Budded: true, Context));
	}

	public static GameObject GenerateBuddedClone(GameObject Original, GameObject Actor = null, bool DuplicateGear = false, bool BecomesCompanion = true, int MaxRadius = 1, string Context = "Budding")
	{
		return StartBuddedClone(Original, GenerateClone(Original, Actor, DuplicateGear, BecomesCompanion, Budded: true, Context, MaxRadius));
	}

	private static GameObject StartBuddedClone(GameObject Original, GameObject Clone)
	{
		Original.Bloodsplatter();
		if (Original.IsPlayer())
		{
			Popup.Show(Clone.Does("detach", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true) + " from you!");
			JournalAPI.AddAccomplishment("On the " + Calendar.GetDay() + " of " + Calendar.GetMonth() + ", you multiplied.", "In the month of " + Calendar.GetMonth() + " of " + Calendar.GetYear() + " AR, =name= immaculately birthed " + The.Player.GetPronounProvider().Reflexive + ".", "In =year=, while traveling near " + JournalAPI.GetLandmarkNearestPlayer().Text + ", =name= created a simulacrum of " + The.Player.GetPronounProvider().Reflexive + " for the purpose of faking chariot accidents.", null, "general", MuralCategory.WeirdThingHappens, MuralWeight.High, null, -1L);
		}
		else if (Original.IsVisible() || Clone.IsVisible())
		{
			MessageQueue.AddPlayerMessage(Clone.Does("detach", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true) + " from " + Original.t() + "!");
		}
		return Clone;
	}

	public static void QueueAchievementCheck()
	{
		GameManager.Instance.gameQueue.queueSingletonTask("CloningAchievements", CheckAchievements);
	}

	public static void CheckAchievements()
	{
		int num = 0;
		string iD = The.Player.ID;
		Zone currentZone = The.Player.CurrentZone;
		for (int i = 0; i < currentZone.Height; i++)
		{
			for (int j = 0; j < currentZone.Width; j++)
			{
				int k = 0;
				for (int count = currentZone.Map[j][i].Objects.Count; k < count; k++)
				{
					if (currentZone.Map[j][i].Objects[k].Property.TryGetValue("CloneOf", out var value) && value == iD)
					{
						num++;
					}
				}
			}
		}
		if (num >= 10)
		{
			Achievement.CLONES_10.Unlock();
		}
		if (num >= 30)
		{
			Achievement.CLONES_30.Unlock();
		}
	}
}
