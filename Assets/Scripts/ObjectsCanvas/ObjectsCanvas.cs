using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

// Storage of objectson map
public class ObjectsCanvas : MonoBehaviour
{
    [SerializeField] UnityEvent<int, Vector2> _dataChangeAction;    // Action to do after delete some object from map
    private List<GameObject> _objects = new List<GameObject>();
    private bool[,] _occupiedCells; // Cell occupancy array 
    private Vector2Int _mapOffset; // For calculate cell occupancy array indices
    private Vector2 _cellSize; // Will take it from config

    // Delete object under given area
    public void Delete(in Bounds areaBounds)
    {
        Vector2Int minCell, maxCell, collCell;

        // Find cells relevant areas
        if (!FindObjectCells(areaBounds, out minCell, out maxCell))
            return;

        // Find collision cell
        if (HasCollision(out collCell, minCell, maxCell))
        {
            collCell += _mapOffset; // Take into account map offset
            // Calculate point to ray from
            Vector2 collPoint = new Vector3(collCell.x + _cellSize.x / 2f, collCell.y + _cellSize.y / 2f);
            RaycastHit2D hit = Physics2D.Raycast(collPoint, Vector2.zero);

            if (hit.collider != null)
            {
                GameObject objToDel = hit.collider.gameObject;
                // Freeing up cells
                SetObjectCells(objToDel.GetComponent<Collider2D>().bounds, false);
                _objects.Remove(objToDel);
                // Send message that object was deleted, position its key
                _dataChangeAction.Invoke(-1, objToDel.transform.position);
                Destroy(objToDel.gameObject);
            }
        }
    }

    // Check for some object in given area
    public bool IsObjectInArea(in Bounds areaBounds)
    {
        Vector2Int minCell, maxCell;
        return FindObjectCells(areaBounds, out minCell, out maxCell) && HasCollision(out _, minCell, maxCell);
    }

    // Put new object on map
    public void Put(GameObject gameObject)
    {
        if (gameObject == null)
            return;

        gameObject.transform.parent = null; // Untie from painter
        _objects.Add(gameObject);

        Collider2D objCollider = gameObject.GetComponent<Collider2D>();
        
        if (objCollider != null)
            SetObjectCells(objCollider.bounds, true);   // Calculate new occupied cells
    }

    // Find cells corresponding to given area (from min to max)
    private bool FindObjectCells(in Bounds objectBounds, out Vector2Int minCell, out Vector2Int maxCell)
    {
        // Calculate сonsidering tiles grid size
        minCell = new Vector2Int();
        minCell.x = (int)(Math.Floor(objectBounds.min.x / _cellSize.x) * _cellSize.x);
        minCell.y = (int)(Math.Floor(objectBounds.min.y /  _cellSize.y) * _cellSize.y);

        maxCell = new Vector2Int();
        maxCell.x = (int)(Math.Floor(objectBounds.max.x / _cellSize.x) * _cellSize.x);
        maxCell.y = (int)(Math.Floor(objectBounds.max.y /  _cellSize.y) * _cellSize.y);

        // Take into account map offset
        minCell -= _mapOffset;
        maxCell -= _mapOffset;

        // Check for out of array size and return
        maxCell.x = Math.Min(maxCell.x, _occupiedCells.GetLength(0) - 1);
        maxCell.y = Math.Min(maxCell.y, _occupiedCells.GetLength(1) - 1);
        return minCell.x >= 0 && minCell.y >= 0 && maxCell.x >= 0 && maxCell.y >= 0;
    }

    // Find collision point with given area (from min to max) and return true if collision detected
    private bool HasCollision(out Vector2Int collisionPoint, in Vector2Int minCell, in Vector2Int maxCell)
    {
        collisionPoint = Vector2Int.zero;

        for (int i = minCell.x; i <= maxCell.x; i++)
            for (int j = minCell.y; j <= maxCell.y; j++)
                if (_occupiedCells[i, j])
                {
                    collisionPoint.x = i;
                    collisionPoint.y = j;
                    return true;
                }
        
        return false;
    }

    private void OnEnable()
    {
        GameObject tilemap = GameObject.FindWithTag("Tilemap");

        if (tilemap == null)
        {
            _occupiedCells = new bool[0,0];
            return;
        }

        // Create an array of cells to record occupied
        Tilemap map = tilemap.GetComponent<Tilemap>();
        map.CompressBounds();
        _mapOffset.x = map.cellBounds.min.x;
        _mapOffset.y = map.cellBounds.min.y;
        _occupiedCells = new bool[map.size.x, map.size.y];

        var configs = Resources.LoadAll("", typeof(GameConfig));
        _cellSize = configs.Length > 0 ? ((GameConfig)configs[0]).cellSize : new Vector2(1f, 1f);
    }

    // Set cells corresponding to given area to status
    private void SetObjectCells(in Bounds objectBounds, bool status)
    {
        Vector2Int minCell, maxCell;

        if (!FindObjectCells(objectBounds, out minCell, out maxCell))
            return;

        for (int i = minCell.x; i <= maxCell.x; i++)
            for (int j = minCell.y; j <= maxCell.y; j++)
                _occupiedCells[i, j] = status;
    }
}
