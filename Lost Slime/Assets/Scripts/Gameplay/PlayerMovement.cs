using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Configuração de Movimento")]
    [SerializeField] private float moveSpeed = 5f;    // Velocidade de movimento
    [SerializeField] private float dashSpeed = 10f;    // Velocidade do dash
    [SerializeField] private float dashCooldown = 1f;  // Tempo de cooldown para usar o dash
    private float dashCooldownTimer = 0f;              // Controle do tempo de cooldown do dash
    private int dashCount = 3;                         // Número de dashes disponíveis

    [Header("Configuração de Liqueficação")]
    [SerializeField] private float liquidDuration = 2f;  // Duração da liqueficação
    [SerializeField] private float liquidCooldown = 5f;  // Tempo de cooldown para usar a liqueficação
    private float liquidCooldownTimer = 0f;             // Controle do tempo de cooldown da liqueficação
    private bool isLiquidified = false;                 // Flag para verificar se o jogador está liquefeito
    private BoxCollider playerCollider;            // Referência ao BoxCollider para modificação
    private Vector3 originalColliderSize;          // Tamanho original do colisor
    private Vector3 originalColliderCenter;        // Centro original do colisor


    private Rigidbody rb;
    private Vector3 inputVector;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;        // Impede rotação indesejada
        playerCollider = GetComponent<BoxCollider>();  // Referência ao colisor do jogador

        // Armazenando o tamanho e o centro do colisor
        originalColliderSize = playerCollider.size;
        originalColliderCenter = playerCollider.center;

    }

    void Update()
    {
        // Leitura de input para movimento (WASD / setas)
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        inputVector = new Vector3(h, 0f, v).normalized;

        // Verifica se o jogador apertou o botão de dash
        if (Input.GetKeyDown(KeyCode.Space) && dashCount > 0 && dashCooldownTimer <= 0f)
        {
            Dash();
        }

        // Verifica se o jogador apertou o botão de liqueficação (Q)
        if (Input.GetKeyDown(KeyCode.Q) && !isLiquidified && liquidCooldownTimer <= 0f)
        {
            StartCoroutine(Liquidify());
        }

        // Atualiza o tempo de cooldown do dash
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        // Atualiza o tempo de cooldown da liqueficação
        if (liquidCooldownTimer > 0f)
            liquidCooldownTimer -= Time.deltaTime;
    }


    void FixedUpdate()
    {
        // Movimenta o jogador
        rb.MovePosition(rb.position + inputVector * moveSpeed * Time.fixedDeltaTime);
    }

    // Função que realiza o Dash

    void Dash()
    {
        dashCooldownTimer = dashCooldown;  // Resetando o cooldown para o uso do dash  
        dashCount--;                        // Decrementa o número de dashes disponíveis  

        // Dash em direção à frente do jogador (direção de movimento)  
        Vector3 dashVector = inputVector * dashSpeed;
        rb.linearVelocity = new Vector3(dashVector.x, rb.linearVelocity.y, dashVector.z);  // Aplica o dash  

        Debug.Log("Dash realizado! Restam " + dashCount + " dashes.");

        // Inicia a recarga do dash  
        StartCoroutine(RechargeDash());
    }

    private IEnumerator RechargeDash()
    {
        yield return new WaitForSeconds(6);
        dashCount++;  // Recarrega o dash após 3 segundos  
        Debug.Log("Dash recarregado! Dashes disponíveis: " + dashCount);
    }


    // Função que realiza a liqueficação
    private IEnumerator Liquidify()
    {
        isLiquidified = true;
        liquidCooldownTimer = liquidCooldown; // Reseta o cooldown da liqueficação

        // Modificando o colisor para o jogador "ficar achatado" e expandido para os lados
        playerCollider.size = new Vector3(playerCollider.size.x * 2f, 0.1f, playerCollider.size.z * 2f);
        playerCollider.center = new Vector3(playerCollider.center.x, 0.1f, playerCollider.center.z);

        // Espera o tempo de liqueficação acabar
        yield return new WaitForSeconds(liquidDuration);

        // Restaura o tamanho e posição do colisor
        playerCollider.size = originalColliderSize;
        playerCollider.center = originalColliderCenter;

        // Garantir que o jogador volte para uma altura normal no Y
        Vector3 currentPos = transform.position;
        transform.position = new Vector3(currentPos.x, 1f, currentPos.z); // Altura padrão 1f

        isLiquidified = false;
        Debug.Log("Liqueficação terminada.");
    }


    // Função para recarregar os dashes individualmente após um tempo
    public void RechargeDashes()
    {
        dashCount = 3;  // Restaurar a quantidade de dashes
    }
}
