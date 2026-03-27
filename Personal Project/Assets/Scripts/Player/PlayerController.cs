using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Character Selection")]
    [SerializeField] private PlayerStatData currentCharacterData;

    [Header("UI Prefabs")]
    [SerializeField] private HealthBarUI healthBarPrefab;
    private HealthBarUI currentHealthBar;
    
    private float currentHealth;
    private bool isDead = false;

    private float currentMoveSpeed;
    private Vector2 moveInput;
    private Rigidbody2D rb;
    private GameObject currentVisual;
    private bool isFacingRight = true;
    
    public Vector2 CurrentLookDirection => isFacingRight ? Vector2.right : Vector2.left;
    private InputSystem_Actions controls;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; 
        
        controls = new InputSystem_Actions();

        // 데이터 있으면 초기화
        if (currentCharacterData != null)
        {
            InitializeCharacter(currentCharacterData);
        }
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.Player.Move.performed += OnMovePerformed;
        controls.Player.Move.canceled += OnMoveCanceled;
    }

    private void OnDisable()
    {
        if (controls != null)
        {
            controls.Player.Move.performed -= OnMovePerformed;
            controls.Player.Move.canceled -= OnMoveCanceled;
            controls.Disable();
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (isDead) return;
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    public void InitializeCharacter(PlayerStatData data)
    {
        if (data == null) return;
        currentCharacterData = data;
        currentMoveSpeed = data.moveSpeed;
        currentHealth = data.maxHealth;
        isDead = false;
        
        if (healthBarPrefab != null && currentHealthBar == null)
        {
            currentHealthBar = Instantiate(healthBarPrefab);
        }

        if (currentHealthBar != null)
        {
            currentHealthBar.gameObject.SetActive(true);
            currentHealthBar.Initialize(this.transform, data.maxHealth);
        }
        
        if (currentVisual != null) Destroy(currentVisual);
        if (data.characterPrefab != null)
        {
            currentVisual = Instantiate(data.characterPrefab, transform);
            currentVisual.transform.localPosition = Vector2.zero;
            
            Weapon weapon = currentVisual.GetComponentInChildren<Weapon>();
            if (weapon != null)
            {
                weapon.Setup(data, this);
            }
        }
        // 자석 범위 
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.magnetRange = data.magnetRange;
            Debug.Log($"[Sync Success] PlayerStats MagnetRange: {stats.magnetRange}");
        }
        else
        {
            Debug.LogError("Player 오브젝트에 PlayerStats 컴포넌트가 없습니다!");
        }
    }

    void FixedUpdate()
    {
        if (isDead) 
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        rb.linearVelocity = moveInput * currentMoveSpeed;
        if (moveInput.x != 0) Flip();
    }

    private void Flip()
    {
        if ((moveInput.x > 0 && !isFacingRight) || (moveInput.x < 0 && isFacingRight))
        {
            isFacingRight = !isFacingRight;
            if (currentVisual != null)
            {
                Vector3 scale = currentVisual.transform.localScale;
                scale.x = isFacingRight ? 1f : -1f;
                currentVisual.transform.localScale = scale;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        if (currentHealthBar != null) currentHealthBar.UpdateHealth(currentHealth);
        if (currentHealth <= 0) { currentHealth = 0; Die(); }
    }

    private void Die()
    {
        isDead = true;
        moveInput = Vector2.zero;
        if (currentHealthBar != null) currentHealthBar.gameObject.SetActive(false);
        Debug.Log("플레이어 사망!");
    }
}