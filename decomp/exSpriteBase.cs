using System;
using UnityEngine;

[ExecuteInEditMode]
public abstract class exSpriteBase : exPlane, exISpriteBase, IMonoBehaviour
{
	[SerializeField]
	protected bool customSize_;

	[SerializeField]
	protected Color color_ = new Color(1f, 1f, 1f, 1f);

	[SerializeField]
	protected Vector2 shear_ = Vector2.zero;

	[SerializeField]
	private Shader shader_;

	[NonSerialized]
	protected bool isOnEnabled;

	[NonSerialized]
	public exUpdateFlags updateFlags = exUpdateFlags.All;

	[NonSerialized]
	protected exClipping clip_;

	[NonSerialized]
	internal Matrix4x4 cachedWorldMatrix;

	[NonSerialized]
	protected int vertexCount_ = -1;

	[NonSerialized]
	protected int indexCount_ = -1;

	[NonSerialized]
	protected Material material_;

	public virtual bool customSize
	{
		get
		{
			return customSize_;
		}
		set
		{
			customSize_ = value;
			updateFlags |= exUpdateFlags.Vertex;
		}
	}

	public Color color
	{
		get
		{
			return color_;
		}
		set
		{
			if (color_ != value)
			{
				color_ = value;
				updateFlags |= exUpdateFlags.Color;
			}
		}
	}

	public Vector2 shear
	{
		get
		{
			return shear_;
		}
		set
		{
			if (shear_ != value)
			{
				shear_ = value;
				updateFlags |= exUpdateFlags.Vertex;
			}
		}
	}

	public Shader shader
	{
		get
		{
			return shader_;
		}
		set
		{
			if ((object)shader_ != value)
			{
				shader_ = value;
				UpdateMaterial();
			}
		}
	}

	exUpdateFlags exISpriteBase.updateFlags
	{
		get
		{
			return updateFlags;
		}
		set
		{
			updateFlags = value;
		}
	}

	public exClipping clip
	{
		get
		{
			return clip_;
		}
		set
		{
			if (clip_ != value)
			{
				clip_ = value;
				UpdateMaterial();
			}
		}
	}

	Matrix4x4 exISpriteBase.cachedWorldMatrix => cachedWorldMatrix;

	public override float width
	{
		get
		{
			return width_;
		}
		set
		{
			if (width_ != value)
			{
				width_ = value;
				updateFlags |= exUpdateFlags.Vertex;
				customSize = true;
			}
		}
	}

	public override float height
	{
		get
		{
			return height_;
		}
		set
		{
			if (height_ != value)
			{
				height_ = value;
				updateFlags |= exUpdateFlags.Vertex;
				customSize = true;
			}
		}
	}

	public override Anchor anchor
	{
		get
		{
			return anchor_;
		}
		set
		{
			if (anchor_ != value)
			{
				anchor_ = value;
				updateFlags |= exUpdateFlags.Vertex;
			}
		}
	}

	public override Vector2 offset
	{
		get
		{
			return offset_;
		}
		set
		{
			if (offset_ != value)
			{
				offset_ = value;
				updateFlags |= exUpdateFlags.Vertex;
			}
		}
	}

	public int vertexCount => vertexCount_;

	public int indexCount => indexCount_;

	public Material material
	{
		get
		{
			if (material_ != null)
			{
				return material_;
			}
			if (clip_ != null)
			{
				material_ = clip_.GetClippedMaterial(shader_, texture);
				if (material_ == null)
				{
					material_ = ex2DRenderer.GetMaterial(shader_, texture);
				}
			}
			else
			{
				material_ = ex2DRenderer.GetMaterial(shader_, texture);
			}
			return material_;
		}
	}

	protected abstract Texture texture { get; }

	public virtual bool visible => isOnEnabled;

	protected void OnEnable()
	{
		isOnEnabled = true;
		if (visible)
		{
			Show();
		}
	}

	protected void OnDisable()
	{
		isOnEnabled = false;
		Hide();
	}

	protected void OnDestroy()
	{
		if (clip_ != null)
		{
			clip_.Remove(this);
		}
	}

	public virtual void SetClip(exClipping _clip = null)
	{
		if ((object)clip_ != _clip)
		{
			if (_clip != null)
			{
				_clip.Add(this);
			}
			else if (clip_ != null)
			{
				clip_.Remove(this);
			}
		}
	}

	internal abstract exUpdateFlags UpdateBuffers(exList<Vector3> _vertices, exList<Vector2> _uvs, exList<Color32> _colors32, exList<int> _indices = null);

	public abstract float GetScaleX(Space _space);

	public abstract float GetScaleY(Space _space);

	void exISpriteBase.UpdateMaterial()
	{
		UpdateMaterial();
	}

	protected abstract void UpdateMaterial();

	protected virtual void Show()
	{
	}

	protected virtual void Hide()
	{
	}
}
