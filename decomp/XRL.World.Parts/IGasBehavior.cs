using System;

namespace XRL.World.Parts;

public abstract class IGasBehavior : IPart
{
	[NonSerialized]
	private Gas _BaseGas;

	public Gas BaseGas => _BaseGas ?? (_BaseGas = ParentObject.GetPart<Gas>());

	public int GasDensity()
	{
		return BaseGas?.Density ?? 0;
	}

	public int GasDensityStepped(int Step = 5)
	{
		return StepValue(GasDensity(), Step);
	}
}
