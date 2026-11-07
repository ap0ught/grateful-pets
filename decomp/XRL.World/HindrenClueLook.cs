using System;
using System.CodeDom.Compiler;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts;

namespace XRL.World;

[Serializable]
[GenerateSerializationPartial]
public sealed class HindrenClueLook : IComposite
{
	public string target;

	public string text;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public bool WantFieldReflection => false;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(target);
		Writer.WriteOptimized(text);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public void Read(SerializationReader Reader)
	{
		target = Reader.ReadOptimizedString();
		text = Reader.ReadOptimizedString();
	}

	public HindrenClueLook()
	{
	}

	public HindrenClueLook(string target, string text)
	{
		this.target = target;
		this.text = text;
	}

	public void apply(GameObject go)
	{
		HindrenClueItem p = new HindrenClueItem();
		go.AddPart(p);
		Description part = go.GetPart<Description>();
		part._Short = part._Short + " " + text;
		if (target == "Kesehind")
		{
			go.Body.GetPart("Body")[0].Equipped?.MakeBloodstained("blood", 7);
		}
	}
}
