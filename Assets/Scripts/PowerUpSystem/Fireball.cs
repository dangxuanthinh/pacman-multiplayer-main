using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ghost"))
        {
            GameManager.Instance.EatGhost(other.GetComponent<Ghost>());
        }
    }
}
