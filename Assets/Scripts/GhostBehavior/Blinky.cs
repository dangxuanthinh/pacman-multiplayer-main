using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blinky : Ghost
{
    protected override void FindChaseTarget()
    {
        targetNode = targetPlayer.currentNode;
    }
}
