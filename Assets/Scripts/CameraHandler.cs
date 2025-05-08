using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraHandler : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    public static CameraHandler Instance;

    private Pacman targetPlayer;

    [SerializeField] private CinemachineTransposer thirdPersonFollow;
    private CinemachineBasicMultiChannelPerlin multiChannelPerlin;

    private Vector3 normalBodyDamping = new Vector3(0.1f, 0.1f, 0f);
    private Vector3 desiredBodyDamping = new Vector3(0.1f, 0.1f, 0f);

    public UnityAction OnCameraSwitched;

    private void Awake()
    {
        Instance = this;
        thirdPersonFollow = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        multiChannelPerlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void Update()
    {
        if (targetPlayer == null)
        {
            virtualCamera.enabled = false;
        }
        else
        {
            virtualCamera.enabled = true;
        }
        HandleDeathPOV();
        HandleBodyDamping();
    }

    public void ShakeCamera(float intensity, float duration) => StartCoroutine(ShakeCameraCoroutine(intensity, duration));

    IEnumerator ShakeCameraCoroutine(float intensity, float duration)
    {
        multiChannelPerlin.m_AmplitudeGain = intensity;
        while (duration > 0)
        {
            duration -= Time.deltaTime;
            yield return null;
        }
        multiChannelPerlin.m_AmplitudeGain = 0f;
    }

    private void HandleBodyDamping()
    {
        if (targetPlayer == null) return;
        if (targetPlayer.IsNearTeleportNode())
        {
            desiredBodyDamping = Vector3.MoveTowards(desiredBodyDamping, Vector3.zero, 3f * Time.deltaTime);
        }
        else
        {
            desiredBodyDamping = Vector3.MoveTowards(desiredBodyDamping, normalBodyDamping, 3f * Time.deltaTime);
        }
        thirdPersonFollow.m_XDamping = desiredBodyDamping.x;
        thirdPersonFollow.m_XDamping = desiredBodyDamping.y;
    }

    private void HandleDeathPOV()
    {
        if (!GameManager.Instance.IsLocalClientDead) return;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetFollowTarget(GetNextPacman());
            OnCameraSwitched?.Invoke();
        }
    }

    public Pacman GetNextPacman()
    {
        int startIndex = GameManager.Instance.spawnedPlayers.IndexOf(targetPlayer);
        for (int i = 1; i <= GameManager.Instance.spawnedPlayers.Count; i++)
        {
            int index = (startIndex + i) % GameManager.Instance.spawnedPlayers.Count;
            if (GameManager.Instance.spawnedPlayers[index] != null)
            {
                return GameManager.Instance.spawnedPlayers[index];
            }
        }
        return null;
    }

    public void SetFollowTarget(Pacman target)
    {
        if (target != null)
        {
            virtualCamera.enabled = true;
            virtualCamera.Follow = target.transform;
            virtualCamera.LookAt = target.transform;
            targetPlayer = target;
        }
    }
}
