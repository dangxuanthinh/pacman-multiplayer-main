using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataPersistenceManager : MonoBehaviour
{

    [Header("File storage configuration")]
    [SerializeField] private string saveFileName;
    public static DataPersistenceManager Instance { get; private set; }

    private FileDataHandler fileDataHandler;

    public GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;

    public bool loadDataFromServerOnSignedIn;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        fileDataHandler = new FileDataHandler(Application.persistentDataPath, saveFileName);
    }

    private void Start()
    {
        UserManager.Instance.OnSignedIn += LoadGameDataFromServer;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UserManager.Instance.OnSignedIn -= LoadGameDataFromServer;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    public void NewGame()
    {
        gameData = new GameData();
    }

    public void LoadGame()
    {
        gameData = fileDataHandler.Load();
        if (gameData == null)
        {
            Debug.Log("No save data found, initializing data to defaults");
            NewGame();
        }
        Debug.Log("Loading local data");
        // Push saved data to other scripts
        foreach (var dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(gameData);
        }
    }

    public string SaveGame()
    {
        if (dataPersistenceObjects != null && dataPersistenceObjects.Count > 0)
        {
            // Pass game data to other scripts so that they can update it
            foreach (var dataPersistenceObj in dataPersistenceObjects)
            {
                dataPersistenceObj.SaveData(gameData);
            }
            // Then save that data
            Debug.Log("Saving local game data");
            return fileDataHandler.Save(gameData);
        }
        return null;
    }

    public void LoadGameDataFromServer(string jsonData)
    {
        if (loadDataFromServerOnSignedIn == false) return;
        if (string.IsNullOrEmpty(jsonData))
        {
            gameData = new GameData();
        }
        else
        {
            gameData = JsonConvert.DeserializeObject<GameData>(jsonData);
        }
        foreach (var dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(gameData);
        }
    }


    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistences = FindObjectsOfType<MonoBehaviour>(true).OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistences);
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private void OnApplicationPause(bool pause)
    {
        //SaveGame();
    }
}
