using UnityEngine;
using ex2D.Detail;

[AddComponentMenu("ex2D/2D Sprite")]
public class exSprite : exLayeredSprite, exISprite, exISpriteBase, IMonoBehaviour
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
			if (layer_ != null)
			{
				((exISprite)this).UpdateBufferSize();
				updateFlags |= exUpdateFlags.All;
			}
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
				if (layer_ != null)
				{
					((exISprite)this).UpdateBufferSize();
					updateFlags |= exUpdateFlags.Vertex | exUpdateFlags.UV;
				}
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
				if (spriteType_ == exSpriteType.Sliced && layer_ != null)
				{
					((exISprite)this).UpdateBufferSize();
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
				if (spriteType_ == exSpriteType.Sliced && layer_ != null)
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
			if (!customBorderSize)
			{
				return textureInfo_.borderLeft;
			}
			return leftBorderSize_;
		}
		set
		{
			if (leftBorderSize_ != value)
			{
				leftBorderSize_ = value;
				if (spriteType_ == exSpriteType.Sliced && layer_ != null)
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
			if (!customBorderSize)
			{
				return textureInfo_.borderRight;
			}
			return rightBorderSize_;
		}
		set
		{
			if (rightBorderSize_ != value)
			{
				rightBorderSize_ = value;
				if (spriteType_ == exSpriteType.Sliced && layer_ != null)
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
			if (!customBorderSize)
			{
				return textureInfo_.borderTop;
			}
			return topBorderSize_;
		}
		set
		{
			if (topBorderSize_ != value)
			{
				topBorderSize_ = value;
				if (spriteType_ == exSpriteType.Sliced && layer_ != null)
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
			if (!customBorderSize)
			{
				return textureInfo_.borderBottom;
			}
			return bottomBorderSize_;
		}
		set
		{
			if (bottomBorderSize_ != value)
			{
				bottomBorderSize_ = value;
				if (spriteType_ == exSpriteType.Sliced && layer_ != null)
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
			else
			{
				if (customSize_ == value)
				{
					return;
				}
				customSize_ = value;
				if ((textureInfo_ != null && (float)textureInfo_.width != width_) || (float)textureInfo_.height != height_)
				{
					if (!customSize_)
					{
						width_ = textureInfo_.width;
						height_ = textureInfo_.height;
					}
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
			if (spriteType_ == exSpriteType.Tiled && layer_ != null)
			{
				((exISprite)this).UpdateBufferSize();
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
			if (spriteType_ == exSpriteType.Tiled && layer_ != null)
			{
				((exISprite)this).UpdateBufferSize();
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
		exUpdateFlags exUpdateFlags2 = base.UpdateBuffers(_vertices, _uvs, _colors32, _indices);
		if (textureInfo_ != null)
		{
			switch (spriteType_)
			{
			case exSpriteType.Simple:
				SpriteBuilder.SimpleUpdateBuffers(this, textureInfo_, useTextureOffset_, Space.World, _vertices, _uvs, _indices, vertexBufferIndex, indexBufferIndex);
				break;
			case exSpriteType.Sliced:
				SpriteBuilder.SlicedUpdateBuffers(this, textureInfo_, useTextureOffset_, Space.World, _vertices, _uvs, _indices, vertexBufferIndex, indexBufferIndex);
				break;
			case exSpriteType.Tiled:
				SpriteBuilder.TiledUpdateBuffers(this, textureInfo_, useTextureOffset_, tiledSpacing_, Space.World, _vertices, _uvs, _indices, vertexBufferIndex, indexBufferIndex);
				break;
			case exSpriteType.Diced:
				SpriteBuilder.DicedUpdateBuffers(this, textureInfo_, useTextureOffset_, Space.World, _vertices, _uvs, _indices, vertexBufferIndex, indexBufferIndex);
				break;
			}
			if ((updateFlags & exUpdateFlags.Color) != 0 && _colors32 != null)
			{
				Color32 color = new Color(color_.r, color_.g, color_.b, color_.a * layer_.alpha);
				for (int i = 0; i < vertexCount_; i++)
				{
					_colors32.buffer[vertexBufferIndex + i] = color;
				}
			}
		}
		else if (updateFlags != exUpdateFlags.None)
		{
			if (_indices != null)
			{
				_vertices.buffer[vertexBufferIndex] = cachedTransform.position;
				for (int j = indexBufferIndex; j < indexBufferIndex + indexCount_; j++)
				{
					_indices.buffer[j] = vertexBufferIndex;
				}
				exUpdateFlags2 |= exUpdateFlags.VertexAndIndex;
			}
			else
			{
				Vector3 position = cachedTransform.position;
				for (int k = vertexBufferIndex; k < vertexBufferIndex + vertexCount_; k++)
				{
					_vertices.buffer[k] = position;
				}
				exUpdateFlags2 |= exUpdateFlags.Vertex;
			}
		}
		exUpdateFlags2 |= updateFlags;
		updateFlags = exUpdateFlags.None;
		return exUpdateFlags2;
	}

	protected override Vector3[] GetVertices(Space _space)
	{
		if (textureInfo_ == null || layer_ == null)
		{
			return new Vector3[0];
		}
		exList<Vector3> tempList = exList<Vector3>.GetTempList();
		((exISprite)this).UpdateBufferSize();
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

	protected override void OnPreAddToLayer()
	{
		this.GetVertexAndIndexCount(out vertexCount_, out indexCount_);
	}

	void exISprite.UpdateBufferSize()
	{
		this.GetVertexAndIndexCount(out var _vertexCount, out var _indexCount);
		if (vertexCount_ != _vertexCount || indexCount_ != _indexCount)
		{
			if (layer_ != null)
			{
				layer_.SetSpriteBufferSize(this, _vertexCount, _indexCount);
				return;
			}
			vertexCount_ = _vertexCount;
			indexCount_ = _indexCount;
		}
	}
}
