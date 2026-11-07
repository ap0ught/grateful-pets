using System;
using System.CodeDom.Compiler;
using Occult.Engine.CodeGeneration;
using XRL.World;
using XRL.World.Skills.Cooking;

namespace Qud.API;

[Serializable]
[GenerateSerializationPartial]
public class JournalRecipeNote : IBaseJournalEntry
{
	public CookingRecipe Recipe;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override bool WantFieldReflection => false;

	[Obsolete]
	public CookingRecipe recipe
	{
		get
		{
			return Recipe;
		}
		set
		{
			Recipe = value;
		}
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Write(SerializationWriter Writer)
	{
		Writer.Write(Recipe);
		Writer.WriteOptimized(ID);
		Writer.WriteOptimized(History);
		Writer.WriteOptimized(Text);
		Writer.WriteOptimized(LearnedFrom);
		Writer.WriteOptimized(Weight);
		Writer.Write(Revealed);
		Writer.Write(Tradable);
		Writer.Write(Attributes);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Read(SerializationReader Reader)
	{
		Recipe = (CookingRecipe)Reader.ReadComposite();
		ID = Reader.ReadOptimizedString();
		History = Reader.ReadOptimizedString();
		Text = Reader.ReadOptimizedString();
		LearnedFrom = Reader.ReadOptimizedString();
		Weight = Reader.ReadOptimizedInt32();
		Revealed = Reader.ReadBoolean();
		Tradable = Reader.ReadBoolean();
		Attributes = Reader.ReadList<string>();
	}

	public override void Reveal(string LearnedFrom = null, bool Silent = false)
	{
		if (!Revealed)
		{
			base.Reveal(LearnedFrom, Silent);
			CookingGameState.instance.knownRecipies.Add(Recipe);
			if (!Silent)
			{
				IBaseJournalEntry.DisplayMessage("You learn to cook " + Recipe.GetDisplayName() + "!", "sfx_cookingRecipe_learn");
			}
		}
	}
}
