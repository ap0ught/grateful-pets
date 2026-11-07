using System;

namespace XRL.World.Parts;

[Serializable]
public class GenericPowerTransmission : IPowerTransmission
{
	public string Type = "generic";

	public override string GetPowerTransmissionType()
	{
		return Type;
	}
}
