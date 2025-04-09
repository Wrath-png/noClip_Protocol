using UnityEngine;

public class PlayerActions : MonoBehaviour
{
    private Vector3 Velocity;
    private Vector3 PlayerMovementInput;
    private Vector2 PlayerMouseInput;
    private bool Sneaking = false;
    private float xRotation;

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
    public float NoClipenergy = 10;     //1 energy per second
    
    [Space]
    [Header("Movement")]
    public float Speed;
    public float JumpForce;
    public float Sensetivity;
    public float Gravity;
    [Space]
    [Header("Sneaking")]
    public bool Sneak = false;
    public float SneakSpeed;

    private void Awake() {
        healthBar = GetComponentInChildren<PlayerHealthBar>(true);   
        //(true) will search all nested children, even inactive objects.
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; 
        curHealth = maxHealth;
        healthBar.UpdateHealthBar(curHealth, maxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        if (MainMenu.IsPaused) return;  //Check if paused

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

        if (Sneaking) {
            Controller.Move(MoveVector * SneakSpeed * Time.deltaTime);
        }
        else {
            Controller.Move(MoveVector * Speed * Time.deltaTime);
        }
        Controller.Move(Velocity * Time.deltaTime);

    }
    private void MoveCamera()
    {
        xRotation -= PlayerMouseInput.y * Sensetivity;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.Rotate(0f, PlayerMouseInput.x * Sensetivity, 0f);     //Horizontal rotation
        PlayerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    public void TakeDamage(int damage) {
        curHealth -= damage;
        healthBar.UpdateHealthBar(curHealth, maxHealth);
        Debug.Log($"{gameObject.name} took {damage} damage. Current Health: {curHealth}/{maxHealth}");
    }
}