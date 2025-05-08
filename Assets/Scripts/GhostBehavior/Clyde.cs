using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clyde : Ghost
{
    protected override void FindChaseTarget()
    {
        Node playerCurrentNode = targetPlayer.currentNode;
        int distanceToPlayer = GameManager.Instance.grid.GetGridDistance(currentNode, playerCurrentNode);
        if (distanceToPlayer >= 10)
        {
            targetNode = targetPlayer.currentNode;
        }
        else
        {
            targetNode = scatterTargetNode;
        }
    }
}
