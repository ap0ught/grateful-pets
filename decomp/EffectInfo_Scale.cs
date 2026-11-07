using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EffectInfo_Scale : EffectInfo_Base
{
	[Serializable]
	public class PropInfo
	{
		public EffectEventType type;

		public Vector3 val;
	}

	public Transform target;

	public Vector3 normal = Vector3.one;

	public List<PropInfo> propInfos = new List<PropInfo>();

	public Vector3 GetValue(EffectEventType _type)
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
