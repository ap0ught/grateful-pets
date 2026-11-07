namespace Genkit.SimplexNoise;

public class LinearSampler : ISample
{
	public SamplePoint SampleStride = new SamplePoint(0.01f, 0.01f, 0.01f);

	public LinearSampler(float Scale)
	{
		SampleStride = new SamplePoint(Scale, Scale, Scale);
	}

	public override SamplePoint Sample(float x, float y, float z)
	{
		return new SamplePoint(x * SampleStride.x, y * SampleStride.y, z * SampleStride.z);
	}

	public override SamplePoint Sample(float x, float y)
	{
		return new SamplePoint(x * SampleStride.x, y * SampleStride.y);
	}

	public override SamplePoint Sample(float x)
	{
		return new SamplePoint(x * SampleStride.x);
	}
}
