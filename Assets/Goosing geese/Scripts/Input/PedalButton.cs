using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PedalButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private InputSwitch _inputSwitch;

    private void OnDisable()
    {
        if (_inputSwitch != null)
            _inputSwitch.SetPedalPressed(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _inputSwitch.SetPedalPressed(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _inputSwitch.SetPedalPressed(false);
    }
}