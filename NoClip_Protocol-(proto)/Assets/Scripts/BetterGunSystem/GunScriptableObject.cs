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

    public void Spawn(Transform Parent, MonoBehaviour ActiveMonoBehaviour) 
    {
        this.ActiveMonoBehaviour = ActiveMonoBehaviour;
        LastShootTime = 0;  //Not properly reset in editor, in build it's fine
        TrailPool = new ObjectPool<TrailRenderer> (CreateTrail);
        Model = Instantiate(ModelPrefab);
        Model.transform.SetParent(Parent,false);
        Model.transform.localPosition = SpawnPoint;
        Model.transform.localRotation = Quaternion.Euler(SpawnRotation);

        ShootSystem = Model.GetComponentInChildren<ParticleSystem>();
        var ownerAI = Model.GetComponentInParent<SmallEnemyAI>();

    }

    public void Shoot(float damageMultiplier)
    {
        if (Time.time > ShootConfig.FireRate + LastShootTime) {
            LastShootTime = Time.time;
            ShootSystem.Play();
            Vector3 shootDirection = ShootSystem.transform.forward 
                + new Vector3(
                    Random.Range(
                        -ShootConfig.Spread.x,
                        ShootConfig.Spread.x
                    ),
                    Random.Range(
                        -ShootConfig.Spread.y,
                        ShootConfig.Spread.y
                    ),
                    Random.Range(
                        -ShootConfig.Spread.z,
                        ShootConfig.Spread.z
                    )
                );
            shootDirection.Normalize();

            if (TrailConfig.UseLaserHitbox)
            {
                GameObject hitbox = Instantiate(TrailConfig.HitboxPrefab, ShootSystem.transform.position, Quaternion.LookRotation(shootDirection));
                HitboxMover mover = hitbox.GetComponent<HitboxMover>();
                mover.SetOwner(Model.GetComponentInParent<SmallEnemyAI>()?.gameObject);


                if (mover != null)
                {
                    mover.Initialize(
                        ShootSystem.transform.position,
                        shootDirection,
                        DamageConfig,
                        TrailConfig.shotSpeed,
                        damageMultiplier
                    );
                }

                return; // Exit early — no need for trail coroutine
            }

            if (Physics.Raycast(
                    ShootSystem.transform.position,
                    shootDirection,
                    out RaycastHit hit,
                    float.MaxValue,
                    ShootConfig.HitMask
                ))
            {
                //Debug.DrawRay(ShootSystem.transform.position, shootDirection * 100f, Color.red, 1f);

                ActiveMonoBehaviour.StartCoroutine(
                    PlayTrail(
                        ShootSystem.transform.position,
                        hit.point,
                        hit,
                        damageMultiplier
                    )
                );
                
            }
            else {
                ActiveMonoBehaviour.StartCoroutine(
                    PlayTrail(
                        ShootSystem.transform.position,
                        ShootSystem.transform.position + (shootDirection * TrailConfig.MissDistance),
                        new RaycastHit(),
                        1f
                        
                    )
                );
            }
        }
    }

    private IEnumerator PlayTrail(Vector3 StartPoint, Vector3 EndPoint, RaycastHit Hit, float damageMultiplier)
    {
        TrailRenderer instance = TrailPool.Get();
        instance.gameObject.SetActive(true);
        instance.transform.position = StartPoint;
        yield return null;

        instance.emitting = true;
        float distance = Vector3.Distance(StartPoint, EndPoint);
        float remainingDistance = distance;

        GameObject hitbox = Instantiate(TrailConfig.HitboxPrefab, StartPoint, Quaternion.identity);
        Rigidbody hitboxRb = hitbox.GetComponent<Rigidbody>();
        HitboxMover mover = hitbox.GetComponent<HitboxMover>();
        if (mover != null) {
            mover.Initialize(StartPoint, EndPoint, DamageConfig, distance, damageMultiplier);
        }

        while (remainingDistance > 0) {
            float t = Mathf.Clamp01(1 - (remainingDistance / distance));
            Vector3 currentPos = Vector3.Lerp(StartPoint, EndPoint, t);

            instance.transform.position = currentPos;

            // Move the hitbox using physics-aware movement
            hitboxRb.MovePosition(currentPos);

            remainingDistance -= TrailConfig.SimulationSpeed * Time.deltaTime;

            yield return new WaitForFixedUpdate();
        }

        instance.transform.position = EndPoint;
        hitboxRb.MovePosition(EndPoint);

        if (Hit.collider != null)
        {
        //     SurfaceManager.Instance.HandleImpact(
        //         Hit.transform.gameObject,
        //         EndPoint,
        //         Hit.normal,
        //         ImpactType,
        //         0
        //     );

            // if (Hit.collider.CompareTag("Player"))
            // {
            //     PlayerActions player = Hit.collider.GetComponent<PlayerActions>();
            //     if (player != null)
            //     {
            //         player.TakeDamage(DamageConfig.GetDamage(distance));
            //     }
            // }
            // else if (Hit.collider.TryGetComponent<IDamageable>(out IDamageable damageable)) {
            //     damageable.TakeDamage(DamageConfig.GetDamage(distance));
            // }

        }
        // Let physics settle for one more frame
        yield return new WaitForFixedUpdate();

        yield return new WaitForSeconds(TrailConfig.Duration);
        yield return null;
        instance.emitting = false;
        instance.gameObject.SetActive(false);
        TrailPool.Release(instance);
    }

    private TrailRenderer CreateTrail()
    {
        GameObject instance = new GameObject("Bullet Trail");
        TrailRenderer trail = instance.AddComponent<TrailRenderer>();
        trail.colorGradient = TrailConfig.Color;
        trail.material = TrailConfig.Material;
        trail.widthCurve = TrailConfig.WidthCurve;
        trail.time = TrailConfig.Duration;
        trail.minVertexDistance = TrailConfig.MinVertexDistance;

        trail.emitting = false;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        return trail;
    }
}