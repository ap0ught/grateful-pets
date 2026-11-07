namespace Genkit.SimplexNoise;

public class ISample
{
	public virtual SamplePoint Sample(float x, float y, float z)
	{
		return new SamplePoint(x, y, z);
	}

	public virtual SamplePoint Sample(float x, float y)
	{
		return new SamplePoint(x, y);
	}

	public virtual SamplePoint Sample(float x)
	{
		return new SamplePoint(x);
	}
}
