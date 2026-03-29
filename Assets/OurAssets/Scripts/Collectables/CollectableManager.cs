using System;
using UnityEngine;

public class CollectableManager : MonoBehaviour
{
    public static CollectableManager Instance { get; private set; }

    [SerializeField]
    Collectable collectablePrefab;
    [SerializeField, Tooltip("Length of array determines number of collectables for level")]
    Transform[] collectablesSpawnTransforms;
    [SerializeField]
    DialogueHolder holder;

    int TotalCollectables => collectablesSpawnTransforms.Length;
    int currentNumCollectables = 0;

    void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start() => System.Array.ForEach(collectablesSpawnTransforms, spawnTransform => Instantiate(collectablePrefab.gameObject, spawnTransform));

    public void CollectCollectable()
    {
        if (++currentNumCollectables >= TotalCollectables) holder.StartDialogue(); 
    }
}
