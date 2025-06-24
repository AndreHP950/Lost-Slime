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

    private float cooldownTimer = 0f;
    private PlayerMovement playerMovement;

    [Header("Animation")]
    [SerializeField] private Animator attackAnimator;  // Referência ao Animator para trigger "Attack"

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();

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
            Shoot(dir);
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

    private void Shoot(Vector3 dir)
    {
        Vector3 spawnPos = transform.position + dir * spawnOffset;
        GameObject bulletGO = Instantiate(bulletPrefab, spawnPos, Quaternion.LookRotation(dir));

        Rigidbody rb = bulletGO.GetComponent<Rigidbody>();
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        bullet.damage = bulletDamage;
        rb.AddForce(dir * bulletSpeed, ForceMode.Impulse);

        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDamage();

        // Dispara trigger Attack
        if (attackAnimator != null)
            attackAnimator.SetTrigger("Attack");
    }
}
