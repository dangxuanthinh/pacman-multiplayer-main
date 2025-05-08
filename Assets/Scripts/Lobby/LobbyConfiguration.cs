using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyConfiguration
{
    public GameMode gameMode;
    public PacmanMap map;
    public GhostDifficulty ghostDifficulty;
    public bool isPrivate;

    public override string ToString()
    {
        return $"Lobby Configuration: {gameMode}, {map.mapName}, {ghostDifficulty}, {isPrivate}";
    }
}
