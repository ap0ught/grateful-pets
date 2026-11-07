using System;

namespace XRL.World;

[Serializable]
public enum MissileMapType
{
	Empty,
	Wall,
	VeryLightCover,
	LightCover,
	MediumCover,
	HeavyCover,
	VeryHeavyCover,
	Hostile,
	Friendly
}
