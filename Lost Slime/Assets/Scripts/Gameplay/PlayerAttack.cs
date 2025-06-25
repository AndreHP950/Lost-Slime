using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private GameObject bulletPrefab;  // prefab da bala

    [Header("Propriedades de Tiro")]
    [Tooltip("Distância do player onde a bala surge")]
    [SerializeField] private float spawnOffset = 1f;
    [Tooltip("Velocidade inicial da bala")]
    [SerializeField] private float bulletSpeed = 10f;
    [Tooltip("Dano por bala")]
    [SerializeField] private float bulletDamage = 1f;
    [Tooltip("Tiros por segundo")]
    [SerializeField] private float fireRate = 4f;
    [Tooltip("true = segurar para atirar; false = só clique")]
    [SerializeField] private bool isAuto = false;

    [Header("PowerUps")]
    [Tooltip("Espaçamento lateral para tiro duplo")]
    [SerializeField] private float doubleShotOffset = 0.5f;

    private float cooldownTimer = 0f;
    private PlayerMovement playerMovement;
    private float baseFireRate;
    private bool hasDoubleShot = false;

    [Header("Animation")]
    [SerializeField] private Animator attackAnimator;  // Referência ao Animator para trigger "Attack"

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        baseFireRate = fireRate;

        // Tenta obter o Animator se não tiver sido atribuído
        if (attackAnimator == null)
            attackAnimator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        bool wantShoot = isAuto ? Input.GetButton("Fire1") : Input.GetButtonDown("Fire1");

        // Não atira se estiver liquidificado
        if (wantShoot && cooldownTimer <= 0f && !playerMovement.IsLiquidified)
        {
            Vector3 dir = GetMouseDirection();

            // Dispara um ou dois tiros, dependendo do power up
            if (hasDoubleShot)
            {
                ShootDouble(dir);
            }
            else
            {
                Shoot(dir);
            }

            cooldownTimer = 1f / fireRate;
        }
    }

    // Retorna vetor normalizado do player até o mouse no plano y=0
    private Vector3 GetMouseDirection()
    {
        Plane ground = new Plane(Vector3.up, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (ground.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 dir = hitPoint - transform.position;
            dir.y = 0;
            return dir.normalized;
        }
        return transform.forward;
    }

    // Disparo normal (um tiro)
    private void Shoot(Vector3 dir)
    {
        Vector3 spawnPos = transform.position + dir * spawnOffset;
        GameObject bulletGO = Instantiate(bulletPrefab, spawnPos, Quaternion.LookRotation(dir));

        Rigidbody rb = bulletGO.GetComponent<Rigidbody>();
        Bullet bullet = bulletGO.GetComponent<Bullet>();

        if (bullet != null)
            bullet.damage = bulletDamage;

        if (rb != null)
            rb.AddForce(dir * bulletSpeed, ForceMode.Impulse);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDamage();

        // Dispara trigger Attack
        if (attackAnimator != null)
            attackAnimator.SetTrigger("Attack");
    }

    // Disparo duplo (dois tiros lado a lado)
    private void ShootDouble(Vector3 dir)
    {
        // Calcula direção perpendicular para deslocamento lateral
        Vector3 perpendicular = new Vector3(-dir.z, 0, dir.x).normalized;

        // Posição para o primeiro tiro (esquerda)
        Vector3 spawnPos1 = transform.position + dir * spawnOffset + perpendicular * doubleShotOffset;
        GameObject bulletGO1 = Instantiate(bulletPrefab, spawnPos1, Quaternion.LookRotation(dir));

        // Posição para o segundo tiro (direita)
        Vector3 spawnPos2 = transform.position + dir * spawnOffset - perpendicular * doubleShotOffset;
        GameObject bulletGO2 = Instantiate(bulletPrefab, spawnPos2, Quaternion.LookRotation(dir));

        // Configura os projéteis
        SetupBullet(bulletGO1, dir);
        SetupBullet(bulletGO2, dir);

        // Toca o som de tiro apenas uma vez
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDamage();

        // Dispara trigger Attack
        if (attackAnimator != null)
            attackAnimator.SetTrigger("Attack");
    }

    // Configura um projétil (usado pelo ShootDouble para evitar duplicação de código)
    private void SetupBullet(GameObject bulletGO, Vector3 direction)
    {
        Rigidbody rb = bulletGO.GetComponent<Rigidbody>();
        Bullet bullet = bulletGO.GetComponent<Bullet>();

        if (bullet != null)
            bullet.damage = bulletDamage;

        if (rb != null)
            rb.AddForce(direction * bulletSpeed, ForceMode.Impulse);
    }

    // --- Métodos para PowerUps ---

    // Ativa/desativa o tiro duplo
    public void EnableDoubleShot(bool enable)
    {
        hasDoubleShot = enable;
        Debug.Log($"Double Shot {(enable ? "ativado" : "desativado")}!");
    }

    // Aplica multiplicador na taxa de tiro (FireRate)
    public void ApplyFireRateMultiplier(float multiplier)
    {
        fireRate = baseFireRate * multiplier;
        Debug.Log($"Fire rate aumentada para {fireRate} tiros/s");
    }

    // Remove o multiplicador da taxa de tiro
    public void RemoveFireRateMultiplier()
    {
        fireRate = baseFireRate;
        Debug.Log($"Fire rate restaurada para {fireRate} tiros/s");
    }
}

