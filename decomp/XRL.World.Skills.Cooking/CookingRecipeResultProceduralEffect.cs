using System;
using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL.World.Skills.Cooking;

[Serializable]
public class CookingRecipeResultProceduralEffect : ICookingRecipeResult, IComposite
{
	[NonSerialized]
	public ProceduralCookingEffect Effect;

	public bool WantFieldReflection => false;

	public CookingRecipeResultProceduralEffect()
	{
	}

	public CookingRecipeResultProceduralEffect(ProceduralCookingEffect Effect)
	{
		this.Effect = Effect;
	}

	public void Write(SerializationWriter Writer)
	{
		XRL.World.Effect.Save(Effect, Writer);
	}

	public void Read(SerializationReader Reader)
	{
		Effect = (ProceduralCookingEffect)XRL.World.Effect.Load(null, Reader);
	}

	public string GetCampfireDescription()
	{
		return Campfire.ProcessEffectDescription(Effect.GetTemplatedProceduralEffectDescription(), The.Player);
	}

	public string apply(GameObject eater)
	{
		ProceduralCookingEffect proceduralCookingEffect = (ProceduralCookingEffect)Effect.DeepCopy(eater);
		proceduralCookingEffect.Init(eater);
		eater.ApplyEffect(proceduralCookingEffect);
		return proceduralCookingEffect.GetProceduralEffectDescription();
	}
}
