using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData
{
    //public int Highscore;
    //public int Coin;
    //public Dictionary<string, bool> itemObtainedTable;

    public Dictionary<string, string> achievementProgressionDictionary;

    // Constructor will set fields to default values
    public GameData()
    {
        achievementProgressionDictionary = new Dictionary<string, string>();
    }
}
