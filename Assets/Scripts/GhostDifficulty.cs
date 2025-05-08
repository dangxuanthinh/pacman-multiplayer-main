using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Difficulty", menuName = "Pacman Level Difficulty"), System.Serializable]
public class GhostDifficulty : ScriptableObject
{
    public string difficultyName;
    public float movementSpeedMultiplier = 1f;
    public List<GhostStateDuration> ghostStateDurations = new List<GhostStateDuration>();
}

[System.Serializable]
public class GhostStateDuration
{
    public GhostState ghostState;
    public float duration;
}
