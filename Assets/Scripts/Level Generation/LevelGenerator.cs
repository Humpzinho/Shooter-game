using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private Transform lastLevelPart;
    [SerializeField] private SnapPoint currentExitSnapPoint; // Reference to the last exit point where the next level part should connect
    private SnapPoint defaultSnapPoint;

    [SerializeField] private List<Transform> levelParts; // Collection of prefabs that can be used as level parts
    private List<Transform> currentLevelParts;
    [SerializeField] private List<Transform> generatedLevelParts = new();

    [SerializeField] private float generationCooldown;
    private float generationTimer;
    private bool generationActive;

    private void Start()
    {
        defaultSnapPoint = currentExitSnapPoint;
        currentLevelParts = new List<Transform>(levelParts); // Initialize the current level parts list
        InitializeGeneration();

    }

    private void Update()
    {
        if (!generationActive)
            return;

        generationTimer -= Time.deltaTime;

        if (generationTimer <= 0)
        {
            if (currentLevelParts.Count > 0)
            {
                GenerateNextLevelPart();
                generationTimer = generationCooldown;
            }
            else
            {
                FinishGeneration();
            }
        }
    }

    private void FinishGeneration()
    {
        generationActive = false; // Stop generation if no parts are left
        GenerateNextLevelPart();

        navMeshSurface.BuildNavMesh();
    }

    [ContextMenu("Restart Generation")]
    private void InitializeGeneration()
    {
        generationActive = true;
        currentLevelParts = new List<Transform>(levelParts); // Initialize the current level parts list
        currentExitSnapPoint = defaultSnapPoint;

        DestroyOldLevelParts();

    }

    private void DestroyOldLevelParts()
    {
        foreach (Transform t in generatedLevelParts)
            Destroy(t.gameObject);

        generatedLevelParts.Clear();
    }

    [ContextMenu("Generate Next Level Part")]
    private void GenerateNextLevelPart()
    {
        Transform nextLevelPart;

        if (generationActive)
            nextLevelPart = Instantiate(GetRandomLevelPart());
        else
            nextLevelPart = Instantiate(lastLevelPart);


        generatedLevelParts.Add(nextLevelPart);

        LevelPart levelPartScript = nextLevelPart.GetComponent<LevelPart>();
        // Connect the new part to the current exit point and update the exit point
        levelPartScript.SnapAndAlignLevelPart(currentExitSnapPoint);
        currentExitSnapPoint = levelPartScript.GetExitSnapPoint();


        if (levelPartScript.IntersectionDetected())
        {
            Debug.LogWarning("intersection detected");
            InitializeGeneration();
        }

    }

    private Transform GetRandomLevelPart()
    {
        int randomIndex = Random.Range(0, currentLevelParts.Count);
        Transform nextLevelPart = currentLevelParts[randomIndex];
        currentLevelParts.RemoveAt(randomIndex); // Remove used part to prevent repetition
        return nextLevelPart;
    }
}
