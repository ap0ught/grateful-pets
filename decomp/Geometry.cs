using System.Collections.Generic;
using UnityEngine;

public class Geometry
{
	public enum Plane2Orientation
	{
		Horizontal,
		Vertical
	}

	public enum Plane2AnchorPoint
	{
		TopLeft,
		TopHalf,
		TopRight,
		RightHalf,
		BottomRight,
		BottomHalf,
		BottomLeft,
		LeftHalf,
		Center
	}

	public static void SetIsoMeshQuadTexture(GameObject Mesh, int widthSegments, int heightSegments, int x, int y, string Atlas, string Sprite)
	{
	}

	public static GameObject CreateIsoMesh(float width, float height, int widthSegments, int heightSegments, Plane2AnchorPoint anchor = Plane2AnchorPoint.BottomLeft, string optionalName = "IsoMesh", bool addCollider = false)
	{
		GameObject gameObject = new GameObject();
		if (!string.IsNullOrEmpty(optionalName))
		{
			gameObject.name = optionalName;
		}
		else
		{
			gameObject.name = "IsoPlane";
		}
		gameObject.transform.position = Vector3.zero;
		Vector2 vector = anchor switch
		{
			Plane2AnchorPoint.TopLeft => new Vector2((0f - width) / 2f, height / 2f), 
			Plane2AnchorPoint.TopHalf => new Vector2(0f, height / 2f), 
			Plane2AnchorPoint.TopRight => new Vector2(width / 2f, height / 2f), 
			Plane2AnchorPoint.RightHalf => new Vector2(width / 2f, 0f), 
			Plane2AnchorPoint.BottomRight => new Vector2(width / 2f, (0f - height) / 2f), 
			Plane2AnchorPoint.BottomHalf => new Vector2(0f, (0f - height) / 2f), 
			Plane2AnchorPoint.BottomLeft => new Vector2((0f - width) / 2f, (0f - height) / 2f), 
			Plane2AnchorPoint.LeftHalf => new Vector2((0f - width) / 2f, 0f), 
			_ => Vector2.zero, 
		};
		MeshFilter meshFilter = (MeshFilter)gameObject.AddComponent(typeof(MeshFilter));
		gameObject.AddComponent(typeof(MeshRenderer));
		Mesh mesh = new Mesh();
		mesh.name = gameObject.name;
		int num = widthSegments * heightSegments * 6;
		int num2 = widthSegments * heightSegments * 4;
		Vector3[] array = new Vector3[num2];
		Vector2[] array2 = new Vector2[num2];
		int[] array3 = new int[num];
		int num3 = 0;
		float num4 = 1f / (float)widthSegments;
		float num5 = 1f / (float)heightSegments;
		float num6 = width / (float)widthSegments;
		float num7 = height / (float)heightSegments / 2f;
		bool flag = false;
		for (float num8 = 0f; num8 < (float)heightSegments; num8++)
		{
			float num9 = 0f;
			if (flag)
			{
				num9 = num6 / 2f;
			}
			for (float num10 = 0f; num10 < (float)widthSegments; num10++)
			{
				array[num3] = new Vector3(num10 * num6 - width / 2f - vector.x + num9, (num8 + 2f) * num7 - height / 2f - vector.y, 0f);
				array2[num3] = new Vector2(num10 * num4, num8 * num5);
				array[num3 + 1] = new Vector3(num10 * num6 - width / 2f - vector.x + num9 - num6 / 2f, (num8 + 2f) * num7 - height / 2f - vector.y - num7, 0f);
				array2[num3 + 1] = new Vector2(num10 * num4, num8 * num5);
				array[num3 + 2] = new Vector3(num10 * num6 - width / 2f - vector.x + num9 + num6 / 2f, (num8 + 2f) * num7 - height / 2f - vector.y - num7, 0f);
				array2[num3 + 2] = new Vector2(num10 * num4, num8 * num5);
				array[num3 + 3] = new Vector3(num10 * num6 - width / 2f - vector.x + num9, (num8 + 2f) * num7 - height / 2f - vector.y - num7 * 2f, 0f);
				array2[num3 + 3] = new Vector2(num10 * num4, num8 * num5);
				num3 += 4;
			}
			flag = !flag;
		}
		num3 = 0;
		int num11 = 0;
		for (int i = 0; i < heightSegments; i++)
		{
			for (int j = 0; j < widthSegments; j++)
			{
				array3[num3] = num11;
				array3[num3 + 1] = num11 + 1;
				array3[num3 + 2] = num11 + 2;
				array3[num3 + 3] = num11 + 1;
				array3[num3 + 4] = num11 + 3;
				array3[num3 + 5] = num11 + 2;
				num3 += 6;
				num11 += 4;
			}
		}
		mesh.vertices = array;
		mesh.uv = array2;
		mesh.triangles = array3;
		mesh.RecalculateNormals();
		meshFilter.sharedMesh = mesh;
		mesh.RecalculateBounds();
		if (addCollider)
		{
			gameObject.AddComponent(typeof(BoxCollider));
		}
		return gameObject;
	}

	public static GameObject CreateIsoPlane(float width, float height, int widthSegments, int heightSegments, Plane2AnchorPoint anchor = Plane2AnchorPoint.BottomLeft, string optionalName = null, bool addCollider = false)
	{
		GameObject gameObject = new GameObject();
		if (!string.IsNullOrEmpty(optionalName))
		{
			gameObject.name = optionalName;
		}
		else
		{
			gameObject.name = "IsoPlane";
		}
		gameObject.transform.position = Vector3.zero;
		Vector2 vector = anchor switch
		{
			Plane2AnchorPoint.TopLeft => new Vector2((0f - width) / 2f, height / 2f), 
			Plane2AnchorPoint.TopHalf => new Vector2(0f, height / 2f), 
			Plane2AnchorPoint.TopRight => new Vector2(width / 2f, height / 2f), 
			Plane2AnchorPoint.RightHalf => new Vector2(width / 2f, 0f), 
			Plane2AnchorPoint.BottomRight => new Vector2(width / 2f, (0f - height) / 2f), 
			Plane2AnchorPoint.BottomHalf => new Vector2(0f, (0f - height) / 2f), 
			Plane2AnchorPoint.BottomLeft => new Vector2((0f - width) / 2f, (0f - height) / 2f), 
			Plane2AnchorPoint.LeftHalf => new Vector2((0f - width) / 2f, 0f), 
			_ => Vector2.zero, 
		};
		MeshFilter meshFilter = (MeshFilter)gameObject.AddComponent(typeof(MeshFilter));
		gameObject.AddComponent(typeof(MeshRenderer));
		Mesh mesh = new Mesh();
		mesh.name = gameObject.name;
		int num = widthSegments + 1;
		int num2 = heightSegments + 2;
		int num3 = widthSegments * heightSegments * 6;
		int num4 = num * num2;
		Vector3[] array = new Vector3[num4];
		Vector2[] array2 = new Vector2[num4];
		int[] array3 = new int[num3];
		int num5 = 0;
		float num6 = 1f / (float)widthSegments;
		float num7 = 1f / (float)heightSegments;
		float num8 = width / (float)widthSegments;
		float num9 = height / (float)heightSegments / 2f;
		bool flag = false;
		for (float num10 = 0f; num10 < (float)num2; num10++)
		{
			float num11 = 0f;
			if (flag)
			{
				num11 = num8 / 2f;
			}
			for (float num12 = 0f; num12 < (float)num; num12++)
			{
				array[num5] = new Vector3(num12 * num8 - width / 2f - vector.x - num11, num10 * num9 - height / 2f - vector.y, 0f);
				array2[num5] = new Vector2(num12 * num6, num10 * num7);
				num5++;
			}
			flag = !flag;
		}
		num5 = 0;
		for (int i = 0; i < heightSegments; i++)
		{
			for (int j = 0; j < widthSegments; j++)
			{
				if (i % 2 == 0)
				{
					int num13 = (array3[num5] = i * num + j);
					array3[num5 + 1] = num13 + num;
					array3[num5 + 2] = num13 + num + 1;
					array3[num5 + 3] = num13 + num;
					array3[num5 + 4] = (i + 2) * num + j;
					array3[num5 + 5] = num13 + num + 1;
				}
				else
				{
					int num14 = (array3[num5] = i * num + j + 1);
					array3[num5 + 1] = num14 + num - 1;
					array3[num5 + 2] = num14 + num;
					array3[num5 + 3] = num14 + num - 1;
					array3[num5 + 4] = (i + 2) * num + 1 + j;
					array3[num5 + 5] = num14 + num;
				}
				num5 += 6;
			}
		}
		mesh.vertices = array;
		mesh.uv = array2;
		mesh.triangles = array3;
		mesh.RecalculateNormals();
		meshFilter.sharedMesh = mesh;
		mesh.RecalculateBounds();
		if (addCollider)
		{
			gameObject.AddComponent(typeof(BoxCollider));
		}
		return gameObject;
	}

	public static GameObject CreatePlane2(out Vector3[] vertices, float width, float length, int widthSegments, int lengthSegments, Plane2AnchorPoint anchor = Plane2AnchorPoint.BottomLeft, Plane2Orientation orientation = Plane2Orientation.Vertical, string optionalName = null, bool addCollider = false)
	{
		GameObject gameObject = new GameObject();
		if (!string.IsNullOrEmpty(optionalName))
		{
			gameObject.name = optionalName;
		}
		else
		{
			gameObject.name = "Plane";
		}
		gameObject.transform.position = Vector3.zero;
		Vector2 vector = anchor switch
		{
			Plane2AnchorPoint.TopLeft => new Vector2((0f - width) / 2f, length / 2f), 
			Plane2AnchorPoint.TopHalf => new Vector2(0f, length / 2f), 
			Plane2AnchorPoint.TopRight => new Vector2(width / 2f, length / 2f), 
			Plane2AnchorPoint.RightHalf => new Vector2(width / 2f, 0f), 
			Plane2AnchorPoint.BottomRight => new Vector2(width / 2f, (0f - length) / 2f), 
			Plane2AnchorPoint.BottomHalf => new Vector2(0f, (0f - length) / 2f), 
			Plane2AnchorPoint.BottomLeft => new Vector2((0f - width) / 2f, (0f - length) / 2f), 
			Plane2AnchorPoint.LeftHalf => new Vector2((0f - width) / 2f, 0f), 
			_ => Vector2.zero, 
		};
		MeshFilter meshFilter = (MeshFilter)gameObject.AddComponent(typeof(MeshFilter));
		gameObject.AddComponent(typeof(MeshRenderer));
		Mesh mesh = new Mesh();
		mesh.name = gameObject.name;
		int num = widthSegments + 1;
		int num2 = lengthSegments + 1;
		int num3 = widthSegments * lengthSegments * 6;
		int num4 = num * num2;
		vertices = new Vector3[num4];
		Vector2[] array = new Vector2[num4];
		Color[] array2 = new Color[num4];
		int[] array3 = new int[num3];
		int num5 = 0;
		float num6 = 1f / (float)widthSegments;
		float num7 = 1f / (float)lengthSegments;
		float num8 = width / (float)widthSegments;
		float num9 = length / (float)lengthSegments;
		for (float num10 = 0f; num10 < (float)num2; num10++)
		{
			for (float num11 = 0f; num11 < (float)num; num11++)
			{
				if (orientation == Plane2Orientation.Horizontal)
				{
					vertices[num5] = new Vector3(num11 * num8 - width / 2f - vector.x, 0f, num10 * num9 - length / 2f - vector.y);
				}
				else
				{
					vertices[num5] = new Vector3(num11 * num8 - width / 2f - vector.x, num10 * num9 - length / 2f - vector.y, 0f);
				}
				array2[num5] = new Color(1f, 1f, 1f, 1f);
				array[num5++] = new Vector2(num11 * num6, num10 * num7);
			}
		}
		num5 = 0;
		for (int i = 0; i < lengthSegments; i++)
		{
			for (int j = 0; j < widthSegments; j++)
			{
				array3[num5] = i * num + j;
				array3[num5 + 1] = (i + 1) * num + j;
				array3[num5 + 2] = i * num + j + 1;
				array3[num5 + 3] = (i + 1) * num + j;
				array3[num5 + 4] = (i + 1) * num + j + 1;
				array3[num5 + 5] = i * num + j + 1;
				num5 += 6;
			}
		}
		mesh.vertices = vertices;
		mesh.colors = array2;
		mesh.uv = array;
		mesh.triangles = array3;
		mesh.RecalculateNormals();
		meshFilter.sharedMesh = mesh;
		mesh.RecalculateBounds();
		if (addCollider)
		{
			gameObject.AddComponent(typeof(BoxCollider));
		}
		return gameObject;
	}

	public static Mesh CreateQuadMesh(float Size)
	{
		Mesh mesh = new Mesh();
		Vector3[] vertices = new Vector3[4]
		{
			new Vector3(Size, Size, 0f),
			new Vector3(Size, 0f - Size, 0f),
			new Vector3(0f - Size, Size, 0f),
			new Vector3(0f - Size, 0f - Size, 0f)
		};
		Vector2[] uv = new Vector2[4]
		{
			new Vector2(1f, 1f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(0f, 0f)
		};
		int[] triangles = new int[6] { 0, 1, 2, 2, 1, 3 };
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
		return mesh;
	}

	public static Mesh CreatePlane(int tileHeight, int tileWidth, int gridHeight, int gridWidth)
	{
		Mesh mesh = new Mesh();
		float tileSizeX = 1f;
		float tileSizeY = 1f;
		List<Vector3> list = new List<Vector3>();
		List<int> list2 = new List<int>();
		List<Vector3> list3 = new List<Vector3>();
		List<Color> list4 = new List<Color>();
		List<Vector2> list5 = new List<Vector2>();
		int index = 0;
		for (int i = 0; i < gridWidth; i++)
		{
			for (int j = 0; j < gridHeight; j++)
			{
				AddVertices(tileHeight, tileWidth, j, i, list);
				index = AddTriangles(index, list2);
				AddNormals(list3);
				AddColors(list4);
				AddUvs(0, tileSizeY, tileSizeX, list5, 0);
			}
		}
		mesh.vertices = list.ToArray();
		mesh.normals = list3.ToArray();
		mesh.triangles = list2.ToArray();
		mesh.colors = list4.ToArray();
		mesh.uv = list5.ToArray();
		mesh.RecalculateNormals();
		return mesh;
	}

	private static void AddVertices(int tileHeight, int tileWidth, int y, int x, ICollection<Vector3> vertices)
	{
		vertices.Add(new Vector3(x * tileWidth, y * tileHeight, 0f));
		vertices.Add(new Vector3(x * tileWidth + tileWidth, y * tileHeight, 0f));
		vertices.Add(new Vector3(x * tileWidth + tileWidth, y * tileHeight + tileHeight, 0f));
		vertices.Add(new Vector3(x * tileWidth, y * tileHeight + tileHeight, 0f));
	}

	private static int AddTriangles(int index, ICollection<int> triangles)
	{
		triangles.Add(index + 2);
		triangles.Add(index + 1);
		triangles.Add(index);
		triangles.Add(index);
		triangles.Add(index + 3);
		triangles.Add(index + 2);
		index += 4;
		return index;
	}

	private static void AddColors(ICollection<Color> colors)
	{
		colors.Add(new Color(0.2f, 0.3f, 0.4f, 0.9f));
		colors.Add(new Color(0.2f, 0.3f, 0.4f, 0.9f));
		colors.Add(new Color(0.2f, 0.3f, 0.4f, 0.9f));
		colors.Add(new Color(0.2f, 0.3f, 0.4f, 0.9f));
	}

	private static void AddNormals(ICollection<Vector3> normals)
	{
		normals.Add(Vector3.forward);
		normals.Add(Vector3.forward);
		normals.Add(Vector3.forward);
		normals.Add(Vector3.forward);
	}

	private static void AddUvs(int tileRow, float tileSizeY, float tileSizeX, ICollection<Vector2> uvs, int tileColumn)
	{
		uvs.Add(new Vector2((float)tileColumn * tileSizeX, (float)tileRow * tileSizeY));
		uvs.Add(new Vector2((float)(tileColumn + 1) * tileSizeX, (float)tileRow * tileSizeY));
		uvs.Add(new Vector2((float)(tileColumn + 1) * tileSizeX, (float)(tileRow + 1) * tileSizeY));
		uvs.Add(new Vector2((float)tileColumn * tileSizeX, (float)(tileRow + 1) * tileSizeY));
	}
}
