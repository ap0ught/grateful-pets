using System;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World;

[Serializable]
public class InteractWithAnObjectDynamicQuestTemplate : BaseDynamicQuestTemplate
{
	public override void init(DynamicQuestContext context)
	{
		Array values = Enum.GetValues(typeof(QuestStoryType_InteractWithAnObject));
		QuestStoryType_InteractWithAnObject questStoryType_InteractWithAnObject = (QuestStoryType_InteractWithAnObject)values.GetValue(Stat.Random(0, values.Length - 1));
		GameObject gameObject = ((questStoryType_InteractWithAnObject != QuestStoryType_InteractWithAnObject.StrangePlan) ? context.getQuestRemoteInteractable() : context.getQuestGenericRemoteInteractable());
		gameObject.AddPart(new DynamicQuestTarget());
		gameObject.AddPart(new InteractQuestTarget
		{
			EventID = gameObject.GetStringProperty("QuestEvent")
		});
		gameObject.SetIntProperty("NoAIEquip", 1);
		if (!gameObject.HasPart<Shrine>())
		{
			gameObject.GiveProperName(context.getQuestItemNameMutation(gameObject.Render.DisplayName), Force: true);
			gameObject.SetStringProperty("HasPregeneratedName", "yes");
			gameObject.SetStringProperty("DefiniteArticle", "the");
			gameObject.SetStringProperty("IndefiniteArticle", "the");
		}
		gameObject.RequirePart<Interesting>().Radius = 1;
		if (gameObject.IsTakeable())
		{
			gameObject.RequirePart<RemoveInterestingOnTake>();
		}
		gameObject.FireEvent("SpecialInit");
		string text = zoneManager.CacheObject(gameObject);
		DynamicQuestDeliveryTarget questDeliveryTarget = context.getQuestDeliveryTarget();
		zoneManager.AddZoneBuilder(context.getQuestGiverZone(), 6000, "InteractWithAnObjectDynamicQuestTemplate_FabricateQuestGiver", "questContext", context, "QST", questStoryType_InteractWithAnObject, "deliveryItemID", text, "deliveryTarget", questDeliveryTarget);
		zoneManager.AddZoneBuilder(questDeliveryTarget.zoneId, 6000, "InteractWithAnObjectDynamicQuestTemplate_FabricateQuestItem", "deliveryItemID", text);
	}
}
