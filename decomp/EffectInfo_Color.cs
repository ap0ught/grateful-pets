using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EffectInfo_Color : EffectInfo_Base
{
	[Serializable]
	public class PropInfo
	{
		public EffectEventType type;

		public Color val;
	}

	public exSpriteBase target;

	public Color normal = Color.white;

	public List<PropInfo> propInfos = new List<PropInfo>();

	public Color GetValue(EffectEventType _type)
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
