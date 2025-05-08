using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoLight : MonoBehaviour
{
    [SerializeField] private Light directionalLight;
    [SerializeField] private float colorSpeed = 1f;
    private int stage = 0;
    private Color currentColor = new Color(0, 0, 0, 1);

    [SerializeField] private float rotationSpeed = 5f;
    private float time = 0f;
    public Vector2 rotationXRange = new Vector2(0, 30);
    public Vector2 rotationYRange = new Vector2(-15, 15);

    public static DiscoLight Instance;

    public Color CurrentColor => Instance.currentColor;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        time += Time.deltaTime * rotationSpeed;
        float rotationX = Mathf.PingPong(time, rotationXRange.y - rotationXRange.x) + rotationXRange.x;
        float rotationY = Mathf.PingPong(time, rotationYRange.y - rotationYRange.x) + rotationYRange.x;
        directionalLight.transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);

        float delta = colorSpeed * Time.deltaTime * 255;

        switch (stage)
        {
            case 0:
                currentColor.r += delta / 255f;
                if (currentColor.r >= 1f)
                {
                    currentColor.r = 1f;
                    stage = 1;
                }
                break;

            case 1:
                currentColor.r -= delta / 255f;
                currentColor.g += delta / 255f;
                if (currentColor.r <= 0f && currentColor.g >= 1f)
                {
                    currentColor.r = 0f;
                    currentColor.g = 1f;
                    stage = 2;
                }
                break;

            case 2:
                currentColor.g -= delta / 255f;
                currentColor.b += delta / 255f;
                if (currentColor.g <= 0f && currentColor.b >= 1f)
                {
                    currentColor.g = 0f;
                    currentColor.b = 1f;
                    stage = 3;
                }
                break;

            case 3:
                currentColor.r += delta / 255f;
                currentColor.b -= delta / 255f;
                if (currentColor.r >= 1f && currentColor.b <= 0f)
                {
                    currentColor.r = 1f;
                    currentColor.b = 0f;
                    stage = 0;
                }
                break;
        }
        directionalLight.color = currentColor;
    }
}
