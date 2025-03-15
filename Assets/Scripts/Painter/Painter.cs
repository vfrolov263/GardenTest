using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// Move brush and draw/erase choosen object
public class Painter : MonoBehaviour
{
    [SerializeField] ObjectsCanvas _objCanvas; // Storage with objects on map
    [SerializeField] private InputAction _pressAction, _screenPosition;
    [SerializeField] GameObject _eraser;
    [SerializeField] UnityEvent<int, Vector2> _dataChangeAction;    // Action after drawing some object
    private List<GameObject> _facilities;   // Palette of available objects to draw. Will take it from config
    private int _choosenFacility;   // index of actual object on brush
    private GameObject _facility;   // actual object on brush
    private DrawMode _drawMode;
    private Vector2 _cellSize;   // Will take it from config
    private bool _waiting;  // When user work with UI pannel

    private enum DrawMode
    {
        Draw,
        Delete,
    }

    public void ChangeDrawMode(int mode)
    {
        DrawMode newMode = mode > 0 ? DrawMode.Draw : DrawMode.Delete;

        if (_drawMode == newMode)
            return;

        _drawMode = newMode;

        if (_drawMode == DrawMode.Delete)
        {
            if (_facility != null)
                Destroy(_facility); // Destroy current brush object

            _facility = _eraser;
            _choosenFacility = -1;  // No drawable object on brush
        }
        else
            _facility = null;
    }

    public void DrawFacility(int id, Vector2 position)
    {
        if (id < 0 || id >= _facilities.Count)
            return;

        GameObject facility = Instantiate(_facilities[id], position, Quaternion.identity);
        _objCanvas.Put(facility);
    }

    public void SelectFacility(int index)
    {
        if (index >= 0 && index < _facilities.Count && _choosenFacility != index)
        {
            if (_drawMode != DrawMode.Draw)
                ChangeDrawMode(1);
            else if (_facility != null)
                Destroy(_facility);

            _choosenFacility = index;
            _facility = Instantiate(_facilities[_choosenFacility], transform);
            _facility.SetActive(!_waiting);
        }
    }

    // Set wait mode when user work with UI panel
    public void Wait(int isOnUI)
    {
        _waiting = isOnUI > 0;
        
        // When wait choosen object on brush not active
        if (_facility != null)
            _facility.SetActive(!_waiting);       
    }

    private void Awake()
    {
        // Input settings
        _screenPosition.Enable();
        _pressAction.Enable();
        _screenPosition.performed += context => { SetNewPosition(context.ReadValue<Vector2>()); };
        _pressAction.performed += _ => { BrushStroke(); };
    }

    // User click on screen
    private void BrushStroke()
    {
        if (_facility == null || _waiting)
            return;

        switch (_drawMode)
        {
            case DrawMode.Draw:
                DrawFacility();
                break;
            case DrawMode.Delete:
                DeleteFacility();
                break;
            default:
                break;
        }
    }

    // Check if brush over an empty area or over some object
    private void CheckBrushState()
    {
        SpriteRenderer sprite = _facility.GetComponent<SpriteRenderer>();
       
        if (_objCanvas.IsObjectInArea(sprite.bounds))
            sprite.color = Color.red;   // When over some object
        else
            sprite.color = Color.white;
    }
    
    private void DeleteFacility()
    {
        _objCanvas.Delete(_facility.GetComponent<SpriteRenderer>().bounds);
    }

    private void DrawFacility()
    {
        // When brush over some object - can't draw
        if (_facility == null || _objCanvas.IsObjectInArea(_facility.GetComponent<SpriteRenderer>().bounds))
            return;

        _objCanvas.Put(_facility);
        // Send message about new object. Position is key
        _dataChangeAction.Invoke(_choosenFacility, _facility.transform.position);
        _facility = null;
        int facilityIndex = _choosenFacility;
        _choosenFacility = -1;
        SelectFacility(facilityIndex);
    }

    private void OnEnable()
    {
        // Get palette of available objects and cell size of tiles grid
        var configs = Resources.LoadAll("", typeof(GameConfig));
        
        if (configs.Length > 0)
        {
            _facilities = ((GameConfig)configs[0]).objectsPalette;
            _cellSize = ((GameConfig)configs[0]).cellSize;
        }
        else
        {
            _facilities = new List<GameObject>();
            _cellSize = new Vector2(1f, 1f);
        }
    }

    // Set new brush position when move mouse/touch
    private void SetNewPosition(Vector2 mousePos)
    {
        if (_waiting)
            return;

        // Calculate position on tiles grid
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.x = (float)Math.Round(worldPos.x / _cellSize.x) * _cellSize.x;
        worldPos.y = (float)Math.Round(worldPos.y / _cellSize.y) * _cellSize.y;
        worldPos.z = 0;
        transform.position = worldPos;

        if (_facility != null)
            CheckBrushState();
    }

    private void Start()
    {
        _choosenFacility = -1;  // No choosen object to draw
        _eraser.SetActive(false);
    }
}