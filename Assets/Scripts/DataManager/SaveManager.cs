using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


// Saves new changes and load save at start
public class SaveManager : MonoBehaviour
{
    [SerializeField] private UnityEvent<int, Vector2> _loadActions; // Event to draw loaded facilities
    // Unique key - object position and each object is represented with id in prefabs list(check resourses/config)
    private Dictionary<Vector2, int> _savedObjects = new();
    private string fileName;
    
    // Save new modification of map
    public void SaveChanges(int objId, Vector2 position)
    {
        // Negative id means object was deleted
        if (objId >= 0)
            SaveLine(objId, position);
        else
            DeleteLine(position);   // Need just key (position)
    }

    private void DeleteLine(Vector2 value)
    {
        _savedObjects.Remove(value);    // Value is object position

        // Leave only the lines without a key
        var linesToKeep = File.ReadLines(fileName).Where(l => HasNoValue(l, in value));
        File.WriteAllLines(fileName, linesToKeep.ToArray<string>());
    }

    // Check string for has no key (position) in it
    private bool HasNoValue(string l, in Vector2 val)
    {
        var args = l.Split(';');    // 3 args id;x;y

        if (args.Length < 3)
            return true;

        // Is there no match with key
        bool xNoMatch = !Mathf.Approximately(float.Parse(args[1]), val.x);
        bool yNoMatch = !Mathf.Approximately(float.Parse(args[2]), val.y);
        return xNoMatch || yNoMatch;
    }
    
    private void LoadSave()
    {
        var objLines = File.ReadAllLines(fileName);

        foreach(var l in objLines)
        {
            var args = l.Split(';');

            if (args.Length < 3)
                continue;

            // Parse line and send message to draw new object with id in readed position
            Vector2 pos = new Vector2(float.Parse(args[1]), float.Parse(args[2]));
            _loadActions.Invoke(int.Parse(args[0]), pos);
        }
    }

    private void SaveLine(int id, Vector2 value)
    {
        _savedObjects.Add(value, id);

        using (StreamWriter sw = File.AppendText(fileName))
            sw.WriteLine($"{id};{value.x};{value.y}");
    }

    private void Start()
    {
        fileName = Application.dataPath + "/save.dat";

        if (!File.Exists(fileName))
            File.Create(fileName);

        LoadSave();
    }
}