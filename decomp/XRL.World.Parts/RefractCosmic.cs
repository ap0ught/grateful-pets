using System;
using Genkit;
using XRL.Collections;

namespace XRL.World.Parts;

[Serializable]
public class RefractCosmic : IPart
{
	[NonSerialized]
	private Rack<float> Directions = new Rack<float>();

	public ReadOnlySpan<float> GetDirections(Location2D Source, int Amount = 3)
	{
		Cell cell = ParentObject.CurrentCell;
		uint hash = Hash.FNV1A32(Source.X | (Source.Y << 16));
		hash = Hash.FNV1A32(cell.X | (cell.Y << 16), hash);
		Random random = new Random((int)hash);
		float[] array = Directions.GetArray(Amount);
		for (int i = 0; i < Amount; i++)
		{
			array[i] = (float)(random.NextDouble() * 360.0);
		}
		return array.AsSpan(0, Amount);
	}
}
