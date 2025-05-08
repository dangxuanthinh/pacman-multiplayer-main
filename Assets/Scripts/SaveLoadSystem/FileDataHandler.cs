using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileDataHandler
{
    private string dataDirPath;
    private string dataFileName;

    public FileDataHandler(string dataDirPath, string dataFileName)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
    }

    public GameData Load()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        GameData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream fileStream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }
                loadedData = JsonConvert.DeserializeObject<GameData>(dataToLoad);

            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when reading data from file: " + fullPath + "\n" + e);
            }
        }
        return loadedData;
    }

    public string Save(GameData data)
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            string dataToStore = JsonConvert.SerializeObject(data, Formatting.Indented);
            using (FileStream fileStream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    writer.Write(dataToStore);
                }
            }
            return dataToStore;
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when saving data to file: " + fullPath + "\n" + e);
            return null;
        }
    }
}
