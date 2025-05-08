using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapHandler : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.OnGameStart += SetupCamera;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameStart -= SetupCamera;
    }

    private void SetupCamera()
    {
        var grid = GameManager.Instance.grid;
        Vector3 mapMiddlePoint = grid.GetCellValue(0, 0).GetWorldPosition() + grid.GetCellValue(grid.Width - 1, grid.Height - 1).GetWorldPosition();
        Vector3 mapCenter = mapMiddlePoint / 2f;
        mapCenter.z = transform.position.z;
        transform.position = mapCenter;
    }
}
