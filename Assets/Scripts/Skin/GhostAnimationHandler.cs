using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GhostAnimationHandler : NetworkBehaviour
{
    private Ghost ghost;
    public float rotationSpeed = 100f;
    private Transform modelHolder;
    private Transform ghostEatenEye;
    private Transform ghostModel;
    private SkinnedMeshRenderer ghostMesh;

    private Color normalColor;
    private float pingpongT;

    private Flasher minimapIconFlasher;

    private void Awake()
    {
        ghost = GetComponent<Ghost>();

        modelHolder = transform.GetChild(0);
        ghostModel = modelHolder.GetChild(0);
        ghostEatenEye = modelHolder.GetChild(1);
        ghostMesh = ghostModel.GetComponent<SkinnedMeshRenderer>();
        normalColor = ghostMesh.material.GetColor("_EmissionColor");
        minimapIconFlasher = GetComponentInChildren<Flasher>();
    }

    private void Update()
    {
        if (IsServer)
        {
            Vector3 targetPosition = ghost.nextNode.GetWorldPosition();
            Vector3 direction = targetPosition - modelHolder.position;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
            float currentAngle = modelHolder.eulerAngles.z;
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, angle, rotationSpeed * Time.deltaTime);

            modelHolder.rotation = Quaternion.Euler(0f, 0f, newAngle);
        }

        if (ghost.ghostState.Value == GhostState.Frightened)
        {
            pingpongT += Time.deltaTime * 10f;
            float t = Mathf.PingPong(pingpongT, 1f);
            Color newColor = Color.Lerp(Color.black, normalColor, t);
            ghostMesh.material.SetColor("_EmissionColor", newColor);
            if (minimapIconFlasher)
                minimapIconFlasher.SetFlashing(true);
        }
        else
        {
            ghostMesh.material.SetColor("_EmissionColor", normalColor);
            if (minimapIconFlasher)
                minimapIconFlasher.SetFlashing(false);
        }
        ghostEatenEye.gameObject.SetActive(ghost.ghostState.Value == GhostState.Eaten);
        ghostModel.gameObject.SetActive(ghost.ghostState.Value != GhostState.Eaten);
    }
}

