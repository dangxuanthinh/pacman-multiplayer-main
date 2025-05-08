using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inky : Ghost
{
    protected override void FindChaseTarget()
    {
        MoveDirection playerMoveDirection = targetPlayer.currentMoveDirection;
        Node playerCurrentNode = targetPlayer.currentNode;
        // Get the 4th node ahead of the player
        Node playerAheadNode = playerCurrentNode;
        switch (playerMoveDirection)
        {
            case MoveDirection.Up:
                playerAheadNode = GameManager.Instance.grid.GetCellValueClamped(playerCurrentNode.x - 4, playerCurrentNode.y + 4);
                break;
            case MoveDirection.Down:
                playerAheadNode = GameManager.Instance.grid.GetCellValueClamped(playerCurrentNode.x - 4, playerCurrentNode.y - 4);
                break;
            case MoveDirection.Left:
                playerAheadNode = GameManager.Instance.grid.GetCellValueClamped(playerCurrentNode.x - 4, playerCurrentNode.y);
                break;
            case MoveDirection.Right:
                playerAheadNode = GameManager.Instance.grid.GetCellValueClamped(playerCurrentNode.x + 4, playerCurrentNode.y);
                break;
            default:
                break;
        }
        Vector2 playerAheadNodeCoordinate = GameManager.Instance.grid.GetGridPosition(playerAheadNode);
        Node blinkyCurrentNode = GameManager.Instance.Blinky.currentNode;
        Vector2 blinkyCurrentCoordinate = GameManager.Instance.grid.GetGridPosition(blinkyCurrentNode);
        // Get the vector from the node ahead of the player to Blinky's current node
        Vector2 playerToBlinky = blinkyCurrentCoordinate - playerAheadNodeCoordinate;
        // Flip that vector by 180 degrees -> Inky's target node
        Vector2 targetNodeCoordinate = playerAheadNodeCoordinate - playerToBlinky;

        //Debug.DrawLine(blinky.currentNode.GetWorldPosition(), playerAheadNode.GetWorldPosition());
        //Debug.DrawLine(playerAheadNode.GetWorldPosition(), targetNode.GetWorldPosition());

        targetNode = GameManager.Instance.grid.GetCellValueClamped((int)targetNodeCoordinate.x, (int)targetNodeCoordinate.y);
    }

}
