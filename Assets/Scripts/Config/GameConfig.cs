using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Scriptable Objects/Game Config")]
public class GameConfig : ScriptableObject
{
    public Vector2 cellSize;    // size of tilemap grid

    public List<GameObject> objectsPalette; // facilities list wich can be added to map
}