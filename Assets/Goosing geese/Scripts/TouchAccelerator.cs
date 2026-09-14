using UnityEngine;

public class TouchAccelerator : MonoBehaviour, IInputSource
{
    [SerializeField] private RectTransform _pedalArea;

    public bool IsAccelerating { get; private set; }

    private void Update()
    {
        IsAccelerating = false;

        for (int touchIndex = 0; touchIndex < Input.touchCount; touchIndex++)
        {
            Touch touch = Input.GetTouch(touchIndex);
            bool touchIsInsidePedalArea = RectTransformUtility.RectangleContainsScreenPoint(_pedalArea, touch.position);

            if (touchIsInsidePedalArea)
            {
                IsAccelerating = true;
                break;
            }
        }
    }
}
