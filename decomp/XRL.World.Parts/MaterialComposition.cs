using System;

namespace XRL.World.Parts;

[Serializable]
public class MaterialComposition : IPart
{
	public string _Material;

	public string Material
	{
		get
		{
			return _Material;
		}
		set
		{
			_Material = value;
		}
	}
}
