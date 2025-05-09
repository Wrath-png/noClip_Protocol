using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class GunRuntime : MonoBehaviour
{
    private GunScriptableObject config;
    private ParticleSystem shootSystem;
    private ObjectPool<TrailRenderer> trailPool;
    private MonoBehaviour owner;
    private Transform firePoint;
    private float lastShootTime;

    public void Initialize(GunScriptableObject config, MonoBehaviour owner)
    {
        this.config = config;
        this.owner = owner;
        lastShootTime = 0f;
        trailPool = new ObjectPool<TrailRenderer>(CreateTrail);

        shootSystem = GetComponentInChildren<ParticleSystem>();
        firePoint = shootSystem != null ? shootSystem.transform : null;
    }

    public void Shoot(float damageMultiplier)
    {
        if (Time.time > config.ShootConfig.FireRate + lastShootTime) {
            lastShootTime = Time.time;

            if (shootSystem != null) shootSystem.Play();

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Vector3 shootDirection = firePoint.forward; 


            if (player!= null) {
                Vector3 targetPoint = player.transform.position;
                Collider playerCollider = player.GetComponent<Collider>();
                if (playerCollider != null)
                    targetPoint = playerCollider.bounds.center;

                shootDirection = (targetPoint - firePoint.position).normalized;
            }
                
            shootDirection += new Vector3(
                Random.Range(
                    -config.ShootConfig.Spread.x,
                    config.ShootConfig.Spread.x
                ),
                Random.Range(
                    -config.ShootConfig.Spread.y,
                    config.ShootConfig.Spread.y
                ),
                Random.Range(
                    -config.ShootConfig.Spread.z,
                    config.ShootConfig.Spread.z
                )
            );
            shootDirection.Normalize();

            if (config.TrailConfig.UseLaserHitbox)
            {
                GameObject hitbox = Instantiate(config.TrailConfig.HitboxPrefab, firePoint.position, Quaternion.LookRotation(shootDirection));
                var mover = hitbox.GetComponent<HitboxMover>();
                mover.SetOwner(owner.gameObject);

                
                if (mover != null)
                {
                    mover.Initialize(
                        firePoint.position,
                        shootDirection,
                        config.DamageConfig,
                        config.TrailConfig.shotSpeed,
                        damageMultiplier
                    );
                }

                return; // Exit early — no need for trail coroutine
            }

            if (Physics.Raycast(
                    firePoint.position,
                    shootDirection,
                    out RaycastHit hit,
                    float.MaxValue,
                    config.ShootConfig.HitMask
                ))
            {
                //Debug.DrawRay(ShootSystem.transform.position, shootDirection * 100f, Color.red, 1f);

                owner.StartCoroutine(
                    PlayTrail(
                        firePoint.position,
                        hit.point,
                        hit,
                        damageMultiplier
                    )
                );
                
            }
            else {
                owner.StartCoroutine(
                    PlayTrail(
                        firePoint.position,
                        firePoint.position + (shootDirection * config.TrailConfig.MissDistance),
                        new RaycastHit(),
                        1f
                        
                    )
                );
            }
        }
    }

    private IEnumerator PlayTrail(Vector3 StartPoint, Vector3 EndPoint, RaycastHit Hit, float damageMultiplier)
    {
        TrailRenderer instance = trailPool.Get();
        instance.gameObject.SetActive(true);
        instance.transform.position = StartPoint;
        yield return null;

        instance.emitting = true;
        float distance = Vector3.Distance(StartPoint, EndPoint);
        float remainingDistance = distance;

        GameObject hitbox = Instantiate(config.TrailConfig.HitboxPrefab, StartPoint, Quaternion.identity);
        Rigidbody hitboxRb = hitbox.GetComponent<Rigidbody>();
        HitboxMover mover = hitbox.GetComponent<HitboxMover>();
        if (mover != null) {
            mover.SetOwner(owner.gameObject);
            mover.Initialize(StartPoint, EndPoint, config.DamageConfig, distance, damageMultiplier);
        }

        while (remainingDistance > 0) {
            float t = Mathf.Clamp01(1 - (remainingDistance / distance));
            Vector3 currentPos = Vector3.Lerp(StartPoint, EndPoint, t);

            instance.transform.position = currentPos;

            // Move the hitbox using physics-aware movement
            hitboxRb.MovePosition(currentPos);

            remainingDistance -= config.TrailConfig.SimulationSpeed * Time.deltaTime;

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

        yield return new WaitForSeconds(config.TrailConfig.Duration);
        yield return null;
        instance.emitting = false;
        instance.gameObject.SetActive(false);
        trailPool.Release(instance);
    }

    private TrailRenderer CreateTrail()
    {
        GameObject instance = new GameObject("Bullet Trail");
        TrailRenderer trail = instance.AddComponent<TrailRenderer>();
        trail.colorGradient = config.TrailConfig.Color;
        trail.material = config.TrailConfig.Material;
        trail.widthCurve = config.TrailConfig.WidthCurve;
        trail.time = config.TrailConfig.Duration;
        trail.minVertexDistance = config.TrailConfig.MinVertexDistance;

        trail.emitting = false;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        return trail;
    }

}