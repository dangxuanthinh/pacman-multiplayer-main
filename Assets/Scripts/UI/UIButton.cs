using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool ButtonPressed { get; set; }
    public bool ButtonDown { get; set; }
    public bool ButtonUp { get; set; }

    private float currentPressTime;
    private float maxPressTime = 2f;

    [HideInInspector] public Button button;

    [SerializeField] private bool playAudio;
    [SerializeField] private bool playAnimation;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
            Debug.Log(gameObject.name);
        button.onClick.AddListener(PlaySound);
    }

    private void Update()
    {
        if (ButtonPressed)
        {
            ButtonDown = currentPressTime == 0f;
            currentPressTime += Time.deltaTime;
            if (currentPressTime >= maxPressTime)
            {
                currentPressTime = maxPressTime;
            }
        }
        else
        {
            ButtonUp = currentPressTime > 0f;
            currentPressTime = 0;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ButtonPressed = true;
        if (playAnimation)
        {
            button.transform.DOScale(0.85f, 0.05f).SetUpdate(UpdateType.Normal).SetUpdate(true);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ButtonDown = false;
        ButtonPressed = false;
        if (playAnimation)
            button.transform.DOScale(1f, 0.15f).SetEase(Ease.OutBack).SetUpdate(UpdateType.Normal).SetUpdate(true);
    }

    private void PlaySound()
    {
        if (playAudio)
            AudioManager.Instance.Play("ButtonPressed", true);
    }
}

