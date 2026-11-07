using System.Collections.Generic;
using UnityEngine;

public struct MaterialTableKey(Shader _shader, Texture _texture)
{
	public class Comparer : IEqualityComparer<MaterialTableKey>
	{
		private static Comparer instance_;

		public static Comparer instance
		{
			get
			{
				if (instance_ == null)
				{
					instance_ = new Comparer();
				}
				return instance_;
			}
		}

		public bool Equals(MaterialTableKey _lhs, MaterialTableKey _rhs)
		{
			if ((object)_lhs.shader == _rhs.shader)
			{
				return (object)_lhs.texture == _rhs.texture;
			}
			return false;
		}

		public int GetHashCode(MaterialTableKey _obj)
		{
			int num = ((_obj.shader != null) ? _obj.shader.GetHashCode() : 0);
			int num2 = ((_obj.texture != null) ? (_obj.texture.GetHashCode() * 1313) : 0);
			return num ^ num2;
		}
	}

	public Shader shader = _shader;

	public Texture texture = _texture;

	public MaterialTableKey(Material _material)
		: this(_material.shader, _material.mainTexture)
	{
	}
}
