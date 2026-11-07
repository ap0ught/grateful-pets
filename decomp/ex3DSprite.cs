using UnityEngine;
using ex2D.Detail;

[AddComponentMenu("ex2D/3D Sprite")]
public class ex3DSprite : exStandaloneSprite, exISprite, exISpriteBase, IMonoBehaviour
{
	[SerializeField]
	private exTextureInfo textureInfo_;

	[SerializeField]
	protected bool useTextureOffset_;

	[SerializeField]
	protected exSpriteType spriteType_;

	[SerializeField]
	protected Vector2 tiledSpacing_ = new Vector2(0f, 0f);

	[SerializeField]
	protected bool borderOnly_;

	[SerializeField]
	protected bool customBorderSize_;

	[SerializeField]
	protected float leftBorderSize_;

	[SerializeField]
	protected float rightBorderSize_;

	[SerializeField]
	protected float topBorderSize_;

	[SerializeField]
	protected float bottomBorderSize_;

	public exTextureInfo textureInfo
	{
		get
		{
			return textureInfo_;
		}
		set
		{
			if (value != null)
			{
				if (isOnEnabled)
				{
					Show();
				}
			}
			else if (isOnEnabled && textureInfo_ != null)
			{
				Hide();
			}
			exSpriteUtility.SetTextureInfo(this, ref textureInfo_, value, useTextureOffset_, spriteType_);
		}
	}

	public bool useTextureOffset
	{
		get
		{
			return useTextureOffset_;
		}
		set
		{
			if (useTextureOffset_ != value)
			{
				useTextureOffset_ = value;
				updateFlags |= exUpdateFlags.Vertex;
			}
		}
	}

	public exSpriteType spriteType
	{
		get
		{
			return spriteType_;
		}
		set
		{
			if (spriteType_ == value)
			{
				return;
			}
			switch (value)
			{
			case exSpriteType.Tiled:
				customSize_ = true;
				break;
			case exSpriteType.Diced:
				if (textureInfo_ != null && textureInfo_.diceUnitWidth == 0 && textureInfo_.diceUnitHeight == 0)
				{
					Debug.LogWarning("The texture info is not diced!");
				}
				break;
			}
			spriteType_ = value;
			UpdateBufferSize();
			updateFlags |= exUpdateFlags.All;
		}
	}

	public Vector2 tiledSpacing
	{
		get
		{
			return tiledSpacing_;
		}
		set
		{
			if (tiledSpacing_ != value)
			{
				tiledSpacing_ = value;
				UpdateBufferSize();
				updateFlags |= exUpdateFlags.Vertex | exUpdateFlags.UV;
			}
		}
	}

	public bool borderOnly
	{
		get
		{
			return borderOnly_;
		}
		set
		{
			if (borderOnly_ != value)
			{
				borderOnly_ = value;
				if (spriteType_ == exSpriteType.Sliced)
				{
					UpdateBufferSize();
					updateFlags |= exUpdateFlags.All;
				}
			}
		}
	}

	public bool customBorderSize
	{
		get
		{
			return customBorderSize_;
		}
		set
		{
			if (customBorderSize_ != value)
			{
				customBorderSize_ = value;
				if (spriteType_ == exSpriteType.Sliced)
				{
					updateFlags |= exUpdateFlags.Vertex;
				}
			}
		}
	}

	public float leftBorderSize
	{
		get
		{
			return leftBorderSize_;
		}
		set
		{
			if (leftBorderSize_ != value)
			{
				leftBorderSize_ = value;
				if (spriteType_ == exSpriteType.Sliced)
				{
					updateFlags |= exUpdateFlags.Vertex;
				}
			}
		}
	}

	public float rightBorderSize
	{
		get
		{
			return rightBorderSize_;
		}
		set
		{
			if (rightBorderSize_ != value)
			{
				rightBorderSize_ = value;
				if (spriteType_ == exSpriteType.Sliced)
				{
					updateFlags |= exUpdateFlags.Vertex;
				}
			}
		}
	}

	public float topBorderSize
	{
		get
		{
			return topBorderSize_;
		}
		set
		{
			if (topBorderSize_ != value)
			{
				topBorderSize_ = value;
				if (spriteType_ == exSpriteType.Sliced)
				{
					updateFlags |= exUpdateFlags.Vertex;
				}
			}
		}
	}

	public float bottomBorderSize
	{
		get
		{
			return bottomBorderSize_;
		}
		set
		{
			if (bottomBorderSize_ != value)
			{
				bottomBorderSize_ = value;
				if (spriteType_ == exSpriteType.Sliced)
				{
					updateFlags |= exUpdateFlags.Vertex;
				}
			}
		}
	}

	protected override Texture texture
	{
		get
		{
			if (textureInfo_ != null)
			{
				return textureInfo_.texture;
			}
			return null;
		}
	}

	public override bool customSize
	{
		get
		{
			if (spriteType_ != exSpriteType.Tiled)
			{
				return customSize_;
			}
			return true;
		}
		set
		{
			if (spriteType_ == exSpriteType.Tiled)
			{
				customSize_ = true;
			}
			else if (customSize_ != value)
			{
				customSize_ = value;
				if (!customSize_ && textureInfo_ != null && ((float)textureInfo_.width != width_ || (float)textureInfo_.height != height_))
				{
					width_ = textureInfo_.width;
					height_ = textureInfo_.height;
					updateFlags |= exUpdateFlags.Vertex;
				}
			}
		}
	}

	public override float width
	{
		get
		{
			if (!customSize_)
			{
				return (textureInfo_ != null) ? textureInfo_.width : 0;
			}
			return width_;
		}
		set
		{
			base.width = value;
			if (spriteType_ == exSpriteType.Tiled)
			{
				UpdateBufferSize();
				updateFlags |= exUpdateFlags.UV;
			}
		}
	}

	public override float height
	{
		get
		{
			if (!customSize_)
			{
				return (textureInfo_ != null) ? textureInfo_.height : 0;
			}
			return height_;
		}
		set
		{
			base.height = value;
			if (spriteType_ == exSpriteType.Tiled)
			{
				UpdateBufferSize();
				updateFlags |= exUpdateFlags.UV;
			}
		}
	}

	public override bool visible
	{
		get
		{
			if (isOnEnabled)
			{
				return textureInfo_ != null;
			}
			return false;
		}
	}

	internal override exUpdateFlags UpdateBuffers(exList<Vector3> _vertices, exList<Vector2> _uvs, exList<Color32> _colors32, exList<int> _indices)
	{
		switch (spriteType_)
		{
		case exSpriteType.Simple:
			SpriteBuilder.SimpleUpdateBuffers(this, textureInfo_, useTextureOffset_, Space.Self, _vertices, _uvs, _indices, 0, 0);
			break;
		case exSpriteType.Sliced:
			SpriteBuilder.SlicedUpdateBuffers(this, textureInfo_, useTextureOffset_, Space.Self, _vertices, _uvs, _indices, 0, 0);
			break;
		case exSpriteType.Tiled:
			SpriteBuilder.TiledUpdateBuffers(this, textureInfo_, useTextureOffset_, tiledSpacing_, Space.Self, _vertices, _uvs, _indices, 0, 0);
			break;
		case exSpriteType.Diced:
			SpriteBuilder.DicedUpdateBuffers(this, textureInfo_, useTextureOffset_, Space.Self, _vertices, _uvs, _indices, 0, 0);
			break;
		}
		if ((updateFlags & exUpdateFlags.Color) != 0 && _colors32 != null)
		{
			Color32 color = new Color(color_.r, color_.g, color_.b, color_.a);
			for (int i = 0; i < vertexCount_; i++)
			{
				_colors32.buffer[i] = color;
			}
		}
		exUpdateFlags result = updateFlags;
		updateFlags = exUpdateFlags.None;
		return result;
	}

	private void TiledUpdateBuffers(exList<Vector3> _vertices, exList<Vector2> _uvs, exList<int> _indices)
	{
	}

	protected override Vector3[] GetVertices(Space _space)
	{
		if (textureInfo_ == null)
		{
			return new Vector3[0];
		}
		exList<Vector3> tempList = exList<Vector3>.GetTempList();
		UpdateBufferSize();
		tempList.AddRange(vertexCount_);
		switch (spriteType_)
		{
		case exSpriteType.Simple:
			SpriteBuilder.SimpleUpdateVertexBuffer(this, textureInfo_, useTextureOffset_, _space, tempList, 0);
			break;
		case exSpriteType.Sliced:
			SpriteBuilder.SimpleUpdateVertexBuffer(this, textureInfo_, useTextureOffset_, _space, tempList, 0);
			SpriteBuilder.SimpleVertexBufferToSliced(this, textureInfo_, tempList, 0);
			break;
		case exSpriteType.Tiled:
			SpriteBuilder.TiledUpdateVertexBuffer(this, textureInfo_, useTextureOffset_, tiledSpacing_, _space, tempList, 0);
			break;
		case exSpriteType.Diced:
			SpriteBuilder.SimpleUpdateVertexBuffer(this, textureInfo_, useTextureOffset_, _space, tempList, 0);
			SpriteBuilder.SimpleVertexBufferToDiced(this, textureInfo_, tempList, 0);
			break;
		}
		return tempList.ToArray();
	}

	protected override void UpdateVertexAndIndexCount()
	{
		this.GetVertexAndIndexCount(out vertexCount_, out indexCount_);
	}

	void exISprite.UpdateBufferSize()
	{
		UpdateBufferSize();
	}
}
