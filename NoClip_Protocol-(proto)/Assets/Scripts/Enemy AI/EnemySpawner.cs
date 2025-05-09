using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;
using System;

public class EnemySpawner : MonoBehaviour
{
    private LevelManager levelManager;
    public Transform playerSpawn; 

    [Header("Enemy Prefab(s)")]
    public GameObject enemyPrefab;

    [Header("Spawn Settings")]
    public int EnemiesToSpawn = 17;

    [Header("Waypoints (each path is a list of transforms)")]
    public PatrolRouteMono[] patrolRoutes;

    [Header("Fallback Spawn Points")]
    public Transform[] wanderPoints;

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
    // void Start()
    // {
    //     levelManager = FindFirstObjectByType<LevelManager>();
    //     if (levelManager == null) {
    //         Debug.LogError("LevelManager not found in the scene!");
    //     }
    //     StartCoroutine(StartLevelSpawnEnemies());   
    // }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        StopAllCoroutines();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex != 2) return;  //Skip if not in levelOne

        StopAllCoroutines();
        // Re-find references here after scene reload
        playerSpawn = GameObject.FindGameObjectWithTag("PlayerSpawn").transform;
        patrolRoutes = FindObjectsByType<PatrolRouteMono>(FindObjectsSortMode.None);
        wanderPoints = GameObject.FindGameObjectsWithTag("WanderPoint").Select(obj => obj.transform).ToArray();

        if (levelManager == null) {
            levelManager = FindFirstObjectByType<LevelManager>();
            if (levelManager == null) {
            Debug.LogError("LevelManager not found in the scene!");
            }
        }
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

        //Increase base states by 10% per level
        int level = Math.Max(1, levelManager.CurrentLevel);
        float levelMultiplier = (float)Math.Pow(1.1f, level - 1);

        float health = baseHealth * levelMultiplier;
        float speed = baseSpeed * levelMultiplier;
        float damage = dmgMult * levelMultiplier;   //Might need fine tuning
        float sightRange = baseSightRange * levelMultiplier;
        float attackRange = baseAttackRange * levelMultiplier;
        float wanderRadius = baseWanderRadius * levelMultiplier;
        float memory = baseMemory * levelMultiplier;

        // Calculate scaled stats
        health *= healthCurve.Evaluate(difficultyNormalized);
        speed *= speedCurve.Evaluate(difficultyNormalized);
        damage *= damageCurve.Evaluate(difficultyNormalized);

        sightRange *= sightRangeCurve.Evaluate(difficultyNormalized);
        attackRange *= attackRangeCurve.Evaluate(difficultyNormalized);
        wanderRadius *= wanderRadiusCurve.Evaluate(difficultyNormalized);
        memory *= memoryCurve.Evaluate(difficultyNormalized);

        float levelNormalized = Mathf.Clamp01((levelManager.CurrentLevel - 1) / 9f); 
        // 0 at level 1, 1 at level 10+
        //Meaning over level 10, all enemies have access to all states.

        bool attackAllowed = canAttackCurve.Evaluate(difficultyNormalized + levelNormalized * 0.5f) > 0.5f;
        bool wanderAllowed = !hasRoute || canWanderCurve.Evaluate(difficultyNormalized + levelNormalized * 0.5f) > 0.5f;
        bool patrolAllowed = hasRoute && canPatrolCurve.Evaluate(difficultyNormalized + levelNormalized * 0.5f) > 0.5f;
        bool chaseAllowed = canChaseCurve.Evaluate(difficultyNormalized + levelNormalized * 0.5f) > 0.5f;
        
        //Find correct spawning position
        Vector3 spawnPos;
        if (hasRoute) {
            spawnPos = path[0].position;
        }
        else if (wanderPoints != null && wanderPoints.Length > 0) {
            int index = UnityEngine.Random.Range(0, wanderPoints.Length);
            spawnPos = wanderPoints[index].position;
        }
        else {
            Debug.LogWarning("No patrol route or wander spawn point available. Enemy not spawned.");
            return;
        }

        // Spawn the enemy
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        newEnemy.name = "Enemy_" + UnityEngine.Random.Range(1000, 9999);

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
            ScaleSize(newEnemy, health, baseHealth * levelMultiplier);
        }
        
        Debug.Log($"Spawned enemy at {(hasRoute ? "patrol" : "wander")} route. Difficulty: {difficultyNormalized:F2}");
    }

    private void ScaleSize(GameObject enemy, float health, float baseHealth) {
        float scaleMultiplier =  Mathf.Clamp(health / baseHealth, 1f, 2f);
        enemy.transform.localScale = Vector3.one * scaleMultiplier;

        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.baseOffset = 10f / scaleMultiplier;
        }
    }

}
