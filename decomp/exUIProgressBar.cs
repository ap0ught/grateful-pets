using System.Collections.Generic;
using UnityEngine;

public class exUIProgressBar : exUIControl
{
	public enum Direction
	{
		Vertical,
		Horizontal
	}

	public new static string[] eventNames = new string[1] { "onProgressChanged" };

	private List<exUIEventListener> onProgressChanged;

	[SerializeField]
	protected float progress_;

	[SerializeField]
	protected float barSize_;

	public Direction direction = Direction.Horizontal;

	protected exSprite bar;

	public float progress
	{
		get
		{
			return progress_;
		}
		set
		{
			if (progress_ != value)
			{
				progress_ = Mathf.Clamp(value, 0f, 1f);
				UpdateBar();
				exUIRatioEvent exUIRatioEvent2 = new exUIRatioEvent();
				exUIRatioEvent2.bubbles = false;
				exUIRatioEvent2.ratio = progress_;
				OnProgressChanged(exUIRatioEvent2);
			}
		}
	}

	public float barSize
	{
		get
		{
			return barSize_;
		}
		set
		{
			if (barSize_ != value)
			{
				barSize_ = value;
				UpdateBar();
			}
		}
	}

	public void OnProgressChanged(exUIEvent _event)
	{
		exUIMng.inst.DispatchEvent(this, "onProgressChanged", onProgressChanged, _event);
	}

	public override void CacheEventListeners()
	{
		base.CacheEventListeners();
		onProgressChanged = eventListenerTable["onProgressChanged"];
	}

	public override string[] GetEventNames()
	{
		string[] array = base.GetEventNames();
		string[] array2 = new string[array.Length + eventNames.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = array[i];
		}
		for (int j = 0; j < eventNames.Length; j++)
		{
			array2[j + array.Length] = eventNames[j];
		}
		return array2;
	}

	public static void SetBarSize(exSprite _bar, float _barSize, float _progress, Direction _direction)
	{
		if (!(_bar != null))
		{
			return;
		}
		if (_direction == Direction.Horizontal)
		{
			if (_bar.spriteType == exSpriteType.Sliced)
			{
				float num = _progress * (_barSize - _bar.leftBorderSize - _bar.rightBorderSize);
				_bar.width = num + _bar.leftBorderSize + _bar.rightBorderSize;
			}
			else
			{
				_bar.width = _progress * _barSize;
			}
		}
		else if (_bar.spriteType == exSpriteType.Sliced)
		{
			float num2 = _progress * (_barSize - _bar.topBorderSize - _bar.bottomBorderSize);
			_bar.height = num2 + _bar.topBorderSize + _bar.bottomBorderSize;
		}
		else
		{
			_bar.height = _progress * _barSize;
		}
	}

	protected new void Awake()
	{
		base.Awake();
		Transform transform = base.transform.Find("__bar");
		if ((bool)transform)
		{
			bar = transform.GetComponent<exSprite>();
		}
		UpdateBar();
	}

	private void UpdateBar()
	{
		SetBarSize(bar, barSize_, progress_, direction);
	}
}
