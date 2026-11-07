using UnityEngine;

namespace Genkit;

public class WaveTemplateEntry
{
	public string name;

	public Color32[] pixels;

	public int width;

	public int height;

	public override string ToString()
	{
		return name + " " + width + "x" + height;
	}
}
