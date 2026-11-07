using System;
using UnityEngine;
using XRL.UI;

public class LetterboxCamera : MonoBehaviour
{
	public enum TileAreaAlignmentType
	{
		Left,
		Right,
		Center
	}

	private bool _Refresh;

	public bool ForcingFullscreen;

	public int UpdateDelay;

	public float _DesiredZoomFactor = 1f;

	public bool Pannable;

	public Vector3 _DesiredPosition;

	public bool ShowDebugTargetArea = true;

	public RectTransform DebugTargetArea;

	public Rect TargetArea;

	public float InitialDesiredWidth = 1280f;

	public float DesiredWidth = 1280f;

	public float LastOrthographicSize;

	private int LastWidth;

	private int LastHeight;

	private float LastZoomFactor;

	private bool lastPrerelaseStage;

	private double lastStageScale;

	private Vector3[] _worldCorners = new Vector3[4];

	public RectTransform lastArea;

	public Rect AdjustedTargetArea;

	public TileAreaAlignmentType TileAreaAlignment;

	public bool suspendUpdate;

	public float widePadding = 216f;

	public float wideLetterboxMagic = 600f;

	public float tileWidth = 16f;

	public float tileHeight = 24f;

	public float tilesWide = 80f;

	public float tilesHigh = 25f;

	private Camera _camera;

	public float baseOrthographicStageScale;

	public float baseOrthographicSize;

	public float UnitsPerPixel;

	public float DesiredZoomFactor
	{
		get
		{
			return _DesiredZoomFactor;
		}
		set
		{
			if (_DesiredZoomFactor != value)
			{
				_DesiredZoomFactor = value;
			}
		}
	}

	public Vector3 DesiredPan
	{
		get
		{
			return ClampPanPosition(_DesiredPosition);
		}
		set
		{
			if (_DesiredPosition != value)
			{
				_DesiredPosition = value;
			}
		}
	}

	public Vector3 CurrentPosition => gameObject.transform.localPosition;

	public Vector3 ActualPan => CurrentPosition - targetAreaCameraCentered;

	public Camera myCamera
	{
		get
		{
			if (_camera == null)
			{
				_camera = GetComponent<Camera>();
			}
			return _camera;
		}
	}

	private Vector3 targetAreaCameraCentered => new Vector3(0f - (AdjustedTargetArea.width / 2f + AdjustedTargetArea.xMin), 0f - (AdjustedTargetArea.height / 2f + AdjustedTargetArea.yMin), -10f);

	public bool isPixelPerfect
	{
		get
		{
			if (GameManager.Instance.TargetZoomFactor > 0f)
			{
				return GameManager.Instance.TileScale >= 1;
			}
			return false;
		}
	}

	public bool IsOverflowing
	{
		get
		{
			if (!(AdjustedTargetArea.width < tilesWide * tileWidth))
			{
				return AdjustedTargetArea.height < tilesHigh * tileHeight;
			}
			return true;
		}
	}

	public void Refresh()
	{
		_Refresh = true;
	}

	public void Awake()
	{
		DesiredPan = new Vector3(0f, 0f, 0f);
		suspendUpdate = false;
	}

	public void SetPositionImmediately(Vector3 position)
	{
		if (UpdateDelay <= 0)
		{
			gameObject.transform.localPosition = position;
		}
		DesiredPan = position;
	}

	private Rect GetWorldCoordinates(RectTransform uiElement)
	{
		uiElement.GetWorldCorners(_worldCorners);
		Vector3 vector = myCamera.ScreenToWorldPoint(_worldCorners[0]);
		Vector3 vector2 = myCamera.ScreenToWorldPoint(_worldCorners[2]);
		return new Rect(vector.x, vector.y, vector2.x - vector.x, vector2.y - vector.y);
	}

	public void SetTargetArea(RectTransform area)
	{
		if (lastArea == null || area != lastArea || DebugTargetArea.sizeDelta.x != area.rect.width || DebugTargetArea.sizeDelta.y != area.rect.height || DebugTargetArea.anchoredPosition.x != area.anchoredPosition.x || DebugTargetArea.anchoredPosition.y != area.anchoredPosition.y)
		{
			_Refresh = true;
			lastArea = area;
			DebugTargetArea.sizeDelta = new Vector2(area.rect.width, area.rect.height);
			DebugTargetArea.anchoredPosition = new Vector3(area.anchoredPosition.x, area.anchoredPosition.y);
		}
		TargetArea = GetWorldCoordinates(area);
		if (ShowDebugTargetArea)
		{
			Rect worldCoordinates = GetWorldCoordinates(DebugTargetArea);
			_ = TargetArea != worldCoordinates;
		}
		AdjustedTargetArea = new Rect(TargetArea.x - transform.position.x, TargetArea.y - transform.position.y, TargetArea.width, TargetArea.height);
	}

	private Vector3 dockHuggingVector()
	{
		Vector3 letterboxTotalArea = GetLetterboxTotalArea();
		if (!ForcingFullscreen && letterboxTotalArea.x > 0f)
		{
			if (TileAreaAlignment == TileAreaAlignmentType.Left)
			{
				return new Vector3(letterboxTotalArea.x / 2f, 0f, 0f);
			}
			if (TileAreaAlignment == TileAreaAlignmentType.Right)
			{
				return new Vector3((0f - letterboxTotalArea.x) / 2f, 0f, 0f);
			}
		}
		return Vector3.zero;
	}

	private Vector3 ClampPanPosition(Vector3 p)
	{
		Bounds bounds = new Bounds(size: new Vector3(Mathf.Ceil(Mathf.Max(0f, tilesWide * tileWidth - AdjustedTargetArea.width) / 2f) * 2f, Mathf.Ceil(Mathf.Max(0f, tilesHigh * tileHeight - AdjustedTargetArea.height) / 2f) * 2f, 0f), center: Vector3.zero);
		if (bounds.Contains(p))
		{
			return p;
		}
		return bounds.ClosestPoint(p);
	}

	public Vector3 GetLetterboxTotalArea()
	{
		return new Vector3(AdjustedTargetArea.width - tilesWide * tileWidth, AdjustedTargetArea.height - tilesHigh * tileHeight, 0f);
	}

	public void OnUpdate()
	{
		if (GameManager.Instance?._ActiveGameView == "Cinematic")
		{
			return;
		}
		AdjustedTargetArea = new Rect(TargetArea.x - transform.position.x, TargetArea.y - transform.position.y, TargetArea.width, TargetArea.height);
		if (DebugTargetArea.gameObject.activeInHierarchy != ShowDebugTargetArea)
		{
			DebugTargetArea?.gameObject.SetActive(ShowDebugTargetArea);
		}
		if (UpdateDelay > 0)
		{
			UpdateDelay--;
			return;
		}
		Vector3 vector = targetAreaCameraCentered + dockHuggingVector();
		if (Pannable)
		{
			vector += ClampPanPosition(DesiredPan);
		}
		if (Options.PlayScale == Options.PlayAreaScaleTypes.Fit || GameManager.Instance.TargetZoomFactor == -1f || ForcingFullscreen)
		{
			baseOrthographicStageScale = Mathf.Min(AdjustedTargetArea.width / (tilesWide * tileWidth), AdjustedTargetArea.height / (tilesHigh * tileHeight));
		}
		else
		{
			baseOrthographicStageScale = Mathf.Max(AdjustedTargetArea.width / (tilesWide * tileWidth), AdjustedTargetArea.height / (tilesHigh * tileHeight));
		}
		if (isPixelPerfect)
		{
			baseOrthographicSize = Screen.height / (2 * GameManager.Instance.TileScale);
		}
		else
		{
			baseOrthographicSize = myCamera.orthographicSize / baseOrthographicStageScale;
		}
		if (float.IsNaN(baseOrthographicSize) || baseOrthographicSize <= 0f)
		{
			return;
		}
		if (baseOrthographicSize < 32f)
		{
			baseOrthographicSize = 32f;
		}
		if (baseOrthographicSize > 1000f)
		{
			baseOrthographicSize = 1000f;
		}
		float num = baseOrthographicSize / DesiredZoomFactor;
		if (ForcingFullscreen)
		{
			num = Math.Max(myCamera.orthographicSize / baseOrthographicStageScale, num);
		}
		if (!Mathf.Approximately(myCamera.orthographicSize, num))
		{
			myCamera.orthographicSize = num;
			UnitsPerPixel = num * 2f / (float)Screen.height;
			if (lastArea != null)
			{
				SetTargetArea(lastArea);
			}
		}
		if (suspendUpdate)
		{
			DesiredZoomFactor = Math.Max(1f, GameManager.Instance.TargetZoomFactor);
		}
		else if (_Refresh || Screen.width != LastWidth || Screen.height != LastHeight || DesiredZoomFactor != LastZoomFactor || GameManager.Instance.ModernUI != lastPrerelaseStage || GameManager.Instance.StageScale != lastStageScale || (transform.localPosition - vector).magnitude > 1.1f)
		{
			_Refresh = false;
			LastWidth = Screen.width;
			LastHeight = Screen.height;
			LastZoomFactor = DesiredZoomFactor;
			lastPrerelaseStage = GameManager.Instance.ModernUI;
			lastStageScale = GameManager.Instance.StageScale;
			GetComponent<CC_AnalogTV>().scanlinesCount = 1853f;
			if (Pannable)
			{
				transform.localPosition = targetAreaCameraCentered + dockHuggingVector() + ClampPanPosition(DesiredPan);
			}
			else
			{
				transform.localPosition = targetAreaCameraCentered + dockHuggingVector();
			}
		}
	}

	public void DockLeft(float fudgeFactor = 1f)
	{
		DesiredPan = new Vector3(GetEdgeDistance(fudgeFactor), 0f, -10f);
	}

	public void DockRight(float fudgeFactor = 1f)
	{
		DesiredPan = new Vector3(0f - GetEdgeDistance(fudgeFactor), 0f, -10f);
	}

	public float GetEdgeDistance(float fudgeFactor)
	{
		float num = (float)LastHeight / 2f / LastOrthographicSize;
		float num2 = LastOrthographicSize / ((float)LastHeight / 2f);
		return ((float)LastWidth - DesiredWidth * fudgeFactor * num) / 2f * num2;
	}

	public float GetStageEdge()
	{
		float num = (float)LastHeight / 2f / LastOrthographicSize;
		return ((float)LastWidth - DesiredWidth * num) / (float)GameManager.Instance.StageScale;
	}

	public float GetScale()
	{
		return (float)LastHeight / 2f / LastOrthographicSize;
	}
}
