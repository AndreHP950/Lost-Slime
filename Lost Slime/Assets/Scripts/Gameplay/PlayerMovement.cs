using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Configuração de Movimento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] public float dashCooldown = 1f;
    [Tooltip("Referência ao objeto visual do Slime (SlimeArmature)")]
    [SerializeField] private Transform slimeArmatureTransform; // arraste o SlimeArmature aqui
    public bool isDashing = false;
    public float dashCooldownTimer = 0f;
    private int dashCount = 3;

    [Header("Configuração de Liqueficação")]
    [SerializeField] private float liquidDuration = 2f;
    [SerializeField] public float liquidCooldown = 5f;

    public float liquidCooldownTimer = 0f;
    private bool isLiquidified = false;
    private BoxCollider playerCollider;
    private Vector3 originalColliderSize;
    private Vector3 originalColliderCenter;

    private Rigidbody rb;
    private Vector3 inputVector;
    private Health health;

    // Armazena a velocidade base para ser usada como referência no multiplicador
    private float baseMoveSpeed;

    // Armazena a altura (Y) original do jogador e o nível do chão
    private float originalY;
    private float originalGroundY;

    // Referência ao Animator do Slime
    private Animator slimeAnimator;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        playerCollider = GetComponent<BoxCollider>();
        health = GetComponent<Health>();

        originalColliderSize = playerCollider.size;
        originalColliderCenter = playerCollider.center;
        originalY = transform.position.y; // Pivô original do jogador
        originalGroundY = originalY - (transform.localScale.y * 0.5f); // Supõe que o pivô está centralizado

        // Armazena a velocidade original definida no Inspector
        baseMoveSpeed = moveSpeed;

        // Busca o Animator no SlimeArmature
        if (slimeArmatureTransform != null)
            slimeAnimator = slimeArmatureTransform.GetComponent<Animator>();
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        inputVector = new Vector3(h, 0f, v).normalized;

        RotateTowardsCursor();

        if (Input.GetKeyDown(KeyCode.Space) && dashCount > 0 && dashCooldownTimer <= 0f)
        {
            Dash();
        }

        if (Input.GetKeyDown(KeyCode.Q) && !isLiquidified && liquidCooldownTimer <= 0f)
        {
            StartCoroutine(Liquidify());
        }

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        if (liquidCooldownTimer > 0f)
            liquidCooldownTimer -= Time.deltaTime;

        // --- Controle de animações ---
        if (slimeAnimator != null)
        {
            // Idle: true se não está se movendo, não está liquidificado e não está dashando
            bool isIdle = inputVector.sqrMagnitude < 0.01f && !isLiquidified && !isDashing && !slimeAnimator.GetBool("IsLiquify");
            slimeAnimator.SetBool("IsIdle", isIdle);

            // Liquify: true se está liquidificado
            slimeAnimator.SetBool("IsLiquify", isLiquidified);
        }
    }

    private void RotateTowardsCursor()
    {
        // Gira o SlimeArmature (visual) para o mouse, mas mantém o corpo do player estático
        if (slimeArmatureTransform == null) return;

        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        float enter;
        if (groundPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 lookDir = hitPoint - slimeArmatureTransform.position;
            lookDir.y = 0f;

            if (lookDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                slimeArmatureTransform.rotation = targetRot;
            }
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + inputVector * moveSpeed * Time.fixedDeltaTime);
    }

    void Dash()
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;
        dashCount--;

        if (health != null)
            health.isImmune = true;

        Vector3 dashVector = inputVector * dashSpeed;
        rb.linearVelocity = new Vector3(dashVector.x, rb.linearVelocity.y, dashVector.z);

        Debug.Log("Dash realizado! Restam " + dashCount + " dashes.");

        StartCoroutine(EndDash());
    }

    private IEnumerator EndDash()
    {
        yield return new WaitForSeconds(0.5f); // duração do dash

        isDashing = false;
        if (health != null)
            health.isImmune = false;

        StartCoroutine(RechargeDash());
    }

    private IEnumerator RechargeDash()
    {
        yield return new WaitForSeconds(5.5f); // tempo para recarregar dash
        dashCount++;
        Debug.Log("Dash recarregado! Dashes disponíveis: " + dashCount);
    }

    private IEnumerator Liquidify()
    {
        slimeAnimator.SetBool("IsIdle", false);
        if (slimeAnimator != null)
            slimeAnimator.SetBool("IsLiquify", true);

        isLiquidified = true;
        liquidCooldownTimer = liquidCooldown;

        moveSpeed = 3f;

        // Novo tamanho achatado
        Vector3 newSize = new Vector3(originalColliderSize.x * 2f, 0.05f, originalColliderSize.z * 2f);

        // Novo centro: base do colisor encostando no chão
        float newCenterY = (newSize.y / 2f) - (originalColliderSize.y / 2f) + originalColliderCenter.y;

        playerCollider.size = newSize;
        playerCollider.center = new Vector3(originalColliderCenter.x, newCenterY, originalColliderCenter.z);

        yield return new WaitForSeconds(liquidDuration);

        // Restaurar colisor
        playerCollider.size = originalColliderSize;
        playerCollider.center = originalColliderCenter;

        moveSpeed = baseMoveSpeed;
        isLiquidified = false;

        if (slimeAnimator != null)
            slimeAnimator.SetBool("IsLiquify", false);

        Debug.Log("Liqueficação terminada.");
    }




    public bool IsLiquidified => isLiquidified;

    /// <summary>
    /// Aplica um multiplicador na velocidade de movimento.
    /// Por exemplo, um multiplicador de 0.3 fará o jogador andar a 30% da velocidade normal.
    /// </summary>
    public void ApplySpeedMultiplier(float multiplier)
    {
        moveSpeed = baseMoveSpeed * multiplier;
    }

    /// <summary>
    /// Remove o multiplicador de velocidade, restaurando a velocidade base.
    /// </summary>
    public void RemoveSpeedMultiplier()
    {
        moveSpeed = baseMoveSpeed;
    }
}
