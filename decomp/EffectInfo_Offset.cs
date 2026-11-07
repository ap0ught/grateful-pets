using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EffectInfo_Offset : EffectInfo_Base
{
	[Serializable]
	public class PropInfo
	{
		public EffectEventType type;

		public Vector2 val;
	}

	public exSpriteBase target;

	public Vector2 normal = Vector2.one;

	public List<PropInfo> propInfos = new List<PropInfo>();

	public Vector2 GetValue(EffectEventType _type)
	{
		for (int i = 0; i < propInfos.Count; i++)
		{
			PropInfo propInfo = propInfos[i];
			if (propInfo.type == _type)
			{
				return propInfo.val;
			}
		}
		return normal;
	}
}
