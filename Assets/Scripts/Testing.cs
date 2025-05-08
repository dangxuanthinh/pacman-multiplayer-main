using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    [Button]
    public void Reposition()
    {
        foreach (Transform line in transform)
        {
            float x = line.localPosition.x;
            x = Mathf.RoundToInt(x);
            line.localPosition = new Vector3(x, 0, 0);
        }
    }
}
