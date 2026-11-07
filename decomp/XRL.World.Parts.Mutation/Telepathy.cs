using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Telepathy : BaseMutation
{
	public static readonly string COMMAND_NAME = "CommandTelepathy";

	public bool RealityDistortionBased;

	public Telepathy()
	{
		Type = "Mental";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<CommandEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			if (RealityDistortionBased && !ParentObject.IsRealityDistortionUsable())
			{
				RealityStabilized.ShowGenericInterdictMessage(ParentObject);
				return false;
			}
			SoundManager.PreloadClipSet("Sounds/Abilities/sfx_ability_telepathy");
			Cell cell = PickDestinationCell(80, AllowVis.OnlyExplored, Locked: true, IgnoreSolid: false, IgnoreLOS: false, RequireCombat: true, PickTarget.PickStyle.EmptyCell, "Telepathy");
			if (cell != null)
			{
				ParentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_telepathy");
				cell.PlayWorldSound("Sounds/Abilities/sfx_ability_telepathy");
				if (RealityDistortionBased)
				{
					Event e = Event.New("InitiateRealityDistortionTransit", "Object", ParentObject, "Mutation", this, "Cell", cell);
					if (!ParentObject.FireEvent(e, E) || !cell.FireEvent(e, E))
					{
						return false;
					}
				}
				cell.ForeachObjectWithPart("ConversationScript", (GameObject GO) =>
				{
					GO.GetPart<ConversationScript>().AttemptConversation(Silent: false, true);
				});
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override string GetDescription()
	{
		string text = "";
		text += "You may communicate with others through the psychic aether.\n\n";
		text += "Chat with anyone in vision\nTakes you much less time to issue orders to companions";
		if (Options.AnySifrah)
		{
			text += "\nUseful in many social and psionic Sifrah games.";
		}
		return text;
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Telepathy", COMMAND_NAME, "Mental Mutations", null, "\u000e", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, RealityDistortionBased);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
