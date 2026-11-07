using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("ex2D/Clipping")]
public class exClipping : exPlane
{
	private const string shaderPostfix = " (Clipping)";

	private Dictionary<MaterialTableKey, Material> materialTable = new Dictionary<MaterialTableKey, Material>(MaterialTableKey.Comparer.instance);

	private Vector2 currentPos = Vector2.zero;

	private bool dirty;

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
				dirty = true;
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
				dirty = true;
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
				dirty = true;
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
				dirty = true;
			}
		}
	}

	private void Awake()
	{
		exSpriteBase[] componentsInChildren = gameObject.GetComponentsInChildren<exSpriteBase>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].clip = this;
		}
		currentPos = new Vector2(transform.position.x, transform.position.y);
		dirty = false;
		UpdateClipMaterials();
	}

	private void OnDestroy()
	{
		Remove(gameObject);
	}

	private void LateUpdate()
	{
		if (transform.hasChanged)
		{
			Vector2 vector = new Vector2(transform.position.x, transform.position.y);
			if (vector != currentPos)
			{
				currentPos = vector;
				dirty = true;
			}
		}
		if (dirty)
		{
			UpdateClipMaterials();
			dirty = false;
		}
	}

	public void SetDirty()
	{
		dirty = true;
	}

	public void CheckDirty()
	{
		if (dirty)
		{
			UpdateClipMaterials();
			dirty = false;
		}
	}

	private void UpdateClipMaterials()
	{
		Rect worldAABoundingRect = GetWorldAABoundingRect();
		Vector4 value = new Vector4(worldAABoundingRect.center.x, worldAABoundingRect.center.y, worldAABoundingRect.width, worldAABoundingRect.height);
		foreach (Material value2 in materialTable.Values)
		{
			value2.SetVector("_ClipRect", value);
		}
	}

	public void Add(exSpriteBase _sprite)
	{
		exClipping clip = _sprite.clip;
		if ((object)clip != this)
		{
			if (clip != null)
			{
				clip.Remove(_sprite);
			}
			exSpriteBase[] componentsInChildren = _sprite.GetComponentsInChildren<exSpriteBase>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].clip = this;
			}
			if (!_sprite.transform.IsChildOf(transform))
			{
				_sprite.transform.parent = transform;
			}
			dirty = true;
		}
	}

	public void Add(GameObject _gameObject)
	{
		exSpriteBase[] componentsInChildren = _gameObject.GetComponentsInChildren<exSpriteBase>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].clip = this;
		}
		if (!_gameObject.transform.IsChildOf(transform))
		{
			_gameObject.transform.parent = transform;
		}
		dirty = true;
	}

	public void Remove(exSpriteBase _sprite)
	{
		Remove(_sprite.gameObject);
	}

	public void Remove(GameObject _gameObject)
	{
		exSpriteBase[] componentsInChildren = _gameObject.GetComponentsInChildren<exSpriteBase>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].clip = null;
		}
		dirty = true;
	}

	public Material GetClippedMaterial(Shader _shader, Texture _texture)
	{
		if (_shader == null)
		{
			_shader = Shader.Find("ex2D/Alpha Blended (Clipping)");
			if (_shader == null)
			{
				return null;
			}
		}
		else
		{
			Shader shader = Shader.Find(_shader.name + " (Clipping)");
			if (shader == null)
			{
				Debug.LogError("Failed to find clip shader named " + _shader.name + " (Clipping)");
				return null;
			}
			_shader = shader;
		}
		MaterialTableKey key = new MaterialTableKey(_shader, _texture);
		if (!materialTable.TryGetValue(key, out var value) || value == null)
		{
			value = new Material(_shader);
			value.hideFlags = HideFlags.DontSave;
			value.mainTexture = _texture;
			Rect worldAABoundingRect = GetWorldAABoundingRect();
			Vector4 value2 = new Vector4(worldAABoundingRect.center.x, worldAABoundingRect.center.y, worldAABoundingRect.width, worldAABoundingRect.height);
			value.SetVector("_ClipRect", value2);
			materialTable[key] = value;
		}
		return value;
	}
}
