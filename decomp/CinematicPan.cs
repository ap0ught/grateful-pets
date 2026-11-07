using System;
using System.IO;
using UnityEngine;
using XRL;

public class CinematicPan : MonoBehaviour
{
	public string id;

	public float t;

	public float elapsed;

	public float waitBefore = 2f;

	public float waitAfter = 5f;

	public Vector3 start;

	public Vector3 end;

	public float startZoom;

	public float endZoom;

	public Camera camera;

	public Transform cameraTransform;

	public void Begin(string id)
	{
		this.id = id;
		GameManager.MainCamera.GetComponent<LetterboxCamera>().enabled = false;
		camera = GameManager.MainCamera.GetComponent<Camera>();
		cameraTransform = camera.transform;
		if (File.Exists(DataManager.SavePath("pans.txt")))
		{
			string[] array = File.ReadAllLines(DataManager.SavePath("pans.txt"));
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split(';');
				if (array2[0] == id)
				{
					waitBefore = Convert.ToInt32(array2[1]);
					waitAfter = Convert.ToInt32(array2[2]);
					t = Convert.ToSingle(array2[3]);
					start = new Vector3(Convert.ToSingle(array2[4].Split(',')[0]), Convert.ToSingle(array2[4].Split(',')[1]), -10f);
					end = new Vector3(Convert.ToSingle(array2[5].Split(',')[0]), Convert.ToSingle(array2[5].Split(',')[1]), -10f);
					startZoom = Convert.ToSingle(array2[6]);
					endZoom = Convert.ToSingle(array2[7]);
				}
			}
		}
		if (id == "1")
		{
			waitBefore = 2f;
			waitAfter = 5f;
			t = 10f;
			start = new Vector3(GameManager.Instance.GetCellCenter(11, 22).x, GameManager.Instance.GetCellCenter(11, 22).y, -10f);
			end = new Vector3(GameManager.Instance.GetCellCenter(53, 3).x, GameManager.Instance.GetCellCenter(53, 3).y, -10f);
			startZoom = 180f;
			endZoom = 240f;
		}
	}

	public void End()
	{
		GameManager.MainCamera.GetComponent<LetterboxCamera>().enabled = true;
		UnityEngine.Object.Destroy(this);
	}

	public void Update()
	{
		if (waitBefore > 0f)
		{
			cameraTransform.position = Vector3.Lerp(start, end, 0f);
			camera.orthographicSize = Mathf.Lerp(startZoom, endZoom, 0f);
			waitBefore -= Time.deltaTime;
		}
		else if (elapsed < t)
		{
			elapsed += Time.deltaTime;
			cameraTransform.position = Vector3.Lerp(start, end, elapsed / t);
			camera.orthographicSize = Mathf.Lerp(startZoom, endZoom, elapsed / t);
		}
		else if (waitAfter > 0f)
		{
			waitAfter -= Time.deltaTime;
		}
		else
		{
			End();
		}
	}
}
