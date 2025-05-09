using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerActions : MonoBehaviour
{
    public static PlayerActions Instance;
    private Vector3 Velocity;
    private Vector3 PlayerMovementInput;
    private Vector2 PlayerMouseInput;
    private bool Sneaking = false;
    private float xRotation;
    private bool isDead = false;

    [Header("Components Needed")]
    public Transform PlayerCamera;
    public CharacterController Controller;
    public Transform Player;
    public Transform LeftArm;
    public Transform RightArm;
    [Space]
    [Header("Status")]
    [SerializeField] PlayerHealthBar healthBar;
    public float curHealth, maxHealth = 100;

    public NoClipEffect noClipEffect;
    [SerializeField] private NoClipEnergyBar energyBar;
    private bool isNoClipActive = false;
    public float NoClipEnergy, maxEnergy = 10;     //1 energy per second
    public float NoClipRegen = 0.2f;
    
    [Space]
    [Header("Movement")]
    public float Speed = 30;
    public float JumpForce;
    public float Sensetivity;
    public float Gravity;
    [Space]
    [Header("Sneaking")]
    public bool Sneak = false;
    public float SneakSpeed;

    void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (SceneManager.GetActiveScene().buildIndex == 0) {
            Destroy(gameObject);
            return;
        }
        healthBar = FindAnyObjectByType<PlayerHealthBar>();
        healthBar.UpdateHealthBar(curHealth, maxHealth);
        energyBar = FindAnyObjectByType<NoClipEnergyBar>();
        NoClipEnergy = maxEnergy;
        energyBar.UpdateEnergyBar(NoClipEnergy, maxEnergy);

    }

    private void Awake() {
        healthBar = FindAnyObjectByType<PlayerHealthBar>();
        energyBar = FindAnyObjectByType<NoClipEnergyBar>();

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // Destroy the duplicate
            return;
        }
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; 
        curHealth = maxHealth;
        healthBar.UpdateHealthBar(curHealth, maxHealth);
        NoClipEnergy = maxEnergy;
        energyBar.UpdateEnergyBar(NoClipEnergy, maxEnergy);
    }

    // Update is called once per frame
    void Update()
    {
        if (MainMenu.IsPaused) return;  //Check if paused
        if (isDead) return;
        //NoClip Behaviors
        if (Input.GetKeyDown(KeyCode.Tab)) {
            Debug.Log("Tab pressed");
            Debug.Log($"isNoClipActive: {isNoClipActive}, NoClipEnergy: {NoClipEnergy}");

            if (!isNoClipActive && NoClipEnergy > 0) {
                Debug.Log("NoClip Activated");
                ActivateNoClip();
            }
            else if (isNoClipActive) {
                Debug.Log("NoClip Deactivated");
                DeactivateNoClip();
            }
        }
        if (isNoClipActive) {
            NoClipEnergy -= Time.deltaTime;
            energyBar.UpdateEnergyBar(NoClipEnergy, maxEnergy);
            if (NoClipEnergy <= 0f) {
                NoClipEnergy = 0f;
                DeactivateNoClip();
            }
        } 
        else if (NoClipEnergy < maxEnergy) {
            NoClipEnergy += NoClipRegen * Time.deltaTime;
            NoClipEnergy = Mathf.Min(NoClipEnergy, maxEnergy);  //Ensures it doesn't go over max
            energyBar.UpdateEnergyBar(NoClipEnergy, maxEnergy);

        }

        //Movement Behaviors
        if (transform.position.y != 10) {
            Vector3 newPosition = transform.position;
            newPosition.y = 10; // Force y to be 10
            transform.position = newPosition;
        }
        PlayerMovementInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        PlayerMouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        MovePlayer();
        MoveCamera();

        if (Input.GetKey(KeyCode.RightShift) && Sneak)
        {
            Player.localScale = new Vector3(1f, 0.5f, 1f);
            Sneaking = true;
        }
        if (Input.GetKeyUp(KeyCode.RightShift))
        {
            Player.localScale = new Vector3(1f, 1f, 1f);
            Sneaking = false;
        }
    }
    private void MovePlayer()
    {
        Vector3 MoveVector = transform.TransformDirection(PlayerMovementInput);

        if (Controller.isGrounded) {
            //Velocity.y = -1f;

            if (Input.GetKeyDown(KeyCode.Space) && Sneaking == false) {
                Velocity.y = JumpForce;
            }
        }
        else {
            //Velocity.y += Gravity * -2f * Time.deltaTime;
        }
        if (isNoClipActive) {
            transform.position += (MoveVector * Speed + Velocity) * Time.deltaTime;
        }
        else if (Sneaking) {
            Controller.Move(MoveVector * SneakSpeed * Time.deltaTime);
        }
        else {
            Controller.Move(MoveVector * Speed * Time.deltaTime);
        }

        if (!isNoClipActive)
        {
            Controller.Move(Velocity * Time.deltaTime);
        }

    }
    private void MoveCamera()
    {
        xRotation -= PlayerMouseInput.y * Sensetivity;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.Rotate(0f, PlayerMouseInput.x * Sensetivity, 0f);     //Horizontal rotation
        PlayerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void ActivateNoClip()
    {
        isNoClipActive = true;
        noClipEffect.Activate(); // You’ll add this method in NoClipEffect.cs
    }

    private void DeactivateNoClip()
    {
        isNoClipActive = false;
        noClipEffect.Deactivate(); // You’ll add this method too
    }

    public void TakeDamage(int damage) {
        curHealth -= damage;
        healthBar.UpdateHealthBar(curHealth, maxHealth);
        Debug.Log($"{gameObject.name} took {damage} damage. Current Health: {curHealth}/{maxHealth}");
        if (curHealth <= 0) {
            if (!isDead) {
                isDead = true;
                GetComponent<PlayerDeath>().Death();
            }
        }
    }


    public void ApplyUpgrade(UpgradeData upgrade) {
        switch (upgrade.upgradeID)
        {
            case "health":
                UpgradeHealth(upgrade);
                break;
            case "ammo":
                UpgradeAmmo(upgrade);
                break;
            case "damage":
                UpgradeDamage(upgrade);
                break;
            case "noclipdamage":
                UpgradeNoClipDamage(upgrade);
                break;
            case "noclipenergy":
                UpgradeNoClipEnergy(upgrade);
                break;
            case "noclipregen":
                UpgradeNoClipRegen(upgrade);
                break;
            case "pellet":
                UpgradePellet(upgrade);
                break;
            case "range":
                UpgradeRange(upgrade);
                break;
            case "speed":
                UpgradeSpeed(upgrade);
                break;
            case "spread":
                UpgradeSpread(upgrade);
                break;
            default:
                Debug.LogWarning($"Unknown upgradeID: {upgrade.upgradeID}");
                break;
        }
    }

    //Gun Upgrades
    private void UpgradeSpread(UpgradeData upgrade)
    {
        GunSystem gunSystem = FindAnyObjectByType<GunSystem>();
        if (gunSystem != null)
            gunSystem.UpgradeSpread(upgrade);    
    }
    private void UpgradeRange(UpgradeData upgrade)
    {
        GunSystem gunSystem = FindAnyObjectByType<GunSystem>();
        if (gunSystem != null)
            gunSystem.UpgradeRange(upgrade);    
    }
    private void UpgradePellet(UpgradeData upgrade)
    {
        GunSystem gunSystem = FindAnyObjectByType<GunSystem>();
        if (gunSystem != null)
            gunSystem.UpgradePellet(upgrade);    
    }
     private void UpgradeDamage(UpgradeData upgrade)
    {
        GunSystem gunSystem = FindAnyObjectByType<GunSystem>();
        if (gunSystem != null)
            gunSystem.UpgradeDamage(upgrade);    
    }
    private void UpgradeAmmo(UpgradeData upgrade)
    {
        GunSystem gunSystem = FindAnyObjectByType<GunSystem>();
        if (gunSystem != null)
            gunSystem.UpgradeAmmo(upgrade);
    }

    //NoClip Upgrades
    private void UpgradeNoClipRegen(UpgradeData upgrade)
    {
        NoClipRegen *= 1 + 0.3f * upgrade.tier;
        Debug.Log($"Applied Energy Regen Upgrade. New Regen Rate: {NoClipRegen}");
    }
    private void UpgradeNoClipEnergy(UpgradeData upgrade)
    {
        maxEnergy *= 1 + 0.1f * upgrade.tier;    //Each upgrade increases current energy by 10%
        Debug.Log($"Applied Max Energy Upgrade. New Max Energy: {maxEnergy}");

    }
    private void UpgradeNoClipDamage(UpgradeData upgrade)
    {
        NoClipSword Sword = FindAnyObjectByType<NoClipSword>();
        if (Sword != null)
            Sword.UpgradeDamage(upgrade);
    }

    //Stat Upgrades
    private void UpgradeSpeed(UpgradeData upgrade)
    {
        Speed *= 1 + 0.1f * upgrade.tier;    //Each upgrade increases current energy by 10%
        Debug.Log($"Applied Speed Upgrade. New Speed: {Speed}");
    }
    private void UpgradeHealth(UpgradeData upgrade) {
        float bonusHealth = 10f * upgrade.tier;
        maxHealth += bonusHealth;
        curHealth += bonusHealth; // Optional: heal when max health increases
        healthBar.UpdateHealthBar(curHealth, maxHealth);
        Debug.Log($"Applied Health Upgrade. New Max Health: {maxHealth}");
    }
}