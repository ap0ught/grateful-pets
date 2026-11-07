using System.Text;

namespace Kobold;

public class ImportDefinition
{
	public string Spec;

	public AtlasDefinition DiffuseAtlas;

	public AtlasDefinition NormalAtlas;

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Spec = " + Spec);
		if (DiffuseAtlas != null)
		{
			stringBuilder.Append("\ndiffuse: " + DiffuseAtlas.ToString());
		}
		if (NormalAtlas != null)
		{
			stringBuilder.Append("\nnormal: " + NormalAtlas.ToString());
		}
		return stringBuilder.ToString();
	}
}
