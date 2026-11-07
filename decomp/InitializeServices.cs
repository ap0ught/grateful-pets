using System.Collections;
using LaundryBear;
using LaundryBear.PlatformServices.None;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitializeServices : MonoBehaviour
{
	private const float DEBUG_TIMER = 20f;

	private void Start()
	{
		AddRuntimeServices(Singleton<ServiceLocator>.Instance);
		StartCoroutine(InitializationRoutine());
	}

	private IEnumerator InitializationRoutine()
	{
		yield return Singleton<ServiceLocator>.Instance.Initialize();
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(1);
		if (asyncOperation != null)
		{
			asyncOperation.allowSceneActivation = false;
		}
		ServiceLocator.TryGetService<PlatformService>(out var service);
		if (service.SetupLaunchUserAndStorage().Result)
		{
			Debug.Log("Laundry Bear's Platform Layer is ready for use.");
			if (asyncOperation != null)
			{
				asyncOperation.allowSceneActivation = true;
				yield return null;
			}
		}
		else
		{
			Debug.LogError("An error occurred while setting up the user or storage. See logs for details. Halting load.");
		}
	}

	public static void AddRuntimeServices(ServiceLocator instance)
	{
		instance.AddService<LaundryBear.PlatformServices.None.Platform>(out var _);
	}
}
