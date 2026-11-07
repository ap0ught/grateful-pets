using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

[ExecuteInEditMode]
public class exLayer : MonoBehaviour
{
	public interface IFriendOfLayer
	{
		float globalDepth { get; set; }

		void DoSetLayer(exLayer _layer);

		void DoSetBufferSize(int _vertexCount, int _indexCount);

		void SetMaterialDirty();

		void ResetLayerProperties();
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct UpdateSpriteWhileRecreating
	{
		private static exLayeredSprite spriteNeedsToSetBufferSize;

		private static int newVertexCount;

		private static int newIndexCount;

		private static exLayeredSprite spriteNeedsToSetMaterialDirty;

		public static void RegisterNewBufferSize(exLayeredSprite _sprite, int _vertexCount, int _indexCount)
		{
			spriteNeedsToSetBufferSize = _sprite;
			newVertexCount = _vertexCount;
			newIndexCount = _indexCount;
		}

		public static void RegisterMaterialDirty(exLayeredSprite _sprite)
		{
			spriteNeedsToSetMaterialDirty = _sprite;
		}

		public static void TryUpdate(exLayeredSprite _sprite)
		{
			if ((object)_sprite == spriteNeedsToSetBufferSize)
			{
				((IFriendOfLayer)spriteNeedsToSetBufferSize).DoSetBufferSize(newVertexCount, newIndexCount);
				spriteNeedsToSetBufferSize = null;
			}
			if ((object)_sprite == spriteNeedsToSetMaterialDirty)
			{
				((IFriendOfLayer)spriteNeedsToSetMaterialDirty).SetMaterialDirty();
				spriteNeedsToSetMaterialDirty = null;
			}
		}

		public static bool Clear()
		{
			bool result = spriteNeedsToSetBufferSize == null && spriteNeedsToSetMaterialDirty == null;
			spriteNeedsToSetBufferSize = null;
			spriteNeedsToSetMaterialDirty = null;
			return result;
		}
	}

	public static int maxDynamicMeshVertex = 90000;

	[SerializeField]
	private bool show_ = true;

	[SerializeField]
	private bool ordered_;

	[SerializeField]
	private exLayerType layerType_ = exLayerType.Dynamic;

	[SerializeField]
	private float alpha_ = 1f;

	[SerializeField]
	private float zMin_;

	[NonSerialized]
	public List<exMesh> meshList = new List<exMesh>();

	[NonSerialized]
	private Transform cachedTransform_;

	[NonSerialized]
	private int nextSpriteUniqueId;

	[NonSerialized]
	private bool alphaHasChanged;

	[NonSerialized]
	private float zMin;

	[NonSerialized]
	private bool anyFontTextureRefreshed;

	[NonSerialized]
	private List<GameObject> depthDirtyGoList = new List<GameObject>();

	public bool show
	{
		get
		{
			return show_;
		}
		set
		{
			if (show_ == value)
			{
				return;
			}
			for (int i = 0; i < meshList.Count; i++)
			{
				exMesh exMesh2 = meshList[i];
				if (exMesh2 != null)
				{
					exMesh2.gameObject.SetActive(value && exMesh2.hasTriangle);
				}
			}
			show_ = value;
		}
	}

	public bool ordered
	{
		get
		{
			return ordered_;
		}
		set
		{
			if (ordered_ != value)
			{
				ordered_ = value;
				if (ordered_)
				{
					DestroyMeshes();
					GenerateMeshes();
				}
			}
		}
	}

	public exLayerType layerType
	{
		get
		{
			return layerType_;
		}
		set
		{
			if (layerType_ != value)
			{
				layerType_ = value;
				bool dynamic = value == exLayerType.Dynamic;
				for (int i = 0; i < meshList.Count; i++)
				{
					meshList[i].SetDynamic(dynamic);
				}
				if (value == exLayerType.Static)
				{
					Compact();
				}
			}
		}
	}

	public float alpha
	{
		get
		{
			return alpha_;
		}
		set
		{
			if (alpha_ != value)
			{
				alpha_ = value;
				alphaHasChanged = true;
			}
		}
	}

	public float customZ
	{
		get
		{
			return zMin_;
		}
		set
		{
			if (zMin_ != value)
			{
				zMin_ = value;
				SetWorldBoundsZMin(zMin_);
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
			}
			return cachedTransform_;
		}
	}

	private void OnDisable()
	{
		DestroyMeshes();
	}

	[ContextMenu("UpdateSprites")]
	public void UpdateSprites()
	{
		if (!show_)
		{
			return;
		}
		for (int num = meshList.Count - 1; num >= 0; num--)
		{
			exMesh exMesh2 = meshList[num];
			for (int num2 = exMesh2.spriteList.Count - 1; num2 >= 0; num2--)
			{
				exLayeredSprite exLayeredSprite2 = exMesh2.spriteList[num2];
				if (exLayeredSprite2 == null)
				{
					RemoveFromMesh(exLayeredSprite2, exMesh2);
				}
			}
		}
		int num3 = 0;
		while (num3 < meshList.Count)
		{
			exMesh exMesh3 = meshList[num3];
			bool flag = exMesh3 == null;
			if (flag || exMesh3.spriteList.Count == 0)
			{
				if (!flag)
				{
					exMesh3.gameObject.Destroy();
				}
				meshList.RemoveAt(num3);
				if (num3 - 1 >= 0 && num3 < meshList.Count)
				{
					int num4 = ((layerType_ == exLayerType.Dynamic) ? maxDynamicMeshVertex : 65000);
					if (meshList[num3 - 1].vertices.Count < num4)
					{
						ShiftSpritesDown(num3 - 1, num4, num4);
					}
				}
			}
			else
			{
				num3++;
			}
		}
		UpdateAllSpritesDepth();
		exUpdateFlags exUpdateFlags2 = (alphaHasChanged ? exUpdateFlags.Color : exUpdateFlags.None);
		alphaHasChanged = false;
		for (int num5 = meshList.Count - 1; num5 >= 0; num5--)
		{
			exMesh exMesh4 = meshList[num5];
			exUpdateFlags exUpdateFlags3 = exUpdateFlags.None;
			for (int i = 0; i < exMesh4.spriteList.Count; i++)
			{
				exLayeredSprite exLayeredSprite3 = exMesh4.spriteList[i];
				exLayeredSprite3.updateFlags |= exUpdateFlags2;
				if (exLayeredSprite3.isInIndexBuffer)
				{
					exLayeredSprite3.UpdateTransform();
					if (exLayeredSprite3.updateFlags != exUpdateFlags.None)
					{
						exUpdateFlags exUpdateFlags4 = exLayeredSprite3.UpdateBuffers(exMesh4.vertices, exMesh4.uvs, exMesh4.colors32, exMesh4.indices);
						exUpdateFlags3 |= exUpdateFlags4;
					}
				}
			}
			exMesh4.Apply(exUpdateFlags3);
		}
		if (!anyFontTextureRefreshed)
		{
			return;
		}
		anyFontTextureRefreshed = false;
		for (int num6 = meshList.Count - 1; num6 >= 0; num6--)
		{
			exMesh exMesh5 = meshList[num6];
			exUpdateFlags exUpdateFlags5 = exUpdateFlags.None;
			for (int j = 0; j < exMesh5.spriteList.Count; j++)
			{
				exLayeredSprite exLayeredSprite4 = exMesh5.spriteList[j];
				if (exLayeredSprite4.isInIndexBuffer && exLayeredSprite4.updateFlags != exUpdateFlags.None)
				{
					exUpdateFlags exUpdateFlags6 = exLayeredSprite4.UpdateBuffers(exMesh5.vertices, exMesh5.uvs, exMesh5.colors32, exMesh5.indices);
					exUpdateFlags5 |= exUpdateFlags6;
				}
			}
			exMesh5.Apply(exUpdateFlags5);
		}
	}

	public void Add(exLayeredSprite _sprite, bool _recursively = true)
	{
		if (_recursively)
		{
			exLayer layer = _sprite.layer;
			if ((object)layer != this)
			{
				if (layer != null)
				{
					layer.Remove(_sprite);
				}
				exLayeredSprite[] componentsInChildren = _sprite.GetComponentsInChildren<exLayeredSprite>(includeInactive: true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					DoAddSprite(componentsInChildren[i], _newSprite: true);
				}
				if (!_sprite.cachedTransform.IsChildOf(cachedTransform))
				{
					_sprite.cachedTransform.parent = cachedTransform_;
				}
			}
		}
		else
		{
			DoAddSprite(_sprite, _newSprite: true);
		}
	}

	public void Add(GameObject _gameObject)
	{
		exLayeredSprite[] componentsInChildren = _gameObject.GetComponentsInChildren<exLayeredSprite>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			DoAddSprite(componentsInChildren[i], _newSprite: true);
		}
		if (!_gameObject.transform.IsChildOf(cachedTransform))
		{
			_gameObject.transform.parent = cachedTransform_;
		}
	}

	public void Remove(exLayeredSprite _sprite, bool _recursively = true)
	{
		if (_recursively)
		{
			Remove(_sprite.gameObject, _recursively);
			return;
		}
		if (_sprite.layer != this)
		{
			UnityEngine.Debug.LogWarning("Sprite not in this layer.");
			return;
		}
		int num = IndexOfMesh(_sprite);
		if (num != -1)
		{
			RemoveFromMesh(_sprite, meshList[num]);
			((IFriendOfLayer)_sprite).DoSetLayer((exLayer)null);
		}
		else
		{
			((IFriendOfLayer)_sprite).ResetLayerProperties();
		}
		if (_sprite.spriteIdInLayer == nextSpriteUniqueId - 1 && nextSpriteUniqueId > 0)
		{
			nextSpriteUniqueId--;
		}
		_sprite.spriteIdInLayer = -1;
	}

	public void Remove(GameObject _gameObject, bool _recursively = true)
	{
		if (_recursively)
		{
			exLayeredSprite[] componentsInChildren = _gameObject.GetComponentsInChildren<exLayeredSprite>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Remove(componentsInChildren[i], _recursively: false);
			}
		}
		else
		{
			exLayeredSprite component = _gameObject.GetComponent<exLayeredSprite>();
			if (component != null)
			{
				Remove(component, _recursively: false);
			}
		}
	}

	internal void ShowSprite(exLayeredSprite _sprite)
	{
		if (!_sprite.isInIndexBuffer)
		{
			int num = IndexOfMesh(_sprite);
			if (num != -1)
			{
				AddIndices(meshList[num], _sprite);
			}
		}
		else
		{
			_sprite.transparent = false;
		}
	}

	internal void HideSprite(exLayeredSprite _sprite)
	{
		if (_sprite.isInIndexBuffer)
		{
			int num = IndexOfMesh(_sprite);
			if (num != -1)
			{
				RemoveIndices(meshList[num], _sprite);
			}
		}
	}

	internal void FastShowSprite(exLayeredSprite _sprite)
	{
		if (!_sprite.isInIndexBuffer)
		{
			ShowSprite(_sprite);
		}
		else
		{
			_sprite.transparent = false;
		}
	}

	internal void FastHideSprite(exLayeredSprite _sprite)
	{
		_sprite.transparent = true;
	}

	public void Compact()
	{
		float z = 0f;
		for (int i = 0; i < meshList.Count; i++)
		{
			exMesh exMesh2 = meshList[i];
			if (exMesh2 != null)
			{
				z = exMesh2.transform.position.z;
				break;
			}
		}
		DestroyMeshes();
		GenerateMeshes();
		meshList.TrimExcess();
		depthDirtyGoList.TrimExcess();
		for (int j = 0; j < meshList.Count; j++)
		{
			exMesh exMesh3 = meshList[j];
			if (exMesh3 != null)
			{
				exMesh3.transform.position = new Vector3(0f, 0f, z);
			}
		}
	}

	public void SetWorldBoundsZMin(float _zMin)
	{
		zMin = _zMin;
		ResortMeshes();
	}

	public void GenerateMeshes()
	{
		int num = 0;
		for (int num2 = meshList.Count - 1; num2 >= 0; num2--)
		{
			exMesh exMesh2 = meshList[num2];
			if (exMesh2 != null)
			{
				for (int num3 = exMesh2.spriteList.Count - 1; num3 >= 0; num3--)
				{
					int spriteIdInLayer = exMesh2.spriteList[num3].spriteIdInLayer;
					if (num < spriteIdInLayer)
					{
						num = spriteIdInLayer;
					}
				}
			}
		}
		exLayeredSprite[] componentsInChildren = GetComponentsInChildren<exLayeredSprite>(includeInactive: true);
		foreach (exLayeredSprite exLayeredSprite2 in componentsInChildren)
		{
			if ((object)exLayeredSprite2.layer != this)
			{
				exLayeredSprite2.spriteIdInLayer += num;
				DoAddSprite(exLayeredSprite2, _newSprite: false);
			}
		}
	}

	public void DestroyMeshes()
	{
		exLayeredSprite[] componentsInChildren = GetComponentsInChildren<exLayeredSprite>(includeInactive: true);
		foreach (exLayeredSprite exLayeredSprite2 in componentsInChildren)
		{
			if (exLayeredSprite2 != null)
			{
				if ((object)exLayeredSprite2.layer != this && exLayeredSprite2.layer != null)
				{
					UnityEngine.Debug.LogError("Sprite's hierarchy is invalid!", exLayeredSprite2);
				}
				((IFriendOfLayer)exLayeredSprite2).ResetLayerProperties();
			}
		}
		for (int num = meshList.Count - 1; num >= 0; num--)
		{
			exMesh exMesh2 = meshList[num];
			if (exMesh2 != null)
			{
				exMesh2.gameObject.DestroyImmediate();
			}
		}
		meshList.Clear();
		nextSpriteUniqueId = 0;
	}

	internal void OnFontTextureRebuilt()
	{
		anyFontTextureRefreshed = true;
	}

	public void SetDepthDirty(GameObject _go)
	{
		Transform transform = _go.transform;
		for (int num = depthDirtyGoList.Count - 1; num >= 0; num--)
		{
			GameObject gameObject = depthDirtyGoList[num];
			if ((object)gameObject == _go)
			{
				return;
			}
			if (gameObject == null)
			{
				depthDirtyGoList.RemoveAt(num);
			}
			else
			{
				Transform transform2 = gameObject.transform;
				if (transform.IsChildOf(transform2))
				{
					return;
				}
				if (transform2.IsChildOf(transform))
				{
					depthDirtyGoList.RemoveAt(num);
				}
			}
		}
		depthDirtyGoList.Add(_go);
	}

	private void UpdateSpriteDepth(exLayeredSprite _sprite, float _parentGlobalDepth)
	{
		float num = _parentGlobalDepth + _sprite.depth;
		if (((IFriendOfLayer)_sprite).globalDepth == num)
		{
			return;
		}
		((IFriendOfLayer)_sprite).globalDepth = num;
		int num2 = IndexOfMesh(_sprite);
		exMesh exMesh2 = meshList[num2];
		if (IsRenderOrderChangedAmongMeshes(_sprite, num2))
		{
			RemoveFromMesh(_sprite, exMesh2);
			UpdateSpriteWhileRecreating.TryUpdate(_sprite);
			AddToMesh(_sprite, GetMeshToAdd(_sprite));
			return;
		}
		int oldSortedSpriteIndex = exMesh2.sortedSpriteList.IndexOf(_sprite);
		if (IsRenderOrderChangedInMesh(_sprite, num2, oldSortedSpriteIndex))
		{
			RemoveFromMesh(_sprite, exMesh2);
			UpdateSpriteWhileRecreating.TryUpdate(_sprite);
			AddToMesh(_sprite, exMesh2);
		}
	}

	private float GetParentGlobalDepth(GameObject _go)
	{
		exLayeredSprite componentUpwards = _go.GetComponentUpwards<exLayeredSprite>();
		if (!(componentUpwards != null))
		{
			return 0f;
		}
		return ((IFriendOfLayer)componentUpwards).globalDepth;
	}

	private void UpdateSpriteDepthRecursively(GameObject _go, float _parentGlobalDepth)
	{
		exLayeredSprite exLayeredSprite2 = _go.GetComponent(typeof(exLayeredSprite)) as exLayeredSprite;
		if (exLayeredSprite2 != null)
		{
			UpdateSpriteDepth(exLayeredSprite2, _parentGlobalDepth);
		}
		float parentGlobalDepth = ((exLayeredSprite2 != null) ? ((IFriendOfLayer)exLayeredSprite2).globalDepth : _parentGlobalDepth);
		Transform transform = _go.transform;
		int i = 0;
		for (int childCount = transform.childCount; i < childCount; i++)
		{
			UpdateSpriteDepthRecursively(transform.GetChild(i).gameObject, parentGlobalDepth);
		}
	}

	internal void SetSpriteBufferSize(exLayeredSprite _sprite, int _vertexCount, int _indexCount)
	{
		UpdateSpriteWhileRecreating.RegisterNewBufferSize(_sprite, _vertexCount, _indexCount);
		UpdateAllSpritesDepth();
		if (!UpdateSpriteWhileRecreating.Clear())
		{
			int num = IndexOfMesh(_sprite);
			if (num != -1)
			{
				RemoveFromMesh(_sprite, meshList[num]);
			}
			((IFriendOfLayer)_sprite).DoSetBufferSize(_vertexCount, _indexCount);
			AddToMesh(_sprite, GetMeshToAdd(_sprite));
		}
	}

	internal void RefreshSpriteMaterial(exLayeredSprite _sprite)
	{
		UpdateSpriteWhileRecreating.RegisterMaterialDirty(_sprite);
		UpdateAllSpritesDepth();
		if (!UpdateSpriteWhileRecreating.Clear())
		{
			int num = IndexOfMesh(_sprite);
			if (num != -1)
			{
				RemoveFromMesh(_sprite, meshList[num]);
			}
			((IFriendOfLayer)_sprite).SetMaterialDirty();
			exMesh meshToAdd = GetMeshToAdd(_sprite);
			AddToMesh(_sprite, meshToAdd);
		}
	}

	[Conditional("UNITY_EDITOR")]
	public void UpdateNowInEditMode()
	{
	}

	public int IndexOfMesh(exLayeredSprite _sprite)
	{
		Material material = _sprite.material;
		for (int i = 0; i < meshList.Count; i++)
		{
			exMesh exMesh2 = meshList[i];
			if ((object)exMesh2.material == material && exMesh2 != null && _sprite.spriteIndexInMesh >= 0 && _sprite.spriteIndexInMesh < exMesh2.spriteList.Count && (object)exMesh2.spriteList[_sprite.spriteIndexInMesh] == _sprite)
			{
				return i;
			}
		}
		return -1;
	}

	private exMesh CreateNewMesh(Material _mat, int _index)
	{
		exMesh exMesh2 = exMesh.Create(this);
		exMesh2.material = _mat;
		exMesh2.SetDynamic(layerType_ == exLayerType.Dynamic);
		meshList.Insert(_index, exMesh2);
		ResortMeshes(_index);
		return exMesh2;
	}

	private exMesh GetNewMesh(Material _mat, int _index)
	{
		int i = 0;
		for (int count = meshList.Count; i < count; i++)
		{
			exMesh exMesh2 = meshList[i];
			if (exMesh2.spriteList.Count == 0 && exMesh2 != null)
			{
				exMesh2.material = _mat;
				if (i < _index)
				{
					meshList.RemoveAt(i);
					meshList.Insert(_index - 1, exMesh2);
					ResortMeshes(_index - 1);
				}
				else if (i > _index)
				{
					meshList.RemoveAt(i);
					meshList.Insert(_index, exMesh2);
					ResortMeshes(_index);
				}
				return exMesh2;
			}
		}
		return CreateNewMesh(_mat, _index);
	}

	private void ResortMeshes(int _startIndex = 0)
	{
		float num = 0.01f;
		float num2 = zMin - (float)_startIndex * num;
		int i = _startIndex;
		for (int count = meshList.Count; i < count; i++)
		{
			exMesh exMesh2 = meshList[i];
			if (exMesh2 != null)
			{
				exMesh2.transform.position = new Vector3(0f, 0f, num2);
			}
			num2 -= num;
		}
	}

	[Conditional("UNITY_EDITOR")]
	public void CheckDuplicated(exLayeredSprite _sprite)
	{
	}

	private void ShiftSprite(exMesh _src, exMesh _dst, exLayeredSprite _sprite)
	{
		RemoveFromMesh(_sprite, _src);
		AddToMesh(_sprite, _dst);
	}

	private void ShiftSpritesUp(int _meshIndex, int _newVertexCount, int _maxVertexCount)
	{
		exMesh exMesh2 = meshList[_meshIndex];
		int num = exMesh2.vertices.Count - _newVertexCount;
		int num2 = 0;
		for (int num3 = exMesh2.sortedSpriteList.Count - 1; num3 >= 0; num3--)
		{
			exLayeredSprite exLayeredSprite2 = exMesh2.sortedSpriteList[num3];
			num2 += exLayeredSprite2.vertexCount;
			if (num2 >= num)
			{
				exMesh exMesh3;
				if (_meshIndex == meshList.Count - 1 || (object)meshList[_meshIndex + 1].material != exMesh2.material)
				{
					exMesh3 = GetNewMesh(exMesh2.material, _meshIndex + 1);
				}
				else
				{
					exMesh3 = meshList[_meshIndex + 1];
					if (exMesh3.vertices.Count + num2 > _maxVertexCount)
					{
						ShiftSpritesUp(_meshIndex + 1, _maxVertexCount - num2, _maxVertexCount);
					}
				}
				for (int num4 = exMesh2.sortedSpriteList.Count - 1; num4 >= num3; num4--)
				{
					ShiftSprite(exMesh2, exMesh3, exMesh2.sortedSpriteList[num4]);
				}
				break;
			}
		}
	}

	private void ShiftSpritesDown(int _meshIndex, int _newVertexCount, int _maxVertexCount)
	{
		for (int i = _meshIndex + 1; i < meshList.Count; i++)
		{
			exMesh exMesh2 = meshList[i];
			exMesh exMesh3 = meshList[i - 1];
			if ((object)exMesh3.material != exMesh2.material)
			{
				break;
			}
			int num = ((i == _meshIndex + 1) ? _newVertexCount : _maxVertexCount);
			while (exMesh2.sortedSpriteList.Count > 0)
			{
				exLayeredSprite exLayeredSprite2 = exMesh2.sortedSpriteList[0];
				if (exMesh3.vertices.Count + exLayeredSprite2.vertexCount > num)
				{
					break;
				}
				ShiftSprite(exMesh2, exMesh3, exLayeredSprite2);
			}
		}
	}

	private int GetBelowVertexCountInMesh(int _meshIndex, exLayeredSprite _sprite, int _maxVertexCount, out int _aboveSpriteIndex)
	{
		exMesh exMesh2 = meshList[_meshIndex];
		_aboveSpriteIndex = exMesh2.sortedSpriteList.BinarySearch(_sprite);
		if (_aboveSpriteIndex < 0)
		{
			_aboveSpriteIndex = ~_aboveSpriteIndex;
		}
		else
		{
			_aboveSpriteIndex++;
		}
		if (_aboveSpriteIndex <= exMesh2.sortedSpriteList.Count)
		{
			int num = 0;
			for (int i = 0; i < _aboveSpriteIndex; i++)
			{
				num += exMesh2.sortedSpriteList[i].vertexCount;
			}
			return num;
		}
		return exMesh2.vertices.Count;
	}

	private exMesh GetShiftedMesh(int _meshIndex, exLayeredSprite _sprite, int _maxVertexCount)
	{
		exMesh exMesh2 = meshList[_meshIndex];
		int vertexCount = _sprite.vertexCount;
		GetBelowVertexCountInMesh(_meshIndex, _sprite, _maxVertexCount, out var _aboveSpriteIndex);
		int num = exMesh2.vertices.Count;
		for (int num2 = exMesh2.sortedSpriteList.Count - 1; num2 >= _aboveSpriteIndex; num2--)
		{
			exLayeredSprite exLayeredSprite2 = exMesh2.sortedSpriteList[num2];
			num -= exLayeredSprite2.vertexCount;
			if (num + vertexCount <= _maxVertexCount)
			{
				ShiftSpritesUp(_meshIndex, num, _maxVertexCount);
				return exMesh2;
			}
		}
		if (_meshIndex + 1 < meshList.Count)
		{
			int num3 = exMesh2.vertices.Count - num;
			int newVertexCount = _maxVertexCount - num3 - vertexCount;
			ShiftSpritesUp(_meshIndex + 1, newVertexCount, _maxVertexCount);
			ShiftSpritesUp(_meshIndex, num, _maxVertexCount);
			return meshList[_meshIndex + 1];
		}
		return exMesh2;
	}

	private void SplitMesh(int _meshIndex, exLayeredSprite _seperatorSprite, int _maxVertexCount)
	{
		int belowVertexCountInMesh = GetBelowVertexCountInMesh(_meshIndex, _seperatorSprite, _maxVertexCount, out var _);
		ShiftSpritesUp(_meshIndex, belowVertexCountInMesh, _maxVertexCount);
	}

	private exMesh GetMeshToAdd(exLayeredSprite _sprite)
	{
		Material material = _sprite.material;
		int num = ((layerType_ == exLayerType.Dynamic) ? maxDynamicMeshVertex : 65000);
		num -= _sprite.vertexCount;
		exMesh exMesh2 = null;
		for (int num2 = meshList.Count - 1; num2 >= 0; num2--)
		{
			exMesh exMesh3 = meshList[num2];
			if (!(exMesh3 == null) && exMesh3.sortedSpriteList.Count != 0)
			{
				exLayeredSprite exLayeredSprite2 = exMesh3.sortedSpriteList[exMesh3.sortedSpriteList.Count - 1];
				exLayeredSprite exLayeredSprite3 = exMesh3.sortedSpriteList[0];
				bool flag = !ordered_ && _sprite <= exLayeredSprite3;
				if (_sprite >= exLayeredSprite2)
				{
					if ((object)exMesh3.material == material && exMesh3.vertices.Count <= num)
					{
						return exMesh3;
					}
					if (!flag)
					{
						if ((object)exMesh2 != null && (object)exMesh2.material == material && exMesh2.vertices.Count <= num)
						{
							return exMesh2;
						}
						return GetNewMesh(material, num2 + 1);
					}
				}
				else if (_sprite >= exLayeredSprite3)
				{
					if ((object)exMesh3.material == material)
					{
						if (exMesh3.vertices.Count <= num)
						{
							return exMesh3;
						}
						if (!flag)
						{
							return GetShiftedMesh(num2, _sprite, num);
						}
					}
					else if (!flag)
					{
						SplitMesh(num2, _sprite, num);
						return GetNewMesh(material, num2 + 1);
					}
				}
				else
				{
					exMesh2 = exMesh3;
				}
			}
		}
		if (meshList.Count > 0)
		{
			exMesh exMesh4 = meshList[0];
			if ((object)exMesh4.material == material && exMesh4.vertices.Count <= num)
			{
				return exMesh4;
			}
			exMesh newMesh = GetNewMesh(material, 0);
			if ((object)exMesh4.material == material)
			{
				ShiftSpritesDown(0, num, num);
			}
			return newMesh;
		}
		return GetNewMesh(material, 0);
	}

	private void DoAddSprite(exLayeredSprite _sprite, bool _newSprite)
	{
		if (!(_sprite.material == null))
		{
			((IFriendOfLayer)_sprite).DoSetLayer(this);
			if ((ordered_ & _newSprite) || _sprite.spriteIdInLayer == -1)
			{
				_sprite.spriteIdInLayer = nextSpriteUniqueId;
				nextSpriteUniqueId++;
			}
			else
			{
				nextSpriteUniqueId = Mathf.Max(_sprite.spriteIdInLayer + 1, nextSpriteUniqueId);
			}
			((IFriendOfLayer)_sprite).globalDepth = GetParentGlobalDepth(_sprite.gameObject) + _sprite.depth;
			exMesh meshToAdd = GetMeshToAdd(_sprite);
			AddToMesh(_sprite, meshToAdd);
		}
	}

	private void AddToMesh(exLayeredSprite _sprite, exMesh _mesh)
	{
		_sprite.updateFlags = exUpdateFlags.None;
		_sprite.spriteIndexInMesh = _mesh.spriteList.Count;
		_mesh.spriteList.Add(_sprite);
		_sprite.FillBuffers(_mesh.vertices, _mesh.uvs, _mesh.colors32);
		if (exLayeredSprite.enableFastShowHide)
		{
			AddIndices(_mesh, _sprite);
			_sprite.transparent = !_sprite.visible;
		}
		else if (_sprite.visible)
		{
			AddIndices(_mesh, _sprite);
		}
	}

	private void RemoveFromMesh(exLayeredSprite _sprite, exMesh _mesh)
	{
		_mesh.spriteList.RemoveAt(_sprite.spriteIndexInMesh);
		int vertexCount = _sprite.vertexCount;
		for (int i = _sprite.spriteIndexInMesh; i < _mesh.spriteList.Count; i++)
		{
			exLayeredSprite exLayeredSprite2 = _mesh.spriteList[i];
			exLayeredSprite2.spriteIndexInMesh = i;
			exLayeredSprite2.vertexBufferIndex -= vertexCount;
			if (!exLayeredSprite2.isInIndexBuffer)
			{
				continue;
			}
			int num = exLayeredSprite2.indexBufferIndex + exLayeredSprite2.indexCount;
			for (int j = exLayeredSprite2.indexBufferIndex; j < num; j++)
			{
				if (_mesh.indices.buffer[j] > 0)
				{
					_mesh.indices.buffer[j] -= vertexCount;
				}
			}
		}
		_mesh.updateFlags |= exUpdateFlags.VertexAndIndex;
		_mesh.vertices.RemoveRange(_sprite.vertexBufferIndex, vertexCount);
		_mesh.colors32.RemoveRange(_sprite.vertexBufferIndex, vertexCount);
		_mesh.uvs.RemoveRange(_sprite.vertexBufferIndex, vertexCount);
		if (_sprite.spriteIndexInMesh != _mesh.spriteList.Count)
		{
			_mesh.updateFlags |= exUpdateFlags.UV | exUpdateFlags.Color | exUpdateFlags.Normal;
		}
		if (_sprite.isInIndexBuffer)
		{
			RemoveIndices(_mesh, _sprite);
		}
	}

	private void AddIndices(exMesh _mesh, exLayeredSprite _sprite)
	{
		if (_sprite.isInIndexBuffer)
		{
			return;
		}
		int num;
		if (_mesh.sortedSpriteList.Count > 0)
		{
			num = ((ordered_ || !(_sprite >= _mesh.sortedSpriteList[_mesh.sortedSpriteList.Count - 1])) ? _mesh.sortedSpriteList.BinarySearch(_sprite) : _mesh.sortedSpriteList.Count);
			if (num < 0)
			{
				num = ~num;
			}
			if (num >= _mesh.sortedSpriteList.Count)
			{
				_sprite.indexBufferIndex = _mesh.indices.Count;
			}
			else
			{
				_sprite.indexBufferIndex = _mesh.sortedSpriteList[num].indexBufferIndex;
			}
		}
		else
		{
			num = 0;
			_sprite.indexBufferIndex = 0;
		}
		int indexCount = _sprite.indexCount;
		_mesh.indices.AddRange(indexCount);
		for (int num2 = _mesh.indices.Count - 1 - indexCount; num2 >= _sprite.indexBufferIndex; num2--)
		{
			_mesh.indices.buffer[num2 + indexCount] = _mesh.indices.buffer[num2];
		}
		_sprite.updateFlags |= exUpdateFlags.Index;
		for (int i = num; i < _mesh.sortedSpriteList.Count; i++)
		{
			_mesh.sortedSpriteList[i].indexBufferIndex += indexCount;
		}
		_mesh.sortedSpriteList.Insert(num, _sprite);
	}

	private void RemoveIndices(exMesh _mesh, exLayeredSprite _sprite)
	{
		if (!_sprite.isInIndexBuffer)
		{
			return;
		}
		_mesh.indices.RemoveRange(_sprite.indexBufferIndex, _sprite.indexCount);
		_mesh.updateFlags |= exUpdateFlags.Index;
		int num = _mesh.sortedSpriteList.Count - 1;
		while (num >= 0)
		{
			exLayeredSprite exLayeredSprite2 = _mesh.sortedSpriteList[num];
			if (exLayeredSprite2.indexBufferIndex > _sprite.indexBufferIndex)
			{
				exLayeredSprite2.indexBufferIndex -= _sprite.indexCount;
				num--;
				continue;
			}
			_mesh.sortedSpriteList.RemoveAt(num);
			break;
		}
		_sprite.isInIndexBuffer = false;
	}

	private exLayeredSprite GetNearestSpriteFromBelowMesh(int _curMeshIndex)
	{
		for (int num = _curMeshIndex - 1; num >= 0; num--)
		{
			exMesh exMesh2 = meshList[num];
			if (exMesh2.sortedSpriteList.Count > 0 && exMesh2 != null)
			{
				return exMesh2.sortedSpriteList[exMesh2.sortedSpriteList.Count - 1];
			}
		}
		return null;
	}

	private exLayeredSprite GetNearestSpriteFromAboveMesh(int _curMeshIndex)
	{
		for (int i = _curMeshIndex + 1; i < meshList.Count; i++)
		{
			exMesh exMesh2 = meshList[i];
			if (exMesh2.sortedSpriteList.Count > 0 && exMesh2 != null)
			{
				return exMesh2.sortedSpriteList[0];
			}
		}
		return null;
	}

	private bool IsRenderOrderChangedAmongMeshes(exLayeredSprite _sprite, int _oldMeshIndex)
	{
		exLayeredSprite nearestSpriteFromAboveMesh = GetNearestSpriteFromAboveMesh(_oldMeshIndex);
		if (nearestSpriteFromAboveMesh != null && _sprite > nearestSpriteFromAboveMesh)
		{
			return true;
		}
		exLayeredSprite nearestSpriteFromBelowMesh = GetNearestSpriteFromBelowMesh(_oldMeshIndex);
		if (nearestSpriteFromBelowMesh != null)
		{
			return _sprite < nearestSpriteFromBelowMesh;
		}
		return false;
	}

	private bool IsRenderOrderChangedInMesh(exLayeredSprite _sprite, int _oldMeshIndex, int _oldSortedSpriteIndex)
	{
		exMesh exMesh2 = meshList[_oldMeshIndex];
		if (_oldSortedSpriteIndex < exMesh2.sortedSpriteList.Count - 1)
		{
			exLayeredSprite exLayeredSprite2 = exMesh2.sortedSpriteList[_oldSortedSpriteIndex + 1];
			if (_sprite > exLayeredSprite2)
			{
				return true;
			}
		}
		if (_oldSortedSpriteIndex > 0)
		{
			exLayeredSprite exLayeredSprite3 = exMesh2.sortedSpriteList[_oldSortedSpriteIndex - 1];
			return _sprite < exLayeredSprite3;
		}
		return false;
	}

	private void UpdateAllSpritesDepth()
	{
		for (int i = 0; i < depthDirtyGoList.Count; i++)
		{
			GameObject gameObject = depthDirtyGoList[i];
			if (gameObject != null)
			{
				UpdateSpriteDepthRecursively(gameObject, GetParentGlobalDepth(gameObject));
			}
		}
		depthDirtyGoList.Clear();
	}
}
