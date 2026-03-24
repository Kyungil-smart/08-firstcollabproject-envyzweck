using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Character Selection")]
    [SerializeField] private PlayerStatData currentCharacterData;

    private float currentMoveSpeed;
    private Vector2 moveInput;
    private Rigidbody2D rb;
    private GameObject currentVisual;
    private bool isFacingRight = true;
    
    // 바라보는 방향을 vector값으로 변환
    public Vector2 CurrentLookDirection => isFacingRight ? Vector2.right : Vector2.left;
    
    private InputSystem_Actions controls;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; 
        
        controls = new InputSystem_Actions();

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
        controls.Player.Move.performed -= OnMovePerformed;
        controls.Player.Move.canceled -= OnMoveCanceled;
        controls.Disable();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
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

        if (currentVisual != null) Destroy(currentVisual);
        if (data.characterPrefab != null)
        {
            currentVisual = Instantiate(data.characterPrefab, transform);
            currentVisual.transform.localPosition = Vector2.zero;
            
            Weapon weapon = currentVisual.GetComponentInChildren<Weapon>();
            
            if (weapon != null)
            {
                // 적이 주변에 없을때 this로 방향파악
                weapon.Setup(data, this);
                Debug.Log($"{data.characterName} 캐릭터의 무기 세팅이 완료되었습니다.");
            }
            else
            {
                Debug.LogWarning($"{data.characterName} 프리팹에 Weapon 컴포넌트가 없습니다!");
            }
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * currentMoveSpeed;
        
        // 이동 입력이 있을 때만 Flip 체크
        if (moveInput.x != 0)
        {
            Flip();
        }
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
}