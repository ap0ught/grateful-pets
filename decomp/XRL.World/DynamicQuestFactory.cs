using System;

namespace XRL.World;

public static class DynamicQuestFactory
{
	public static BaseDynamicQuestTemplate fabricateQuestTemplate(string type, DynamicQuestContext context)
	{
		Type type2 = ModManager.ResolveType(type);
		if (type2 == null)
		{
			throw new ArgumentException("couldn't resolve quest type " + type);
		}
		return fabricateQuestTemplate(type2, context);
	}

	public static BaseDynamicQuestTemplate fabricateQuestTemplate(Type type, DynamicQuestContext context)
	{
		BaseDynamicQuestTemplate obj = (Activator.CreateInstance(type) as BaseDynamicQuestTemplate) ?? throw new ArgumentException("type should be a BaseDynamicQuestTemplate derived type");
		obj.id = DynamicQuestsGameState.Instance.NextID++;
		obj.init(context);
		return obj;
	}
}
