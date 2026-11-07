using System.Text;
using UnityEngine;

namespace Kobold;

public class AtlasDefinition
{
	public string Prefix;

	public int XSize;

	public int YSize;

	public string Compression;

	public bool Trim;

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("-atlasdef " + Prefix + " x:" + XSize + " y:" + YSize + " compression:" + Compression);
		return stringBuilder.ToString();
	}

	public exAtlas GenerateAtlas()
	{
		exAtlas exAtlas2 = ScriptableObject.CreateInstance<exAtlas>();
		exAtlas2.width = XSize;
		exAtlas2.height = YSize;
		exAtlas2.useContourBleed = true;
		exAtlas2.usePaddingBleed = true;
		exAtlas2.trimElements = false;
		exAtlas2.trimThreshold = 1000;
		exAtlas2.customPadding = 3;
		return exAtlas2;
	}
}
