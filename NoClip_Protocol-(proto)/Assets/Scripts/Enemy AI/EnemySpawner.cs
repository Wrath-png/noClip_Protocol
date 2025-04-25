using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Linq;

public class EnemySpawner : MonoBehaviour
{
    public Transform playerSpawn; 

    [Header("Enemy Prefab(s)")]
    public GameObject enemyPrefab;

    [Header("Spawn Settings")]
    public int EnemiesToSpawn = 17;

    [Header("Waypoints (each path is a list of transforms)")]
    public PatrolRouteMono[] patrolRoutes;

    [Header("Fallback Spawn Points")]
    public List<Transform> wanderSpawnPoints;

    [Header("Small Enemy Base Stats")]
    public float baseSpeed = 15f;
    public float baseHealth = 100;
    public float dmgMult = 1;
    public float baseSightRange = 80f;
    public float baseAttackRange = 50f;
    public float baseWanderRadius = 45f;
    public float baseMemory = 2;

    [Header("Behavior Flags")]
    public bool canAttack = true;
    public bool canWander = false;
    public bool canPatrol = true;
    public bool canChase = true;

    [Header("Difficulty Scaling Curves")]
    [Tooltip("X is normalized path index (0 = easiest, 1 = hardest). Y is multiplier.")]
    public AnimationCurve healthCurve = AnimationCurve.Linear(0, 1, 1, 2);
    public AnimationCurve speedCurve = AnimationCurve.Linear(0, 1, 1, 1.5f);
    public AnimationCurve damageCurve = AnimationCurve.Linear(0, 1, 1, 2f);

    public AnimationCurve sightRangeCurve = AnimationCurve.Linear(0, 1, 1, 2);
    public AnimationCurve attackRangeCurve = AnimationCurve.Linear(0, 1, 1, 1.5f);
    public AnimationCurve wanderRadiusCurve = AnimationCurve.Linear(0, 1, 1, 2);
    public AnimationCurve memoryCurve = AnimationCurve.Linear(0, 1, 1, 2);
    public AnimationCurve canAttackCurve = AnimationCurve.Linear(0, 0, 1, 1); // False to True
    public AnimationCurve canWanderCurve = AnimationCurve.Linear(0, 0, 1, 1); // False to True
    public AnimationCurve canPatrolCurve = AnimationCurve.Linear(0, 0, 1, 1); // False true
    public AnimationCurve canChaseCurve = AnimationCurve.Linear(0, 1, 1, 1); // Always to True

    private float maxDistance;
    void Awake() 
    {
        patrolRoutes = FindObjectsByType<PatrolRouteMono>(FindObjectsSortMode.None);
    }
    void Start()
    {
        StartCoroutine(StartLevelSpawnEnemies());   
    }

    //At the start of each level, spawn an enemy on every patrol route
    IEnumerator StartLevelSpawnEnemies() 
    {
        // Calculate distances
        foreach (var route in patrolRoutes)
        {
            route.CalculateDistanceFrom(playerSpawn);
        }
        maxDistance = patrolRoutes.Max(r => r.distanceFromStart);
        // Sort by distance for difficulty scaling (optional)
        var sortedRoutes = patrolRoutes.OrderBy(r => r.distanceFromStart).ToArray();

        foreach (var route in sortedRoutes)
        {
            SpawnEnemy(route);
            yield return new WaitForSeconds(0.1f);
        }

        // for (int i = 0; i < EnemiesToSpawn; i++)
        // {
        //     SpawnEnemy();
        //     yield return new WaitForSeconds(0.1f);  // Time between each spawn, can be adjusted
        // }
    }

    void SpawnEnemy(PatrolRouteMono route) {
        // int pathIndex = Random.Range(0, patrolRoutes.Length);
        // Transform[] path = patrolRoutes[pathIndex].GetWaypoints();
        // float difficultyNormalized = pathIndex / (float)(patrolRoutes.Length - 1);

        Transform[] path = route?.GetWaypoints();
        bool hasRoute = path != null && path.Length > 0;

        float difficultyNormalized = route != null && patrolRoutes.Length > 1
            ? Mathf.Clamp01(route.distanceFromStart / maxDistance)
            : 0f;

        // Calculate scaled stats
        float health = baseHealth * healthCurve.Evaluate(difficultyNormalized);
        float speed = baseSpeed * speedCurve.Evaluate(difficultyNormalized);
        float damage = dmgMult * damageCurve.Evaluate(difficultyNormalized);

        float sightRange = baseSightRange * sightRangeCurve.Evaluate(difficultyNormalized);
        float attackRange = baseAttackRange * attackRangeCurve.Evaluate(difficultyNormalized);
        float wanderRadius = baseWanderRadius * wanderRadiusCurve.Evaluate(difficultyNormalized);
        float memory = baseMemory * memoryCurve.Evaluate(difficultyNormalized);

        bool attackAllowed = canAttackCurve.Evaluate(difficultyNormalized) > 0.5f;
        bool wanderAllowed = !hasRoute || canWanderCurve.Evaluate(difficultyNormalized) > 0.5f;
        bool patrolAllowed = hasRoute && canPatrolCurve.Evaluate(difficultyNormalized) > 0.5f;
        bool chaseAllowed = canChaseCurve.Evaluate(difficultyNormalized) > 0.5f;

        //Find correct spawning position
        Vector3 spawnPos;
        if (hasRoute) {
            spawnPos = path[0].position;
        }
        else if (wanderSpawnPoints != null && wanderSpawnPoints.Count > 0) {
            int index = Random.Range(0, wanderSpawnPoints.Count);
            spawnPos = wanderSpawnPoints[index].position;
        }
        else {
            Debug.LogWarning("No patrol route or wander spawn point available. Enemy not spawned.");
            return;
        }

        // Spawn the enemy
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        // Set up enemy stats and path
        SmallEnemyAI ai = newEnemy.GetComponent<SmallEnemyAI>();
        if (ai != null)
        {
            if (hasRoute)
                ai.SetPatrolPath(path);
            ai.SetStats(speed, damage, sightRange, attackRange, wanderRadius, memory);
            ai.SetBehaviors(attackAllowed, wanderAllowed, patrolAllowed, chaseAllowed);
        }

        EnemyHealth healthScript = newEnemy.GetComponent<EnemyHealth>();
        if (healthScript != null) {
            int roundedHealth = Mathf.RoundToInt(health);
            healthScript.SetMaxHealth(roundedHealth);
        }
        
        Debug.Log($"Spawned enemy at {(hasRoute ? "patrol" : "wander")} route. Difficulty: {difficultyNormalized:F2}");
    }
}
