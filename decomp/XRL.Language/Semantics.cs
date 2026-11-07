using XRL.World;

namespace XRL.Language;

public static class Semantics
{
	public static string GetSingularSemantic(string name, GameObject go, string defaultResult)
	{
		return go.GetTagOrStringProperty("Semantic" + name, defaultResult);
	}

	public static string GetPluralSemantic(string name, GameObject go, string defaultResult)
	{
		return Grammar.Pluralize(GetSingularSemantic(name, go, defaultResult));
	}
}
