using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MoneyManager : MonoBehaviour, IDataPersistence
{
    public static MoneyManager Instance;

    public int currentCoin;


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    public bool Consume(int coinAmount)
    {
        if (currentCoin >= coinAmount)
        {
            currentCoin -= coinAmount;
            return true;
        }
        else
        {
            Debug.Log($"Not enough coin!");
            return false;
        }
    }

    public void LoadData(GameData data)
    {
        //currentCoin = data.Coin;
    }

    public void SaveData(GameData data)
    {
        //data.Coin = currentCoin;
    }
}
