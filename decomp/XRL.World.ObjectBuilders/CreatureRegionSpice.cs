using System;
using System.Collections.Generic;
using HistoryKit;
using Newtonsoft.Json.Linq;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ObjectBuilders;

public class CreatureRegionSpice : IObjectBuilder
{
	public string TileColor;

	public string DetailColor;

	public override void Initialize()
	{
		TileColor = null;
		DetailColor = null;
	}

	public override void Apply(GameObject Object, string Context)
	{
		if (Object.Render == null)
		{
			return;
		}
		string text = ZoneManager.ZoneGenerationContext?.GetTerrainRegion();
		if (text.IsNullOrEmpty())
		{
			return;
		}
		Render render = Object.Render;
		JToken jToken = HistoricSpice.root["history"]["regions"]["terrain"][text];
		JArray jArray = (JArray)jToken["creatureRegionAdjective"];
		JArray jArray2 = (JArray)jToken["creatureRegionNoun"];
		JArray jArray3 = (JArray)jToken["creatureAlteredLocale"];
		JArray jArray4 = (JArray)jToken["creatureAlteredCast"];
		Random seededRandomGenerator = Stat.GetSeededRandomGenerator(Object.Blueprint);
		string text2 = (string?)jArray[seededRandomGenerator.Next(jArray.Count)];
		string text3 = (string?)jArray2[seededRandomGenerator.Next(jArray2.Count)];
		render.DisplayName = render.DisplayName.StartReplace().AddObject(Object).AddReplacer("creatureRegionAdjective", text2)
			.AddReplacer("creatureRegionNoun", text3)
			.ToString();
		if (Object.TryGetPart<Description>(out var Part))
		{
			string text4 = (string?)jArray3[seededRandomGenerator.Next(jArray3.Count)];
			string text5 = (string?)jArray4[seededRandomGenerator.Next(jArray4.Count)];
			string[] value = Object.GetxTag("TextFragments", "PoeticFeatures").Split(',');
			Part._Short = Part._Short.Replace("=creatureRegionAdjective=", text2).Replace("=creatureRegionNoun=", text3);
			Description description = Part;
			description._Short = description._Short + " Time in " + text4 + " has altered =pronouns.possessive= features -- " + string.Join(", ", value) + " -- and given them " + text5 + ".";
		}
		if (!TileColor.IsNullOrEmpty())
		{
			string text6 = TileColor.CachedCommaExpansion().GetRandomElement(seededRandomGenerator);
			if (text6.Length == 1)
			{
				text6 = "&" + text6;
			}
			render.ColorString = text6;
			if (!render.TileColor.IsNullOrEmpty())
			{
				render.TileColor = render.ColorString;
			}
		}
		else
		{
			JArray jArray5 = (JArray)jToken["baseColor"];
			render.ColorString = "&" + jArray5[seededRandomGenerator.Next(jArray5.Count)];
			if (!render.TileColor.IsNullOrEmpty())
			{
				render.TileColor = render.ColorString;
			}
		}
		if (!DetailColor.IsNullOrEmpty())
		{
			List<string> list = DetailColor.CachedCommaExpansion();
			char tileForegroundColorChar = Object.Render.GetTileForegroundColorChar();
			int num = 0;
			do
			{
				render.DetailColor = list.GetRandomElement(seededRandomGenerator);
			}
			while (tileForegroundColorChar == render.getDetailColor() && num++ < list.Count);
		}
	}
}
