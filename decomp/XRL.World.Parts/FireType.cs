using System;

namespace XRL.World.Parts;

[Serializable]
public enum FireType
{
	Normal = 0,
	SuppressingFire = 1,
	SureFire = 2,
	WoundingFire = 3,
	BeaconFire = 4,
	FlatteningFire = 5,
	DisorientingFire = 6,
	OneShot = 7,
	Mark = 10
}
