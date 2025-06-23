using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Configuração de Movimento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] public float dashCooldown = 1f;
    [SerializeField] private Transform slimeTransform;
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
    private Vector3 originalScale;

    private Rigidbody rb;
    private Vector3 inputVector;
    private Health health;

    // Armazena a velocidade base para ser usada como referência no multiplicador
    private float baseMoveSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        playerCollider = GetComponent<BoxCollider>();
        health = GetComponent<Health>();

        originalColliderSize = playerCollider.size;
        originalColliderCenter = playerCollider.center;
        originalScale = transform.localScale;

        // Armazena a velocidade original definida no Inspector
        baseMoveSpeed = moveSpeed;
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
    }
    private void RotateTowardsCursor()
    {
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        float enter;
        if (groundPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 lookDir = hitPoint - transform.position;
            lookDir.y = 0f;

            if (lookDir.sqrMagnitude > 0.01f && slimeTransform != null)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                // Mantém o -90 em X, aplica só o Y do targetRot
                Vector3 euler = targetRot.eulerAngles;
                slimeTransform.rotation = Quaternion.Euler(-90f, euler.y, 0f);
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
        isLiquidified = true;
        liquidCooldownTimer = liquidCooldown;

        moveSpeed = 3f;
        // Visual e hitbox achatados (pool)
        playerCollider.size = new Vector3(originalColliderSize.x * 2f, 0.1f, originalColliderSize.z * 2f);
        playerCollider.center = new Vector3(originalColliderCenter.x, 0.05f, originalColliderCenter.z);
        transform.localScale = new Vector3(originalScale.x * 2f, 0.1f, originalScale.z * 2f);

        yield return new WaitForSeconds(liquidDuration);

        // Restaurar visual e hitbox
        playerCollider.size = originalColliderSize;
        playerCollider.center = originalColliderCenter;
        transform.localScale = originalScale;

        // Garante altura padrão
        Vector3 currentPos = transform.position;
        transform.position = new Vector3(currentPos.x, 1f, currentPos.z);

        moveSpeed = baseMoveSpeed; // Velocidade normal restaurada
        isLiquidified = false;
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
