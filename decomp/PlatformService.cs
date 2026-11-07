using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using LaundryBear;
using LaundryBear.PlatformServices;
using Platform.IO;
using UnityEngine;

public class PlatformService : MonoBehaviour, IService
{
	public class InitializationBundle
	{
		public readonly LeaderboardResult Result;

		public readonly string LeaderboardId;

		public InitializationBundle(LeaderboardResult result, string leaderboardId)
		{
			Result = result;
			LeaderboardId = leaderboardId;
		}
	}

	private IUser? m_user;

	private IStorage m_storage;

	private const string MOUNTNAME_CONSOLE = "save";

	private const string GAME_NETWORK_ID = "t-b08cccc4";

	private SynchronizationContext m_mainContext;

	public string Name => "Platform Service";

	public ServiceLocator.ServiceInitializationStatus InitializationStatus { get; private set; }

	public IStorage GetStorage()
	{
		return m_storage;
	}

	public IEnumerator Initialize(bool sync = false)
	{
		ServiceLocator.ManagersInitializedEvent += OnServicesInitialized;
		m_mainContext = SynchronizationContext.Current;
		InitializationStatus = ServiceLocator.ServiceInitializationStatus.Ready;
		yield break;
	}

	private void OnServicesInitialized()
	{
		ServiceLocator.ManagersInitializedEvent -= OnServicesInitialized;
	}

	public async Task<bool> SetupLaunchUserAndStorage()
	{
		if (ServiceLocator.TryGetService<IUserService>(out var service))
		{
			m_user = await GetLaunchUserAsync(service);
			if (m_user == null)
			{
				Debug.LogError("Couldn't get user from IUserService, halting setup.");
				return false;
			}
		}
		if (!ServiceLocator.TryGetService<IStorageService>(out var service2))
		{
			Debug.LogError("Couldn't get IStorageService, halting setup.)");
			return false;
		}
		string mountPoint = string.Empty;
		if (m_user != null)
		{
			mountPoint = "save";
		}
		var (storageResult, storage) = await GetStorageAsync(service2, m_user, mountPoint);
		switch (storageResult)
		{
		case StorageResult.QuotaExceeded:
			Debug.LogError($"{storageResult} handling not implemented. On certain consoles (i think ps5) you are required to let the user retry after having time to try a different user or storage device... this should probably be handled by the platform service implementation of OpenOrCreate. but it is handled directly in PlatformService for now");
			return await SetupLaunchUserAndStorage();
		default:
			Debug.LogError($"{storageResult} handling not implemented. On certain consoles (i think ps5) you are required to let the user retry after having time to try a different user or storage device... this should probably be handled by the platform service implementation of OpenOrCreate");
			return false;
		case StorageResult.Success:
			m_storage = storage;
			State.SetStorage(storage);
			return true;
		}
	}

	private static Task<IUser?> GetLaunchUserAsync(IUserService service)
	{
		TaskCompletionSource<IUser?> tcs = new TaskCompletionSource<IUser>();
		service.GetLaunchUser((SignInResult result, IUser user) =>
		{
			tcs.SetResult((result == SignInResult.Success) ? user : null);
		});
		return tcs.Task;
	}

	private static Task<(StorageResult result, IStorage? storage)> GetStorageAsync(IStorageService service, IUser? user, string mountPoint)
	{
		TaskCompletionSource<(StorageResult result, IStorage? storage)> tcs = new TaskCompletionSource<(StorageResult, IStorage)>();
		if (user != null)
		{
			service.OpenOrCreate(user, mountPoint, callback);
		}
		else
		{
			service.OpenOrCreate(mountPoint, callback);
		}
		return tcs.Task;
		void callback(StorageResult result, IStorage storage)
		{
			if (result == StorageResult.Success)
			{
				tcs.SetResult((result, storage));
			}
			else
			{
				tcs.SetResult((result, null));
			}
		}
	}

	public void BeginShutdownNoticeSection()
	{
	}

	public void EndShutdownNoticeSection()
	{
	}
}
