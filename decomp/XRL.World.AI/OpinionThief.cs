using System.CodeDom.Compiler;
using Occult.Engine.CodeGeneration;

namespace XRL.World.AI;

[GenerateSerializationPartial]
public class OpinionThief : IOpinionObject
{
	public string Item;

	public string ItemArticle;

	public string From;

	public string FromArticle;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override bool WantFieldReflection => false;

	public override int BaseValue => -20;

	public override float Limit => 10f;

	public override int Cooldown => 0;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Item);
		Writer.WriteOptimized(ItemArticle);
		Writer.WriteOptimized(From);
		Writer.WriteOptimized(FromArticle);
		Writer.Write(Magnitude);
		Writer.WriteOptimized(Time);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Read(SerializationReader Reader)
	{
		Item = Reader.ReadOptimizedString();
		ItemArticle = Reader.ReadOptimizedString();
		From = Reader.ReadOptimizedString();
		FromArticle = Reader.ReadOptimizedString();
		Magnitude = Reader.ReadSingle();
		Time = Reader.ReadOptimizedInt64();
	}

	public override void Initialize(GameObject Actor, GameObject Subject, GameObject Object)
	{
		if (Object?.Render != null)
		{
			Item = Object.Render.DisplayName;
			ItemArticle = Object.IndefiniteArticle(Capital: false, null, AsIfKnown: true);
		}
		else
		{
			Item = null;
			ItemArticle = null;
		}
		Zone currentZone = Subject.GetCurrentZone();
		if (currentZone != null && currentZone.HasProperName)
		{
			From = currentZone.BaseDisplayName;
			FromArticle = null;
		}
		else if (Object != null && !Object.Owner.IsNullOrEmpty())
		{
			Faction faction = Factions.Get(Object.Owner);
			From = faction.DisplayName;
			FromArticle = (faction.FormatWithArticle ? "the " : null);
		}
		else
		{
			From = null;
			FromArticle = null;
		}
	}

	public override string GetText(GameObject Actor)
	{
		return "Stole " + ItemArticle + (Item ?? "something") + " from " + FromArticle + (From ?? "us") + ".";
	}
}
