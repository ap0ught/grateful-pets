using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class exMesh : MonoBehaviour
{
	public static bool enableDoubleBuffer = true;

	public const int QUAD_INDEX_COUNT = 6;

	public const int QUAD_VERTEX_COUNT = 4;

	public const int MAX_VERTEX_COUNT = 65000;

	public const int MAX_QUAD_COUNT = 16250;

	[NonSerialized]
	private Renderer cachedRenderer;

	[NonSerialized]
	private MeshFilter cachedFilter;

	[NonSerialized]
	public List<exLayeredSprite> spriteList = new List<exLayeredSprite>();

	[NonSerialized]
	public List<exLayeredSprite> sortedSpriteList = new List<exLayeredSprite>();

	[NonSerialized]
	private Mesh mesh0;

	[NonSerialized]
	private Mesh mesh1;

	[NonSerialized]
	private bool isEvenMeshBuffer = true;

	[NonSerialized]
	public exList<Vector3> vertices = new exList<Vector3>();

	[NonSerialized]
	public exList<int> indices = new exList<int>(6);

	[NonSerialized]
	public exList<Vector2> uvs = new exList<Vector2>();

	[NonSerialized]
	public exList<Color32> colors32 = new exList<Color32>();

	[NonSerialized]
	public exList<Vector3> tangents = new exList<Vector3>();

	[NonSerialized]
	public exList<Vector4> normals = new exList<Vector4>();

	[NonSerialized]
	public exUpdateFlags updateFlags;

	[NonSerialized]
	public exUpdateFlags lastUpdateFlags;

	public Material material
	{
		get
		{
			if ((bool)cachedRenderer)
			{
				return cachedRenderer.sharedMaterial;
			}
			return null;
		}
		set
		{
			if ((bool)cachedRenderer)
			{
				cachedRenderer.sharedMaterial = value;
			}
			else
			{
				UnityEngine.Debug.LogError("no MeshRenderer");
			}
		}
	}

	public bool hasTriangle => sortedSpriteList.Count > 0;

	private bool isDynamic
	{
		get
		{
			if ((object)mesh0 != null)
			{
				return (object)mesh1 != null;
			}
			return false;
		}
	}

	private void Awake()
	{
		Init();
	}

	private void OnDestroy()
	{
		spriteList = null;
		sortedSpriteList = null;
		vertices = null;
		indices = null;
		uvs = null;
		colors32 = null;
		if (mesh0 != null)
		{
			mesh0.Destroy();
		}
		mesh0 = null;
		if (mesh1 != null)
		{
			mesh1.Destroy();
		}
		mesh1 = null;
		cachedFilter.sharedMesh = null;
		cachedFilter = null;
		cachedRenderer.sharedMaterial = null;
		cachedRenderer = null;
	}

	public static exMesh Create(exLayer _layer)
	{
		exMesh exMesh2 = new GameObject
		{
			hideFlags = HideFlags.HideAndDontSave
		}.AddComponent<exMesh>();
		exMesh2.Init();
		return exMesh2;
	}

	public static void FlushBuffers(Mesh _mesh, exUpdateFlags _updateFlags, exList<Vector3> _vertices, exList<int> _indices, exList<Vector2> _uvs, exList<Color32> _colors32, exList<Vector4> _tangents, exList<Vector3> _normals)
	{
		if ((_updateFlags & exUpdateFlags.VertexAndIndex) == exUpdateFlags.VertexAndIndex)
		{
			_mesh.triangles = null;
		}
		if ((_updateFlags & exUpdateFlags.Vertex) != 0 || (_updateFlags & exUpdateFlags.Index) != 0)
		{
			_mesh.vertices = _vertices.FastToArray();
		}
		if ((_updateFlags & exUpdateFlags.UV) != 0)
		{
			_mesh.uv = _uvs.FastToArray();
		}
		if ((_updateFlags & exUpdateFlags.Color) != 0)
		{
			_mesh.colors32 = _colors32.FastToArray();
			_mesh.tangents = _tangents.FastToArray();
			_mesh.normals = _normals.FastToArray();
		}
		if ((_updateFlags & exUpdateFlags.Index) != 0)
		{
			_mesh.triangles = _indices.FastToArray();
		}
		if ((_updateFlags & exUpdateFlags.Index) != 0 || (_updateFlags & exUpdateFlags.Vertex) != 0)
		{
			_mesh.RecalculateBounds();
		}
		if ((_updateFlags & exUpdateFlags.Normal) != 0)
		{
			Vector3[] array = new Vector3[_vertices.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new Vector3(0f, 0f, -1f);
			}
			_mesh.normals = array;
		}
	}

	public void Apply(exUpdateFlags _additionalUpdateFlags = exUpdateFlags.None)
	{
		updateFlags |= _additionalUpdateFlags;
		Mesh mesh = ((!isDynamic || updateFlags == exUpdateFlags.None) ? GetMeshBuffer() : SwapMeshBuffer());
		FlushBuffers(mesh, updateFlags, vertices, indices, uvs, colors32, null, null);
		if ((updateFlags & exUpdateFlags.Vertex) != 0)
		{
			bool flag = false;
			if (spriteList.Count > 10)
			{
				flag = true;
			}
			else
			{
				int i = 0;
				for (int count = spriteList.Count; i < count; i++)
				{
					if (spriteList[i].visible)
					{
						flag = true;
						break;
					}
				}
			}
			if (gameObject.activeSelf != flag)
			{
				gameObject.SetActive(flag);
			}
		}
		else if ((updateFlags & exUpdateFlags.Index) != 0)
		{
			bool flag2 = indices.Count > 0;
			if (gameObject.activeSelf != flag2)
			{
				gameObject.SetActive(flag2);
			}
		}
		updateFlags = exUpdateFlags.None;
	}

	public void Compact()
	{
		spriteList.TrimExcess();
		vertices.TrimExcess();
		indices.TrimExcess();
		uvs.TrimExcess();
		colors32.TrimExcess();
		updateFlags |= exUpdateFlags.UV | exUpdateFlags.Color | exUpdateFlags.Normal;
	}

	public void SetDynamic(bool _dynamic)
	{
		if (isDynamic == _dynamic)
		{
			return;
		}
		if (_dynamic)
		{
			if (mesh0 == null)
			{
				mesh0 = CreateMesh();
			}
			mesh0.MarkDynamic();
			if (mesh1 == null)
			{
				mesh1 = CreateMesh();
			}
			mesh1.MarkDynamic();
			lastUpdateFlags = exUpdateFlags.All;
		}
		else
		{
			if (!isEvenMeshBuffer)
			{
				isEvenMeshBuffer = true;
				updateFlags |= lastUpdateFlags;
			}
			if (mesh1 != null)
			{
				mesh1.Destroy();
			}
			mesh1 = null;
		}
	}

	[ContextMenu("Recalculate Bounds")]
	[Conditional("EX_DEBUG")]
	public void RecalculateBounds()
	{
		GetMeshBuffer().RecalculateBounds();
	}

	[ContextMenu("Output Mesh Info")]
	[Conditional("EX_DEBUG")]
	public void OutputDebugInfo(bool outputBuffer = false)
	{
		Mesh meshBuffer = GetMeshBuffer();
		if (meshBuffer == null)
		{
			UnityEngine.Debug.Log("mesh is null");
			return;
		}
		string text = string.Join(", ", spriteList.ConvertAll((exLayeredSprite x) => x.gameObject.name).ToArray());
		UnityEngine.Debug.Log(string.Format("exMesh: {3} spriteList[{0}]: ({2}) CurBufferId: {1}", spriteList.Count, (!isEvenMeshBuffer) ? 1 : 0, text, gameObject.name), this);
		if (outputBuffer)
		{
			string message = "Sprite Info: ";
			foreach (exLayeredSprite sortedSprite in sortedSpriteList)
			{
				UnityEngine.Debug.Log(string.Format("{4}: vertexBufferIndex: {0} vertexCount: {1} indexBufferIndex: {2} indexCount: {3} ", sortedSprite.vertexBufferIndex, sortedSprite.vertexCount, sortedSprite.indexBufferIndex, sortedSprite.indexCount, sortedSprite.gameObject.name), sortedSprite);
			}
			UnityEngine.Debug.Log(message, this);
		}
		Vector3[] buffer;
		if (outputBuffer)
		{
			string text2 = "Vertex Buffer: ";
			buffer = vertices.buffer;
			foreach (Vector3 vector in buffer)
			{
				string text3 = text2;
				Vector3 vector2 = vector;
				text2 = text3 + vector2.ToString();
				text2 += " ";
			}
			UnityEngine.Debug.Log(text2, this);
		}
		string text4 = "Mesh.vertices[" + meshBuffer.vertexCount + "]: ";
		buffer = meshBuffer.vertices;
		foreach (Vector3 vector3 in buffer)
		{
			text4 += vector3.ToString("F3");
			text4 += ", ";
		}
		UnityEngine.Debug.Log(text4, this);
		int[] buffer2;
		if (outputBuffer)
		{
			string text5 = "Index Buffer: ";
			buffer2 = indices.buffer;
			foreach (int num2 in buffer2)
			{
				text5 += num2;
				text5 += ",";
			}
			UnityEngine.Debug.Log(text5, this);
		}
		string text6 = "Mesh.indices[" + meshBuffer.triangles.Length + "]: ";
		buffer2 = meshBuffer.triangles;
		foreach (int num3 in buffer2)
		{
			text6 += num3;
			text6 += ",";
		}
		UnityEngine.Debug.Log(text6, this);
		Vector2[] buffer3;
		if (outputBuffer)
		{
			string text7 = "UV Buffer[" + uvs.buffer.Length + "]: ";
			buffer3 = uvs.buffer;
			foreach (Vector2 vector4 in buffer3)
			{
				string text8 = text7;
				Vector2 vector5 = vector4;
				text7 = text8 + vector5.ToString();
				text7 += ",";
			}
			UnityEngine.Debug.Log(text7, this);
		}
		string text9 = "Mesh.uvs[" + meshBuffer.uv.Length + "]: ";
		buffer3 = meshBuffer.uv;
		foreach (Vector2 vector6 in buffer3)
		{
			text9 += vector6.ToString("F4");
			text9 += ",";
		}
		UnityEngine.Debug.Log(text9, this);
		text9 = "Mesh.colors: ";
		Color[] colors = meshBuffer.colors;
		foreach (Color color in colors)
		{
			string text10 = text9;
			Color color2 = color;
			text9 = text10 + color2.ToString();
			text9 += ",";
		}
		UnityEngine.Debug.Log(text9, this);
	}

	public void Clear()
	{
		spriteList.Clear();
		sortedSpriteList.Clear();
		vertices.Clear();
		indices.Clear();
		uvs.Clear();
		colors32.Clear();
		if (mesh0 != null)
		{
			mesh0.Clear();
		}
		if (mesh1 != null)
		{
			mesh1.Clear();
		}
		updateFlags = exUpdateFlags.None;
	}

	private Mesh SwapMeshBuffer()
	{
		if (enableDoubleBuffer)
		{
			isEvenMeshBuffer = !isEvenMeshBuffer;
			exUpdateFlags exUpdateFlags2 = updateFlags;
			updateFlags |= lastUpdateFlags;
			lastUpdateFlags = exUpdateFlags2;
		}
		return GetMeshBuffer();
	}

	private Mesh GetMeshBuffer()
	{
		if (isEvenMeshBuffer)
		{
			if (mesh0 == null)
			{
				mesh0 = CreateMesh();
			}
			if (cachedFilter.sharedMesh != mesh0)
			{
				cachedFilter.sharedMesh = mesh0;
			}
			return mesh0;
		}
		if (mesh1 == null)
		{
			mesh1 = CreateMesh();
		}
		if (cachedFilter.sharedMesh != mesh1)
		{
			cachedFilter.sharedMesh = mesh1;
		}
		return mesh1;
	}

	private Mesh CreateMesh()
	{
		return new Mesh
		{
			hideFlags = HideFlags.DontSave
		};
	}

	private void Init()
	{
		if (cachedFilter == null)
		{
			cachedFilter = gameObject.GetComponent<MeshFilter>();
		}
		if (mesh0 == null)
		{
			mesh0 = CreateMesh();
		}
		if (cachedRenderer == null)
		{
			cachedRenderer = gameObject.GetComponent<MeshRenderer>();
			cachedRenderer.receiveShadows = false;
		}
	}

	[Conditional("EX_DEBUG")]
	public void UpdateDebugName(exLayer layer = null)
	{
	}
}
