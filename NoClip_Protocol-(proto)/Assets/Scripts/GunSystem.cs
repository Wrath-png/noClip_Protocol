using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GunSystem : MonoBehaviour
{
    //Gun stats
    public int damage;
    public float timeBetweenShooting, spread, range, reloadTime, bulletSpeed, timeBetweenShots;
    //Bullet Speed default 200
    public int magSize, bulletsPerTap, pelletCount;
    public bool allowButtonHold;
    int bulletsLeft, bulletsRight, pelletsShot, currentGun;
        //All of these are here so that shots switch from left to right

    //booleans
    bool shooting, readyToShoot, reloading;

    //References
    //public Camera FPC;
    public Transform LeftMuzzle;
    public Transform RightMuzzle;
    public AudioSource leftAudio;
    public AudioSource rightAudio;
    public AudioClip shotgunSound;
    //public Transform attackPoint;
    public RaycastHit hit;
    public LayerMask canHit;
    //Graphics
    public GameObject muzzleFlash, bulletHoleGraphic;
    [SerializeField] private TrailRenderer BulletTrail;
    public CameraShake camShake;
    public GunShake gunShake;
    public float camShakeMag, camShakeDur, gunShakeMag, gunShakeDur;
    public TextMeshProUGUI textLeft;
    public TextMeshProUGUI textRight;

    private void MyInput() {
        if(allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);    //Will charge for more powerful shot
        else shooting = Input.GetKeyDown(KeyCode.Mouse0);               //Shoots one shell

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magSize && !reloading) Reload();

        //Shoot
        if (readyToShoot && shooting && !reloading && ((bulletsLeft > 0) || (bulletsRight > 0))) {
            currentGun = (currentGun + 1) % 2;
            if (currentGun == 0) {      //0 = left gun
                Quaternion muzzleRotation = LeftMuzzle.rotation;
                muzzleRotation = Quaternion.Euler(muzzleRotation.eulerAngles.x, muzzleRotation.eulerAngles.y - 90f, muzzleRotation.eulerAngles.z);
                GameObject flash = Instantiate(muzzleFlash, LeftMuzzle.position, muzzleRotation, LeftMuzzle);
                ShootLeft();
                Destroy(flash, 0.1f);
                //pelletsShot = 0;
                bulletsLeft--;
                //Shake Camera for more omph
                StartCoroutine(camShake.Shake(camShakeDur, camShakeMag));
                //StartCoroutine(LeftMuzzle.GetComponent<GunShake>().Shake(gunShakeDur, gunShakeMag));      //Does not work yet
            }
            if (currentGun == 1) {      //1 = right gun
                Quaternion muzzleRotation = RightMuzzle.rotation;
                muzzleRotation = Quaternion.Euler(muzzleRotation.eulerAngles.x, muzzleRotation.eulerAngles.y - 90f, muzzleRotation.eulerAngles.z);
                GameObject flash = Instantiate(muzzleFlash, RightMuzzle.position, muzzleRotation, RightMuzzle);
                ShootRight();
                Destroy(flash, 0.1f);
                pelletsShot = 0;
                bulletsRight--;
                //Cam Shake
                StartCoroutine(camShake.Shake(camShakeDur, camShakeMag));
                //StartCoroutine(RightMuzzle.GetComponent<GunShake>().Shake(gunShakeDur, gunShakeMag));      //Does not work yet
            }
        }
    }
    private void ShootLeft() {
        readyToShoot = false;
        leftAudio.PlayOneShot(shotgunSound);
        for (pelletsShot = 0; pelletsShot < pelletCount; pelletsShot++) {
            //Debug.Log("In Shoot Left");

            //Shot Spread
            float x = Random.Range(-spread, spread);
            float y = Random.Range(-spread, spread); 

            //Calculate Direction with Spread
            Vector3 direction = (-LeftMuzzle.up + LeftMuzzle.TransformDirection(new Vector3(x, 0, y))).normalized;
            if (Physics.Raycast(LeftMuzzle.position, direction, out hit, range, canHit)) {
                
                TrailRenderer trail = Instantiate(BulletTrail, LeftMuzzle.position, Quaternion.identity);
                StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, true));
                
                Debug.DrawRay(LeftMuzzle.position, direction * range, Color.red, 2f);
                

                if (hit.collider.CompareTag("Enemy")) {
                    SmallEnemyAI enemyAI = hit.collider.GetComponent<SmallEnemyAI>();
                    if (enemyAI != null) {
                        enemyAI.TakeDamage(damage);
                    }
                }
                if (hit.collider.CompareTag("Practice Dummy")) { 
                    PracticeDummy dummy = hit.collider.GetComponent<PracticeDummy>();
                    if (dummy != null) {
                        dummy.TakeDamage(damage);
                    }
                }
                // if (hit.normal != Vector3.zero) { // Ensure the normal is valid
                //     //Instantiate(rockParticle, hit.point, Quaternion.LookRotation(hit.normal));
                //     Instantiate(bulletHoleGraphic, hit.point, Quaternion.LookRotation(hit.normal));
                // }
            }
            else {
                //Will show Trails even if nothing is hit.
                TrailRenderer trail = Instantiate(BulletTrail, LeftMuzzle.position, Quaternion.identity);
                StartCoroutine(SpawnTrail(trail, LeftMuzzle.position + direction * range, Vector3.zero, false));
            }
        }

        // if(pelletsShot < pelletCount) {
        //     Invoke("ShootLeft", timeBetweenShots);
        // }
        Invoke("ResetShot", timeBetweenShooting);
    }
    private void ShootRight() {
        readyToShoot = false;
        rightAudio.PlayOneShot(shotgunSound);
        for (pelletsShot = 0; pelletsShot < pelletCount; pelletsShot++) {
            //Debug.Log("In Shoot Left");

            //Shot Spread
            float x = Random.Range(-spread, spread);
            float y = Random.Range(-spread, spread); 

            //Calculate Direction with Spread
            Vector3 direction = (-RightMuzzle.up + RightMuzzle.TransformDirection(new Vector3(x, 0, y))).normalized;
            if (Physics.Raycast(RightMuzzle.position, direction, out hit, range, canHit)) {
                
                TrailRenderer trail = Instantiate(BulletTrail, RightMuzzle.position, Quaternion.identity);
                StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, true));
                
                Debug.DrawRay(RightMuzzle.position, direction * range, Color.red, 2f);
                

                if (hit.collider.CompareTag("Enemy")) {
                    SmallEnemyAI enemyAI = hit.collider.GetComponent<SmallEnemyAI>();
                    if (enemyAI != null) {
                        enemyAI.TakeDamage(damage);
                    }
                }
                if (hit.collider.CompareTag("Practice Dummy")) { 
                    PracticeDummy dummy = hit.collider.GetComponent<PracticeDummy>();
                    if (dummy != null) {
                        dummy.TakeDamage(damage);
                    }
                }
                // if (hit.normal != Vector3.zero) { // Ensure the normal is valid
                //     //Instantiate(rockParticle, hit.point, Quaternion.LookRotation(hit.normal));
                //     Instantiate(bulletHoleGraphic, hit.point, Quaternion.LookRotation(hit.normal));
                // }
            }
            else {
                //Will show Trails even if nothing is hit.
                TrailRenderer trail = Instantiate(BulletTrail, RightMuzzle.position, Quaternion.identity);
                StartCoroutine(SpawnTrail(trail, RightMuzzle.position + direction * range, Vector3.zero, false));
            }
        }

        // if(pelletsShot < pelletCount) {
        //     Invoke("ShootLeft", timeBetweenShots);
        // }
        Invoke("ResetShot", timeBetweenShooting);
    }
    private void ResetShot() {
        readyToShoot = true;
        pelletsShot = 0;
    }
    private void Reload() {
        reloading = true;
        Invoke("ReloadFinished", reloadTime);
    }
    private void ReloadFinished() {
        bulletsLeft = magSize;
        bulletsRight = magSize;
        reloading = false;
    }

    private IEnumerator SpawnTrail(TrailRenderer Trail, Vector3 HitPoint, Vector3 HitNormal, bool MadeImpact) {
        //Debug.Log("In Spawn Trail");
        Vector3 startPosition = Trail.transform.position;
        float distance = Vector3.Distance(Trail.transform.position, HitPoint);
        float remainingDistance = distance;

        if (distance == 0) yield break;

        while (remainingDistance > 0) {
            Trail.transform.position = Vector3.Lerp(startPosition, HitPoint, 1 - (remainingDistance / distance));

            remainingDistance -= bulletSpeed * Time.deltaTime;

            yield return null;
        }
        Trail.transform.position = HitPoint;
        if (MadeImpact)
        {
            Instantiate(bulletHoleGraphic, HitPoint, Quaternion.LookRotation(HitNormal));
        }

        Destroy(Trail.gameObject, Trail.time);
    
    }
    void Start()
    {
        bulletsLeft = magSize;
        bulletsRight = magSize;
        currentGun = 1;
        readyToShoot = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (MainMenu.IsPaused) return;  //Check if paused
        MyInput();
        //Set Text
        textLeft.SetText(bulletsLeft + " / " + magSize);
        textRight.SetText(bulletsRight + " / " + magSize);
    }
}
