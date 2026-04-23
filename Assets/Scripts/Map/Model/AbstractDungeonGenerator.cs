using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractDungeonGenerator : MonoBehaviour
{
    [SerializeField]
    protected TilemapVisualizer tilemapVisualizer = null;
    [SerializeField]
    protected Vector2Int startPosition = Vector2Int.zero;
    [SerializeField]
    protected bool generateOnStart = true;

    private IEnumerator Start()
    {
        if (generateOnStart)
        {
            yield return null;
            yield return new WaitForEndOfFrame(); 
            
            Debug.Log($"Generator na obiekcie '{gameObject.name}' rozpoczyna automatyczne generowanie lochu...");
            GenerateDungeon();
        }
    }

    public void GenerateDungeon()
    {
        tilemapVisualizer.Clear();
        RunProceduralGeneration();
    }

    protected abstract void RunProceduralGeneration();
}