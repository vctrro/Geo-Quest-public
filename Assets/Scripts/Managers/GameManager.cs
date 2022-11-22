using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;

//TODO: 

public class GameManager : Singleton<GameManager>
{
    // (Optional) Prevent non-singleton constructor use.
    // protected GameManager() {}
    public GameConfig GameConfig { get => gameConfig; }
    public AudioManager AudioManager { get => audioManager; }
    private GameConfig gameConfig;
    private AudioManager audioManager;
    private Animator fade;


    private void Awake()
    {
        InitializeGame();        
    }

    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Addressables.InstantiateAsync("FadeScreen", gameObject.transform).Completed +=
            (UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> fadeScreen) => {
            fade = fadeScreen.Result.GetComponent<Animator>();
            };
    }

    private void OnApplicationFocus(bool focusStatus)
    {
        //think
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        //save data
        if (pauseStatus)
        {
            PlayerPrefs.SetString("GameConfig", JsonUtility.ToJson(gameConfig));
            PlayerPrefs.Save();
        }
    }

    private void OnApplicationQuit()
    {
        //save data
        PlayerPrefs.SetString("GameConfig", JsonUtility.ToJson(gameConfig));
        PlayerPrefs.Save();
    }

    public void LoadScene(string scene)
    {
        if (SceneUtility.GetBuildIndexByScenePath(scene) >= 0)
        {
            StartCoroutine(FadeLoadScene(scene));
        }
        else
        {
            StartCoroutine(FadeLoadScene($"MainMenu"));
        }
    }

    public IEnumerator SetActiveWithDelay(GameObject object1, bool active, float time)
    {
        yield return new WaitForSecondsRealtime(time);
        object1.SetActive(active);
    }

    private void InitializeGame()
    {
        /* Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
        var dependencyStatus = task.Result;
        if (dependencyStatus == Firebase.DependencyStatus.Available) {
            // Create and hold a reference to your FirebaseApp,
            // where app is a Firebase.FirebaseApp property of your application class.
            //   app = Firebase.FirebaseApp.DefaultInstance;

            // Set a flag here to indicate whether Firebase is ready to use by your app.
        } else {
            UnityEngine.Debug.LogError(System.String.Format(
            "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            // Firebase Unity SDK is not safe to use here.
        }
        }); */
        
        if (PlayerPrefs.HasKey("GameConfig"))
        {
            gameConfig = JsonUtility.FromJson<GameConfig>(PlayerPrefs.GetString("GameConfig"));
        }
        else
        {
            gameConfig = new GameConfig();
            PlayerPrefs.SetString("GameConfig", JsonUtility.ToJson(gameConfig));
        }

        audioManager = gameObject.AddComponent<AudioManager>();

        Debug.Log($"Initialize Game");
        Debug.Log($"Section        {GameConfig.CurrentSection}");
        Debug.Log($"Quest          {GameConfig.CurrentQuest}");

    }

    private IEnumerator AsyncLoadScene(string scene)
    {
        AsyncOperation load = SceneManager.LoadSceneAsync(scene);
        while (!load.isDone)
        {
            yield return null;
        }
    }

    private IEnumerator FadeLoadScene(string scene)
    {
        fade.Play("FadeIN");
        yield return new WaitForSecondsRealtime(0.4f);
        yield return AsyncLoadScene(scene);
        fade.Play("FadeOUT");
        Time.timeScale = 1;
    }

    private void OnDestroy()
    {
        // Debug.LogWarning($"{gameObject} has been destroyed");
    }
}
