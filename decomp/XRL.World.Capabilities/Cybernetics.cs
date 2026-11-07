using System.Text;
using XRL.UI;
using XRL.Wish;
using XRL.World.Anatomy;
using XRL.World.Units;

namespace XRL.World.Capabilities;

[HasWishCommand]
public static class Cybernetics
{
	public static StringBuilder GetCreationDetails(GameObjectBlueprint cyber)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (cyber != null)
		{
			string tag = cyber.GetTag("CyberneticsCreationDetails");
			if (!string.IsNullOrEmpty(tag))
			{
				stringBuilder.Append(tag);
			}
			else
			{
				stringBuilder.Append(cyber.GetPartParameter<string>("Description", "Short"));
				tag = cyber.GetPartParameter<string>("CyberneticsBaseItem", "BehaviorDescription");
				if (!string.IsNullOrEmpty(tag))
				{
					stringBuilder.Append("\n\n{{rules|").Append(tag).Append("}}");
				}
			}
			stringBuilder.Append("\n\n");
			if (cyber.HasTag("CyberneticsDestroyOnRemoval"))
			{
				stringBuilder.Append("{{rules|Destroyed when uninstalled}}\n");
			}
			stringBuilder.Append("{{rules|License points: ").Append(cyber.GetPartParameter("CyberneticsBaseItem", "Cost", 0)).Append("}}");
		}
		else
		{
			stringBuilder.Append("{{rules|+1 Toughness}}\n\n").Append("{{R|-2 License Tier (down to 0)}}");
		}
		return stringBuilder;
	}

	[WishCommand("implant", null)]
	private static void WishImplant(string Argument)
	{
		Argument.Split(':', out var name, out var slot);
		GameObjectBlueprint gameObjectBlueprint = GameObjectFactory.Factory.BlueprintList.Find((GameObjectBlueprint x) => x.Name.EqualsNoCase(name)) ?? GameObjectFactory.Factory.BlueprintList.Find((GameObjectBlueprint x) => x.CachedDisplayNameStripped.EqualsNoCase(name));
		if (gameObjectBlueprint == null)
		{
			Popup.ShowFail("No blueprint by the name '" + name + "' could be found.");
			return;
		}
		if (!gameObjectBlueprint.HasPart("CyberneticsBaseItem"))
		{
			Popup.ShowFail("The blueprint '" + gameObjectBlueprint.Name + "' is not a cybernetic.");
			return;
		}
		if (!slot.IsNullOrEmpty())
		{
			BodyPart bodyPart = The.Player.Body.GetFirstPart((BodyPart x) => x.Name.EqualsNoCase(slot)) ?? The.Player.Body.GetFirstPart((BodyPart x) => x.Description.EqualsNoCase(slot)) ?? The.Player.Body.GetFirstPart((BodyPart x) => x.Type.EqualsNoCase(slot));
			if (bodyPart == null)
			{
				Popup.ShowFail("No body part by the name '" + slot + "' could be found.");
				return;
			}
			if (bodyPart.Cybernetics != null)
			{
				bodyPart.Unimplant(MoveToInventory: false);
			}
			slot = bodyPart.Name;
		}
		GameObject gameObject = new GameObjectCyberneticsUnit
		{
			Blueprint = gameObjectBlueprint.Name,
			Slot = slot
		}.Implant(The.Player);
		if (gameObject != null)
		{
			BodyPart bodyPart2 = The.Player.FindCybernetics(gameObject);
			Popup.Show("Your " + bodyPart2.GetOrdinalName() + " " + (bodyPart2.Plural ? "are" : "is") + " implanted with " + gameObject.DisplayName + "!");
		}
	}
}
