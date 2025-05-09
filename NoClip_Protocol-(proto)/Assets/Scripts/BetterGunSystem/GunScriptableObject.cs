using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu (fileName = "Gun", menuName = "Guns/Gun", order = 0)]
public class GunScriptableObject : ScriptableObject
{
    public GunType Type;
    //public ImpactType ImpactType; 
    //Will add surface manager to manage impact effects and sounds
    public string Name;
    public GameObject ModelPrefab;
    public Vector3 SpawnPoint;
    public Vector3 SpawnRotation;
    
    public DamageConfigScriptableObject DamageConfig;
    public ShootConfigurationScriptableObject ShootConfig;
    public TrailConfigScriptableObject TrailConfig;

    private MonoBehaviour ActiveMonoBehaviour;
    private GameObject Model;
    private float LastShootTime;
    private ParticleSystem ShootSystem;
    private ObjectPool<TrailRenderer> TrailPool;

    public GunRuntime Spawn(Transform parent, MonoBehaviour owner) 
    {
        GameObject modelInstance = Instantiate(ModelPrefab, parent);
        modelInstance.transform.localPosition = SpawnPoint;
        modelInstance.transform.localRotation = Quaternion.Euler(SpawnRotation);

        GunRuntime runtime = modelInstance.AddComponent<GunRuntime>();
        runtime.Initialize(this, owner);

        return runtime;
    }
}