using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PacmanAnimationHandler : NetworkBehaviour
{
    private Pacman pacman;
    public float rotationSpeed = 100f;
    private Transform modelHolder;
    private GameObject model;

    private float timeBtwBlinks = 0.1f;
    private float blinkElapsed = 0f;
    private NetworkVariable<bool> isModelActive = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Awake()
    {
        pacman = GetComponent<Pacman>();
        modelHolder = transform.GetChild(0);
        model = modelHolder.GetChild(0).gameObject;
        isModelActive.OnValueChanged += OnModelActiveChanged;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        isModelActive.OnValueChanged -= OnModelActiveChanged;
    }

    private void Update()
    {
        HandleRotation();
        HandlePacmanInvincibleBlinking();
    }

    private void HandleRotation()
    {
        if (IsOwner == false) return;
        Vector3 targetPosition = pacman.nodeAhead.GetWorldPosition();
        Vector3 direction = targetPosition - modelHolder.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
        float currentAngle = modelHolder.eulerAngles.z;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, angle, rotationSpeed * Time.deltaTime);
        modelHolder.rotation = Quaternion.Euler(0f, 0f, newAngle);
    }

    private void HandlePacmanInvincibleBlinking()
    {
        if (IsOwner == false) return;

        if (pacman.IsDeadTemporary)
        {
            isModelActive.Value = false;
            return;
        }

        if (pacman.IsInvincible)
        {
            blinkElapsed += Time.deltaTime;
            if (blinkElapsed >= timeBtwBlinks)
            {
                isModelActive.Value = !isModelActive.Value;
                blinkElapsed = 0f;
            }
        }
        else
        {
            if (!isModelActive.Value)
            {
                isModelActive.Value = true;
            }
        }
    }

    private void OnModelActiveChanged(bool previousValue, bool newValue)
    {
        model.SetActive(newValue);
    }
}
