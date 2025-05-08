using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameResourceHandler : MonoBehaviour
{
    public static GameResourceHandler Instance;

    public List<GhostDifficulty> difficulties = new List<GhostDifficulty>();
    public List<PacmanMap> maps = new List<PacmanMap>();

    private Dictionary<string, bool> itemObtainedTable = new Dictionary<string, bool>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        LoadDifficulties();
        LoadMaps();
    }

    private void LoadDifficulties()
    {
        difficulties = Resources.LoadAll<GhostDifficulty>("Difficulties").ToList();
    }

    private void LoadMaps()
    {
        maps = Resources.LoadAll<PacmanMap>("Maps").ToList();
    }

    public PacmanMap GetMap(string mapName)
    {
        return maps.Find(m => m.mapName == mapName);
    }

    public GhostDifficulty GetGhostDifficulty(string difficultyName)
    {
        return difficulties.Find(d => d.difficultyName == difficultyName);
    }
}
