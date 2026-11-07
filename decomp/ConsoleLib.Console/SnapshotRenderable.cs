using System;
using System.CodeDom.Compiler;
using Occult.Engine.CodeGeneration;
using XRL.World;

namespace ConsoleLib.Console;

[Serializable]
[GenerateSerializationPartial]
public class SnapshotRenderable : Renderable
{
	public bool HFlip;

	public bool VFlip;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override bool WantFieldReflection => false;

	public SnapshotRenderable()
	{
	}

	public SnapshotRenderable(IRenderable Source)
	{
		Copy(Source);
		Tile = Source.getTile();
		RenderString = Source.getRenderString();
		ColorString = Source.getColorString();
		TileColor = Source.getTileColor();
		DetailColor = Source.getDetailColor();
		HFlip = Source.getHFlip();
		VFlip = Source.getVFlip();
	}

	public override bool getHFlip()
	{
		return HFlip;
	}

	public override bool getVFlip()
	{
		return VFlip;
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Write(SerializationWriter Writer)
	{
		Writer.Write(HFlip);
		Writer.Write(VFlip);
		Writer.WriteOptimized(Tile);
		Writer.WriteOptimized(RenderString);
		Writer.WriteOptimized(ColorString);
		Writer.WriteOptimized(TileColor);
		Writer.Write(DetailColor);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Read(SerializationReader Reader)
	{
		HFlip = Reader.ReadBoolean();
		VFlip = Reader.ReadBoolean();
		Tile = Reader.ReadOptimizedString();
		RenderString = Reader.ReadOptimizedString();
		ColorString = Reader.ReadOptimizedString();
		TileColor = Reader.ReadOptimizedString();
		DetailColor = Reader.ReadChar();
	}
}
