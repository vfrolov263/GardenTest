using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// For interract with UI panel events
public class GameControls : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] UnityEvent<int> _mouseOnUIAction;

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        _mouseOnUIAction.Invoke(1);
    }

    //Detect when Cursor leaves the GameObject
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        _mouseOnUIAction.Invoke(0);
    }
}