using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public abstract class exStandaloneSprite : exSpriteBase
{
	[NonSerialized]
	protected Renderer cachedRenderer_;

	[NonSerialized]
	protected Mesh mesh_;

	private bool _bCollide;

	[NonSerialized]
	protected exList<Vector3> vertices = new exList<Vector3>();

	[NonSerialized]
	protected exList<int> indices = new exList<int>(6);

	[NonSerialized]
	protected exList<Vector2> uvs = new exList<Vector2>();

	[NonSerialized]
	protected exList<Color32> colors32 = new exList<Color32>();

	[NonSerialized]
	protected exList<Vector4> tangents = new exList<Vector4>();

	[NonSerialized]
	protected exList<Vector3> normals = new exList<Vector3>();

	public Renderer cachedRenderer
	{
		get
		{
			if (cachedRenderer_ == null)
			{
				cachedRenderer_ = GetComponent<Renderer>();
			}
			return cachedRenderer_;
		}
	}

	public Mesh mesh
	{
		get
		{
			if (mesh_ == null)
			{
				mesh_ = GetComponent<MeshFilter>().sharedMesh;
			}
			return mesh_;
		}
		private set
		{
			mesh_ = value;
			GetComponent<MeshFilter>().sharedMesh = mesh_;
			UpdateMesh();
		}
	}

	public bool bCollide
	{
		get
		{
			return _bCollide;
		}
		set
		{
			_bCollide = value;
			if (_bCollide)
			{
				gameObject.AddComponent<MeshCollider>();
			}
			UpdateMesh();
		}
	}

	public void UpdateMesh()
	{
		if (_bCollide)
		{
			GetComponent<MeshCollider>().sharedMesh = null;
		}
		if (_bCollide)
		{
			GetComponent<MeshCollider>().sharedMesh = mesh_;
		}
	}

	protected void Awake()
	{
		mesh = new Mesh();
		mesh.name = "ex2D Mesh";
		mesh.hideFlags = HideFlags.DontSave;
		mesh.MarkDynamic();
	}

	protected new void OnDestroy()
	{
		vertices = null;
		indices = null;
		uvs = null;
		colors32 = null;
		tangents = null;
		normals = null;
		if (mesh != null)
		{
			mesh_.Destroy();
		}
		mesh = null;
		cachedRenderer.sharedMaterial = null;
		cachedRenderer_ = null;
	}

	protected new void OnEnable()
	{
		isOnEnabled = true;
		Show();
		if (vertices.Count == 0)
		{
			cachedRenderer.sharedMaterial = material;
			UpdateVertexAndIndexCount();
			FillBuffers();
		}
	}

	protected new void OnDisable()
	{
		isOnEnabled = false;
		Hide();
	}

	protected void LateUpdate()
	{
		if (updateFlags == exUpdateFlags.None || !visible)
		{
			return;
		}
		exUpdateFlags exUpdateFlags2 = UpdateBuffers(vertices, uvs, colors32, indices);
		exMesh.FlushBuffers(mesh, exUpdateFlags2, vertices, indices, uvs, colors32, tangents, normals);
		if ((exUpdateFlags2 & exUpdateFlags.Index) != 0)
		{
			bool flag = indices.Count > 0;
			if (cachedRenderer.enabled != flag)
			{
				cachedRenderer.enabled = flag;
			}
		}
		UpdateMesh();
	}

	protected override void Show()
	{
		if (!cachedRenderer.enabled)
		{
			cachedRenderer.enabled = true;
		}
	}

	protected override void Hide()
	{
		if (cachedRenderer.enabled)
		{
			cachedRenderer.enabled = false;
		}
	}

	protected override void UpdateMaterial()
	{
		material_ = null;
		cachedRenderer.sharedMaterial = material;
	}

	public override float GetScaleX(Space _space)
	{
		if (_space == Space.World)
		{
			return transform.lossyScale.x;
		}
		return transform.localScale.x;
	}

	public override float GetScaleY(Space _space)
	{
		if (_space == Space.World)
		{
			return transform.lossyScale.y;
		}
		return transform.localScale.y;
	}

	public override Vector3[] GetWorldVertices()
	{
		Vector3[] array = GetVertices(Space.Self);
		Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = localToWorldMatrix.MultiplyPoint3x4(array[i]);
		}
		return array;
	}

	protected abstract void UpdateVertexAndIndexCount();

	protected void UpdateBufferSize()
	{
		int num = vertexCount_;
		int num2 = indexCount_;
		UpdateVertexAndIndexCount();
		if (vertexCount_ != num || indexCount_ != num2)
		{
			tangents.Clear();
			normals.Clear();
			vertices.Clear();
			uvs.Clear();
			colors32.Clear();
			indices.Clear();
			FillBuffers();
		}
	}

	private void FillBuffers()
	{
		vertices.AddRange(vertexCount_);
		colors32.AddRange(vertexCount_);
		tangents.AddRange(vertexCount_);
		normals.AddRange(vertexCount_);
		uvs.AddRange(vertexCount_);
		indices.AddRange(indexCount_);
		updateFlags |= exUpdateFlags.All;
	}
}
