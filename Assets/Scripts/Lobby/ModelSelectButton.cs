using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModelSelectButton : MonoBehaviour
{
    [SerializeField] private int modelIndex;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            PacmanMultiplayer.Instance.SetPlayerSelectedModel(modelIndex);
        });
    }
}
