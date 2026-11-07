using System.CodeDom.Compiler;
using Occult.Engine.CodeGeneration;

namespace XRL.World.AI;

[GenerateSerializationPartial]
public class OpinionAttack : IOpinionCombat
{
	public string Weapon;

	public string WeaponArticle;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override bool WantFieldReflection => false;

	public override int BaseValue => -75;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Weapon);
		Writer.WriteOptimized(WeaponArticle);
		Writer.Write(Magnitude);
		Writer.WriteOptimized(Time);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Read(SerializationReader Reader)
	{
		Weapon = Reader.ReadOptimizedString();
		WeaponArticle = Reader.ReadOptimizedString();
		Magnitude = Reader.ReadSingle();
		Time = Reader.ReadOptimizedInt64();
	}

	public override void Initialize(GameObject Actor, GameObject Subject, GameObject Object)
	{
		if (Object != null)
		{
			Weapon = Object.BaseDisplayName;
			WeaponArticle = Object.IndefiniteArticle();
		}
		else
		{
			Weapon = null;
			WeaponArticle = null;
		}
		GameObject gameObject = Subject.Brain?.GetFinalLeader();
		if (gameObject != null)
		{
			Actor.Brain.AddOpinion<OpinionCompanionAttack>(gameObject, Object);
			Actor.Brain.GetFinalLeader()?.Brain.AddOpinion<OpinionCompanionAttackAlly>(gameObject, Actor);
		}
	}

	public override string GetText(GameObject Actor)
	{
		if (!Weapon.IsNullOrEmpty())
		{
			return "Attacked me with " + WeaponArticle + Weapon + ".";
		}
		return "Attacked me.";
	}
}
