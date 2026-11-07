using System.Collections;
using System.Collections.Generic;
using RedShadow.Tween;
using UnityEngine;

namespace RedShadow.CommonDialogs;

public class DialogBase : MonoBehaviour
{
	public bool DestroyOnClose = true;

	protected static List<DialogBase> VisibleDialogs = new List<DialogBase>();

	protected GameObject _blockingPanel;

	protected GameObject _mainWindow;

	public bool IsVisible { get; private set; }

	protected virtual void Awake()
	{
		if (transform.Find("BlockingPanel") != null)
		{
			_blockingPanel = transform.Find("BlockingPanel").gameObject;
		}
		if (transform.Find("Window") != null)
		{
			_mainWindow = transform.Find("Window").gameObject;
		}
	}

	public virtual void Start()
	{
		if (_blockingPanel != null)
		{
			_blockingPanel.SetActive(value: false);
		}
		if (_mainWindow != null)
		{
			_mainWindow.SetActive(value: false);
			_mainWindow.transform.eulerAngles = new Vector3(0f, 90f, 0f);
		}
		else if (!DestroyOnClose)
		{
			gameObject.SetActive(value: false);
		}
	}

	public virtual void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) && isTop())
		{
			cancel();
		}
	}

	public virtual void cancel()
	{
		hide();
	}

	protected virtual void show()
	{
		IsVisible = true;
		VisibleDialogs.Add(this);
		if (_blockingPanel != null)
		{
			_blockingPanel.SetActive(value: true);
		}
		if (_mainWindow != null)
		{
			_mainWindow.SetActive(value: true);
			_mainWindow.transform.rotateTo(new Vector3(0f, 0f, 0f)).duration(0.2f);
		}
		else if (!DestroyOnClose)
		{
			gameObject.SetActive(value: true);
		}
	}

	protected virtual IEnumerator show_co(float delay)
	{
		yield return new WaitForSeconds(delay);
		show();
	}

	protected virtual void hide()
	{
		IsVisible = false;
		VisibleDialogs.Remove(this);
		if (_blockingPanel != null)
		{
			_blockingPanel.SetActive(value: false);
		}
		if (_mainWindow != null)
		{
			TweenManager.delayedCall(0.2f, () =>
			{
				_mainWindow.SetActive(value: false);
			});
			_mainWindow.transform.rotateTo(new Vector3(0f, 90f, 0f)).duration(0.2f);
		}
		else if (!DestroyOnClose)
		{
			gameObject.SetActive(value: false);
		}
		if (DestroyOnClose)
		{
			Object.Destroy(gameObject, 0.5f);
		}
	}

	public bool isTop()
	{
		if (VisibleDialogs.Count > 0)
		{
			return VisibleDialogs[VisibleDialogs.Count - 1] == this;
		}
		return false;
	}

	public static DialogBase getTopmost()
	{
		if (VisibleDialogs.Count > 0)
		{
			return VisibleDialogs[VisibleDialogs.Count - 1];
		}
		return null;
	}

	public void ensureVisible()
	{
		RectTransform rectTransform = ((!(_mainWindow != null)) ? GetComponent<RectTransform>() : _mainWindow.GetComponent<RectTransform>());
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		RectTransform component = GetComponent<RectTransform>();
		Vector3[] array2 = new Vector3[4];
		component.GetWorldCorners(array2);
		float num = rectTransform.position.x;
		float num2 = rectTransform.position.y;
		if (array[0].x < array2[0].x)
		{
			num += array2[0].x - array[0].x;
		}
		if (array[0].y < array2[0].y)
		{
			num2 += array2[0].y - array[0].y;
		}
		if (array[2].x > array2[2].x)
		{
			num -= array[2].x - array2[2].x;
		}
		if (array[2].y > array2[2].y)
		{
			num2 -= array[2].y - array2[2].y;
		}
		rectTransform.position = new Vector2(num, num2);
	}
}
