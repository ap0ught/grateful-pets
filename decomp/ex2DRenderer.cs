using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("ex2D/2D Renderer")]
public class ex2DRenderer : MonoBehaviour
{
	public List<exLayer> layerList = new List<exLayer>();

	[SerializeField]
	private bool customizeLayerZ_;

	[NonSerialized]
	private static ex2DRenderer instance_;

	[NonSerialized]
	private Camera cachedCamera_;

	private static Dictionary<MaterialTableKey, Material> materialTable = new Dictionary<MaterialTableKey, Material>(MaterialTableKey.Comparer.instance);

	public bool customizeLayerZ
	{
		get
		{
			return customizeLayerZ_;
		}
		set
		{
			if (customizeLayerZ_ != value)
			{
				customizeLayerZ_ = value;
				ResortLayerDepth();
			}
		}
	}

	public static ex2DRenderer instance
	{
		get
		{
			if (instance_ == null)
			{
				instance_ = UnityEngine.Object.FindFirstObjectByType<ex2DRenderer>();
			}
			return instance_;
		}
	}

	public Camera cachedCamera
	{
		get
		{
			if (cachedCamera_ == null)
			{
				cachedCamera_ = GetComponent<Camera>();
			}
			return cachedCamera_;
		}
	}

	private void OnEnable()
	{
		if (instance_ == null)
		{
			instance_ = this;
		}
		for (int i = 0; i < layerList.Count; i++)
		{
			exLayer exLayer2 = layerList[i];
			if (exLayer2 != null)
			{
				exLayer2.GenerateMeshes();
			}
		}
		ResortLayerDepth();
	}

	private void OnDisable()
	{
		for (int i = 0; i < layerList.Count; i++)
		{
			exLayer exLayer2 = layerList[i];
			if (exLayer2 != null)
			{
				exLayer2.DestroyMeshes();
			}
		}
		if ((object)this == instance_)
		{
			instance_ = null;
		}
	}

	private void OnDestroy()
	{
		cachedCamera_ = null;
	}

	private void LateUpdate()
	{
		for (int num = layerList.Count - 1; num >= 0; num--)
		{
			if (layerList[num] == null)
			{
				layerList.RemoveAt(num);
			}
		}
		UpdateLayers();
	}

	public exLayer CreateLayer(string name = "New Layer", int _idx = -1)
	{
		exLayer exLayer2 = new GameObject(name).AddComponent<exLayer>();
		if (_idx == -1)
		{
			layerList.Add(exLayer2);
		}
		else
		{
			layerList.Insert(_idx, exLayer2);
		}
		ResortLayerDepth();
		return exLayer2;
	}

	public void DestroyLayer(int _idx)
	{
		DestroyLayer(layerList[_idx]);
	}

	public void DestroyLayer(exLayer _layer)
	{
		layerList.Remove(_layer);
		if (_layer != null)
		{
			_layer.gameObject.Destroy();
		}
	}

	public void InsertLayer(int _idx, exLayer _layer)
	{
		_idx = Mathf.Clamp(_idx, 0, layerList.Count);
		if (layerList.Contains(_layer))
		{
			Debug.LogWarning("Layer already exists in ex2DRenderer: " + _layer, _layer);
			return;
		}
		layerList.Insert(_idx, _layer);
		_layer.GenerateMeshes();
		ResortLayerDepth();
	}

	public bool HasLayer(exLayer _layer)
	{
		return layerList.Contains(_layer);
	}

	public exLayer GetLayer(string _layerName)
	{
		for (int i = 0; i < layerList.Count; i++)
		{
			exLayer exLayer2 = layerList[i];
			if (exLayer2 != null && exLayer2.name == _layerName)
			{
				return exLayer2;
			}
		}
		return null;
	}

	public void ResortLayerDepth()
	{
		float num = cachedCamera.transform.position.z + cachedCamera.nearClipPlane;
		float num2 = (cachedCamera.farClipPlane - cachedCamera.nearClipPlane) / (float)(layerList.Count + 1);
		float num3 = num + num2;
		for (int i = 0; i < layerList.Count; i++)
		{
			exLayer exLayer2 = layerList[i];
			if (exLayer2 != null)
			{
				if (customizeLayerZ_)
				{
					exLayer2.SetWorldBoundsZMin(exLayer2.customZ);
					continue;
				}
				exLayer2.SetWorldBoundsZMin(num3);
				num3 += num2;
			}
		}
	}

	public static Material GetMaterial(Shader _shader, Texture _texture)
	{
		if (_shader == null)
		{
			_shader = Shader.Find("ex2D/Alpha Blended");
			if (_shader == null)
			{
				return null;
			}
		}
		MaterialTableKey key = new MaterialTableKey(_shader, _texture);
		if (!materialTable.TryGetValue(key, out var value) || value == null)
		{
			value = new Material(_shader);
			value.hideFlags = HideFlags.DontSave;
			value.mainTexture = _texture;
			materialTable[key] = value;
		}
		return value;
	}

	public void ForceRenderScene()
	{
		for (int i = 0; i < layerList.Count; i++)
		{
			exLayer exLayer2 = layerList[i];
			if (exLayer2 != null)
			{
				exLayer2.DestroyMeshes();
				exLayer2.GenerateMeshes();
			}
		}
		ResortLayerDepth();
		UpdateLayers();
	}

	public void UpdateLayers()
	{
		for (int i = 0; i < layerList.Count; i++)
		{
			exLayer exLayer2 = layerList[i];
			if (exLayer2 != null)
			{
				exLayer2.UpdateSprites();
			}
		}
	}

	public void ResetCamera(bool exclusiveCamera)
	{
		if (!cachedCamera.orthographic)
		{
			cachedCamera.orthographic = true;
		}
		if (cachedCamera.orthographicSize != (float)Screen.height / 2f)
		{
			cachedCamera.orthographicSize = (float)Screen.height / 2f;
		}
		cachedCamera.transform.rotation = Quaternion.identity;
		cachedCamera.transform.SetLossyScale(Vector3.one);
	}
}
