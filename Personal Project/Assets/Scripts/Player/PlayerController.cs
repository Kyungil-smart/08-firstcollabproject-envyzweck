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
    
    private InputSystem_Actions controls;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        // 벽 뚫기 방지
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; 
        
        controls = new InputSystem_Actions();

        if (currentCharacterData != null)
        {
            InitializeCharacter(currentCharacterData);
        }
    }

    // 입력 시스템 활성화/비활성화 (메모리 관리 및 최적화)
    private void OnEnable()
    {
        controls.Enable();
        // C# 이벤트 연결 _ Move 액션이 일어날 때 불러오기
        controls.Player.Move.performed += OnMovePerformed;
        controls.Player.Move.canceled += OnMoveCanceled;
    }

    private void OnDisable()
    {
        // 이벤트 해제 (메모리 누수 방지)
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
            
            // 스탯을 넘겨주고 초기화
            if (weapon != null)
            {
                weapon.Setup(data);
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
        // 물리 연산 최적화: linearVelocity 직접 대입
        rb.linearVelocity = moveInput * currentMoveSpeed;
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
                scale.x *= -1;
                currentVisual.transform.localScale = scale;
            }
        }
    }
}