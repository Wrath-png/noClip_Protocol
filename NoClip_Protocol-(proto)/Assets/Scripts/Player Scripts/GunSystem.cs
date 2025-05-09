using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GunSystem : MonoBehaviour
{
    //Gun stats
    public int damage = 8;
    public float timeBetweenShooting, reloadTime, bulletSpeed;  //Bullet Speed default 200
    public float spread = 0.6f;
    public float range = 25;
    public int magSize = 2;
    public int pelletCount = 12;
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
    public AudioClip reloadSound;
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
                
                //Will deal damage to anying with Idamageable attached to it
                IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                if (damageable != null) 
                    damageable.TakeDamage(damage);
            }
            else {
                //Will show Trails even if nothing is hit.
                TrailRenderer trail = Instantiate(BulletTrail, LeftMuzzle.position, Quaternion.identity);
                StartCoroutine(SpawnTrail(trail, LeftMuzzle.position + direction * range, Vector3.zero, false));
            }
        }
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
                
                //Will deal damage to anying with Idamageable attached to it
                IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                if (damageable != null) 
                    damageable.TakeDamage(damage);
            }
            else {
                //Will show Trails even if nothing is hit.
                TrailRenderer trail = Instantiate(BulletTrail, RightMuzzle.position, Quaternion.identity);
                StartCoroutine(SpawnTrail(trail, RightMuzzle.position + direction * range, Vector3.zero, false));
            }
        }
        Invoke("ResetShot", timeBetweenShooting);
    }
    private void ResetShot() {
        readyToShoot = true;
        pelletsShot = 0;
    }
    private void Reload() {
        reloading = true;
        rightAudio.PlayOneShot(reloadSound);
        leftAudio.PlayOneShot(reloadSound);
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

    void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        textLeft = GameObject.Find("Ammo Left").GetComponent<TextMeshProUGUI>();
        textRight = GameObject.Find("Ammo Right").GetComponent<TextMeshProUGUI>();
        ReloadFinished();
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

    public void UpgradeSpread(UpgradeData upgrade)
    {
        spread -= 0.1f;
        Debug.Log($"Applied Spread Upgrade. New Gun Spread: {spread}");

    }
    public void UpgradeRange(UpgradeData upgrade)
    {
        range *= 1 + 0.1f * upgrade.tier;    //Each upgrade increases current range by 10%
        Debug.Log($"Applied Range Upgrade. New Gun Range: {range}");
    }
    public void UpgradePellet(UpgradeData upgrade)
    {
        pelletCount += 2;
        Debug.Log($"Applied Pellet Count Upgrade. New Pellet Count: {pelletCount}");
    }
    public void UpgradeDamage(UpgradeData upgrade)
    {
        damage = Mathf.CeilToInt(damage * (1 + 0.1f * upgrade.tier));   //Each upgrade increases current damage by 10%
        Debug.Log($"Applied Damage Upgrade. New Damage: {damage}");
    }
    public void UpgradeAmmo(UpgradeData upgrade)
    {
        magSize += 1;    //Each upgrade increases magSize by 1
        Debug.Log($"Applied Ammo Upgrade. New Mag Size: {magSize}");
    }
}
