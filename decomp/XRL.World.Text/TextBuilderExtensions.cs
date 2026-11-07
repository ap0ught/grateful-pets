namespace XRL.World.Text;

public static class TextBuilderExtensions
{
	public static void AppendTo(this string String, TextBuilder Builder)
	{
		Builder.Append(String);
	}

	public static void CompoundTo(this string String, TextBuilder Builder)
	{
		Builder.Compound(String);
	}

	public static void CompoundTo(this string String, TextBuilder Builder, string With)
	{
		Builder.Compound(String, With);
	}
}
