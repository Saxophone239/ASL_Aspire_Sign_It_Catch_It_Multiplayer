using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationController : MonoBehaviour
{
    [SerializeField] private ClientSingleton clientPrefab;
    [SerializeField] private HostSingleton hostPrefab;
    [SerializeField] private ServerSingleton serverPrefab;

    private ApplicationData appData;

    private async void Start()
    {
        // This object will persist across scenes
        DontDestroyOnLoad(gameObject);

        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
    }

    /// <summary>
    /// Begin logic for launching in a dedicated mode.
    /// </summary>
    /// <param name="isDedicatedServer">Whether the computer running this game is server-only or not</param>
    /// <returns>A Task to be awaited</returns>
    private async Task LaunchInMode(bool isDedicatedServer)
    {
        if (isDedicatedServer)
        {
            // set frame rate so server CPU doesn't crash
            Application.targetFrameRate = 60;

            // Instantiate class to get command line data on server startup
            appData = new ApplicationData();

            // Create and wait for our server singleton & variables to be created
            ServerSingleton serverSingleton = Instantiate(serverPrefab);
            StartCoroutine(LoadGameSceneAsync(serverSingleton));
        }
        else
        {
            // Create and wait for our host singleton & variables to be created
            HostSingleton hostSingleton = Instantiate(hostPrefab);
            hostSingleton.CreateHost();

            // Create and wait for our client singleton & variables to be created
            ClientSingleton clientSingleton = Instantiate(clientPrefab);
            bool isAuthenticated = await clientSingleton.CreateClient();

            // Go to main menu
            if (isAuthenticated)
            {
                clientSingleton.GameManager.GoToMenu();
            }
        }
    }

    /// <summary>
    /// As a server, load the next scene, which is the main game scene.
    /// </summary>
    /// <param name="serverSingleton">The server singleton we're using</param>
    /// <returns>An IEnumerator used for a Coroutine</returns>
    private IEnumerator LoadGameSceneAsync(ServerSingleton serverSingleton)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(BasketGameScenes.GameSceneName);

        while (!asyncOperation.isDone)
        {
            yield return null;
        }

        // Starter server
        Task createServerTask = serverSingleton.CreateServer();
        yield return new WaitUntil(() => createServerTask.IsCompleted);

        // Startup game server
        Task startServerTask = serverSingleton.GameManager.StartGameServerAsync();
        yield return new WaitUntil(() => startServerTask.IsCompleted);
    }
}
