using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class LazerPowerup : PowerUp
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform particle;
    [SerializeField] private LayerMask attackMask;

    public NetworkVariable<Vector3> lineStartPosition = new NetworkVariable<Vector3>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Vector3> lineEndPosition = new NetworkVariable<Vector3>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Awake()
    {
        lineStartPosition.OnValueChanged += OnLineStartPositionChanged;
        lineEndPosition.OnValueChanged += OnLineEndPositionChanged;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        lineStartPosition.OnValueChanged -= OnLineStartPositionChanged;
        lineEndPosition.OnValueChanged -= OnLineEndPositionChanged;
    }

    private void OnLineStartPositionChanged(Vector3 oldPosition, Vector3 newPosition)
    {
        SetStartPosition(newPosition);
    }

    private void OnLineEndPositionChanged(Vector3 oldPosition, Vector3 newPosition)
    {
        SetEndPosition(newPosition);
    }

    private void Update()
    {
        if (!IsOwner) return;
        particle.transform.position = lineRenderer.GetPosition(1);
    }

    private void FixedUpdate()
    {
        if (!IsOwner || player == null) return;
        lineStartPosition.Value = player.transform.position;
        RaycastHit hit;
        if (Physics.Raycast(player.transform.position, player.moveDirectionVector, out hit, 300f, attackMask))
        {
            lineEndPosition.Value = hit.point;
            if (hit.transform.CompareTag("Ghost"))
            {
                GameManager.Instance.EatGhost(hit.transform.GetComponent<Ghost>());
            }
        }
        else
        {
            lineEndPosition.Value = (Vector2)player.transform.position + player.moveDirectionVector * 100f;
        }
    }

    public void SetStartPosition(Vector3 startPosition)
    {
        lineRenderer.SetPosition(0, startPosition);
    }

    public void SetEndPosition(Vector3 endPosition)
    {
        lineRenderer.SetPosition(1, endPosition);
    }

    public override void Setup(Pacman player)
    {
        AudioManager.Instance.Play("LazerPowerup");
    }

    public override void Cleanup(Pacman player)
    {
        Debug.Log("Stoping lazer sound");
        AudioManager.Instance.Stop("LazerPowerup");
    }
}
