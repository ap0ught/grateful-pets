namespace Genkit.SimplexNoise;

public class NoiseLayer
{
	public int Seed;

	public Noise LayerNoise;

	public float OctaveStrength = 2f;

	public ISample SampleFunction;

	public NoiseLayer(int _Seed)
	{
		Seed = _Seed;
		LayerNoise = new Noise(Seed);
		SampleFunction = new LinearSampler(0.01f);
	}

	public float Generate(float x)
	{
		SamplePoint samplePoint = SampleFunction.Sample(x);
		return LayerNoise.Generate(samplePoint.x);
	}

	public float Generate(float x, float y)
	{
		SamplePoint samplePoint = SampleFunction.Sample(x, y);
		return LayerNoise.Generate(samplePoint.x, samplePoint.y);
	}

	public float Generate(float x, float y, float z)
	{
		SamplePoint samplePoint = SampleFunction.Sample(x, y, z);
		return LayerNoise.Generate(samplePoint.x, samplePoint.y, samplePoint.z);
	}
}
