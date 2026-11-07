using System;
using UnityEngine;

public abstract class exLayeredSprite : exSpriteBase, IComparable<exLayeredSprite>, exLayer.IFriendOfLayer
{
	public static bool enableFastShowHide = true;

	[SerializeField]
	protected float depth_;

	[HideInInspector]
	public int spriteIdInLayer = -1;

	[NonSerialized]
	internal int spriteIndexInMesh = -1;

	[NonSerialized]
	internal int vertexBufferIndex = -1;

	[NonSerialized]
	internal int indexBufferIndex = -1;

	[NonSerialized]
	protected bool transparent_ = true;

	[NonSerialized]
	protected exLayer layer_;

	[NonSerialized]
	private float globalDepth_;

	[NonSerialized]
	protected Transform cachedTransform_;

	public float depth
	{
		get
		{
			return depth_;
		}
		set
		{
			if (depth_ != value)
			{
				depth_ = value;
				SetDepthDirty();
			}
		}
	}

	public bool transparent
	{
		get
		{
			return transparent_;
		}
		set
		{
			if (transparent_ != value)
			{
				transparent_ = value;
				updateFlags |= exUpdateFlags.Transparent;
			}
		}
	}

	public exLayer layer => layer_;

	float exLayer.IFriendOfLayer.globalDepth
	{
		get
		{
			return globalDepth_;
		}
		set
		{
			globalDepth_ = value;
		}
	}

	public bool isInIndexBuffer
	{
		get
		{
			return indexBufferIndex != -1;
		}
		set
		{
			if (!value)
			{
				indexBufferIndex = -1;
			}
			else
			{
				Debug.LogError("isInIndexBuffer can not set to true, use SetLayer instead.");
			}
		}
	}

	public Transform cachedTransform
	{
		get
		{
			if ((object)cachedTransform_ == null)
			{
				cachedTransform_ = transform;
				cachedWorldMatrix = cachedTransform_.localToWorldMatrix;
			}
			return cachedTransform_;
		}
	}

	protected override void Show()
	{
		if (layer_ != null)
		{
			if (enableFastShowHide)
			{
				layer_.FastShowSprite(this);
			}
			else
			{
				layer_.ShowSprite(this);
			}
		}
	}

	protected override void Hide()
	{
		if (layer_ != null)
		{
			if (enableFastShowHide)
			{
				layer_.FastHideSprite(this);
			}
			else
			{
				layer_.HideSprite(this);
			}
		}
	}

	protected new void OnDestroy()
	{
		base.OnDestroy();
		if (layer_ != null)
		{
			layer_.Remove(this, _recursively: false);
		}
	}

	protected override void UpdateMaterial()
	{
		if (layer_ != null)
		{
			layer_.RefreshSpriteMaterial(this);
		}
		else
		{
			material_ = null;
		}
	}

	public override float GetScaleX(Space _space)
	{
		if (_space == Space.World)
		{
			return new Vector3(cachedWorldMatrix.m00, cachedWorldMatrix.m10, cachedWorldMatrix.m20).magnitude;
		}
		return cachedTransform.localScale.x;
	}

	public override float GetScaleY(Space _space)
	{
		if (_space == Space.World)
		{
			return new Vector3(cachedWorldMatrix.m01, cachedWorldMatrix.m11, cachedWorldMatrix.m21).magnitude;
		}
		return cachedTransform.localScale.y;
	}

	public override Vector3[] GetWorldVertices()
	{
		Vector3[] vertices = GetVertices(Space.World);
		if (layer_ != null)
		{
			int num = layer_.IndexOfMesh(this);
			if (num != -1)
			{
				Matrix4x4 localToWorldMatrix = layer_.meshList[num].transform.localToWorldMatrix;
				for (int i = 0; i < vertices.Length; i++)
				{
					vertices[i] = localToWorldMatrix.MultiplyPoint3x4(vertices[i]);
				}
				return vertices;
			}
		}
		return vertices;
	}

	public override void SetClip(exClipping _clip = null)
	{
		if (_clip != null && layer_ != null && !_clip.transform.IsChildOf(layer_.transform))
		{
			Debug.LogError("Can not add to clip which not in current layer!");
		}
		else
		{
			base.SetClip(_clip);
		}
	}

	internal override exUpdateFlags UpdateBuffers(exList<Vector3> _vertices, exList<Vector2> _uvs, exList<Color32> _colors32, exList<int> _indices = null)
	{
		if ((updateFlags & exUpdateFlags.Transparent) != 0)
		{
			updateFlags &= ~exUpdateFlags.Transparent;
			if (transparent_)
			{
				Vector3 vector = _vertices.buffer[0];
				for (int i = 1; i < vertexCount_; i++)
				{
					_vertices.buffer[vertexBufferIndex + i] = vector;
				}
				updateFlags &= ~exUpdateFlags.Vertex;
			}
			else
			{
				updateFlags |= exUpdateFlags.Vertex;
			}
			return exUpdateFlags.Vertex | exUpdateFlags.Transparent;
		}
		if (transparent_ && (updateFlags & exUpdateFlags.Vertex) != 0)
		{
			updateFlags &= ~exUpdateFlags.Vertex;
		}
		return exUpdateFlags.None;
	}

	public static bool operator >(exLayeredSprite _lhs, exLayeredSprite _rhs)
	{
		bool flag = (object)_lhs.layer_ == null || _lhs.layer_.ordered;
		if (!(_lhs.globalDepth_ > _rhs.globalDepth_))
		{
			if (flag && _lhs.globalDepth_ == _rhs.globalDepth_)
			{
				return _lhs.spriteIdInLayer > _rhs.spriteIdInLayer;
			}
			return false;
		}
		return true;
	}

	public static bool operator >=(exLayeredSprite _lhs, exLayeredSprite _rhs)
	{
		bool flag = (object)_lhs.layer_ == null || _lhs.layer_.ordered;
		if (!(_lhs.globalDepth_ > _rhs.globalDepth_))
		{
			if (_lhs.globalDepth_ == _rhs.globalDepth_)
			{
				if (flag)
				{
					return _lhs.spriteIdInLayer >= _rhs.spriteIdInLayer;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public static bool operator <(exLayeredSprite _lhs, exLayeredSprite _rhs)
	{
		bool flag = (object)_lhs.layer_ == null || _lhs.layer_.ordered;
		if (!(_lhs.globalDepth_ < _rhs.globalDepth_))
		{
			if (flag && _lhs.globalDepth_ == _rhs.globalDepth_)
			{
				return _lhs.spriteIdInLayer < _rhs.spriteIdInLayer;
			}
			return false;
		}
		return true;
	}

	public static bool operator <=(exLayeredSprite _lhs, exLayeredSprite _rhs)
	{
		bool flag = (object)_lhs.layer_ == null || _lhs.layer_.ordered;
		if (!(_lhs.globalDepth_ < _rhs.globalDepth_))
		{
			if (_lhs.globalDepth_ == _rhs.globalDepth_)
			{
				if (flag)
				{
					return _lhs.spriteIdInLayer <= _rhs.spriteIdInLayer;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public int CompareTo(exLayeredSprite _other)
	{
		if (globalDepth_ < _other.globalDepth_)
		{
			return -1;
		}
		if (globalDepth_ > _other.globalDepth_)
		{
			return 1;
		}
		if ((object)layer_ == null || layer_.ordered)
		{
			return spriteIdInLayer - _other.spriteIdInLayer;
		}
		return 0;
	}

	void exLayer.IFriendOfLayer.DoSetLayer(exLayer _layer)
	{
		if (layer_ == null && _layer != null)
		{
			OnPreAddToLayer();
		}
		layer_ = _layer;
	}

	void exLayer.IFriendOfLayer.DoSetBufferSize(int _vertexCount, int _indexCount)
	{
		vertexCount_ = _vertexCount;
		indexCount_ = _indexCount;
	}

	void exLayer.IFriendOfLayer.SetMaterialDirty()
	{
		material_ = null;
	}

	void exLayer.IFriendOfLayer.ResetLayerProperties()
	{
		layer_ = null;
		isInIndexBuffer = false;
	}

	public void SetLayer(exLayer _layer = null)
	{
		if ((object)layer_ != _layer)
		{
			if (_layer != null)
			{
				_layer.Add(this);
			}
			else if (layer_ != null)
			{
				layer_.Remove(this);
			}
		}
	}

	[ContextMenu("SetDepthDirty")]
	public void SetDepthDirty()
	{
		if (layer_ != null)
		{
			layer_.SetDepthDirty(gameObject);
		}
	}

	protected virtual void OnPreAddToLayer()
	{
	}

	internal virtual void FillBuffers(exList<Vector3> _vertices, exList<Vector2> _uvs, exList<Color32> _colors32)
	{
		vertexBufferIndex = _vertices.Count;
		_vertices.AddRange(vertexCount_);
		_colors32?.AddRange(vertexCount_);
		_uvs.AddRange(vertexCount_);
		updateFlags |= exUpdateFlags.AllExcludeIndex;
	}

	public void UpdateTransform()
	{
		if (cachedTransform.hasChanged)
		{
			cachedTransform_.hasChanged = false;
			cachedWorldMatrix = cachedTransform_.localToWorldMatrix;
			updateFlags |= exUpdateFlags.Vertex;
		}
	}
}
