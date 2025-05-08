using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeInfo : MonoBehaviour
{
    [HideInInspector] public Node node;
    public bool passable;

    [ShowIf("@passable == true && isGhostHouse == false")]
    public Transform teleportDesination;

    [HideIf("@passable == false")]
    public bool hasPowerUpPellet;

    [HideIf("@passable == false")]
    public bool isGhostHouse;

    [HideIf("@isGhostHouse == false || passable == false")]
    public bool isGhostHouseEntrance;

    [HideIf("@isGhostHouse == false || passable == false")]
    public GhostName ghostHouseOwner;

    [HideIf("@isGhostHouse == true")]
    public GhostName scatterDestinationOwner;

    [Header("Debug")]
    [SerializeField] private Material red;
    [SerializeField] private Material pink;
    [SerializeField] private Material cyan;
    [SerializeField] private Material orange;
    [SerializeField] private Material grey;
    [SerializeField] private Material yellow;
    [SerializeField] private Material white;
    [SerializeField] private Material brown;

    private void OnValidate()
    {
        if (!TryGetComponent<MeshRenderer>(out var meshRenderer)) return;
        Material material = white;
        if (teleportDesination != null)
            material = grey;
        else if (ghostHouseOwner == GhostName.Blinky || scatterDestinationOwner == GhostName.Blinky)
            material = red;
        else if (ghostHouseOwner == GhostName.Inky || scatterDestinationOwner == GhostName.Inky)
            material = cyan;
        else if (ghostHouseOwner == GhostName.Pinky || scatterDestinationOwner == GhostName.Pinky)
            material = pink;
        else if (ghostHouseOwner == GhostName.Clyde || scatterDestinationOwner == GhostName.Clyde)
            material = orange;
        else if (hasPowerUpPellet)
            material = yellow;
        else if (passable == false)
            material = brown;
        meshRenderer.material = material;
    }
}
