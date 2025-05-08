using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pinky : Ghost
{
    protected override void FindChaseTarget()
    {
        MoveDirection playerMoveDirection = targetPlayer.currentMoveDirection;
        Node playerCurrentNode = targetPlayer.currentNode;
        switch (playerMoveDirection)
        {
            case MoveDirection.Up:
                targetNode = GameManager.Instance.grid.GetCellValueClamped(playerCurrentNode.x - 4, playerCurrentNode.y + 4);
                break;
            case MoveDirection.Down:
                targetNode = GameManager.Instance.grid.GetCellValueClamped(playerCurrentNode.x - 4, playerCurrentNode.y - 4);
                break;
            case MoveDirection.Left:
                targetNode = GameManager.Instance.grid.GetCellValueClamped(playerCurrentNode.x - 4, playerCurrentNode.y);
                break;
            case MoveDirection.Right:
                targetNode = GameManager.Instance.grid.GetCellValueClamped(playerCurrentNode.x + 4, playerCurrentNode.y);
                break;
            default:
                break;
        }
    }
}
