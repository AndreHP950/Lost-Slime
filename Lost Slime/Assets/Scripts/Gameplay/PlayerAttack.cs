// Assets/Scripts/Gameplay/PlayerAttack.cs
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

    void Update()
    {
        // decrementa cooldown
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        // checa input de tiro
        bool wantShoot = isAuto
            ? Input.GetButton("Fire1")
            : Input.GetButtonDown("Fire1");

        if (wantShoot && cooldownTimer <= 0f)
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
            dir.y = 0;                // anula componente vertical
            return dir.normalized;    // normaliza já “plano”
        }
        return transform.forward;
    }


    private void Shoot(Vector3 dir)
    {
        // calcula posição de spawn fora do player
        Vector3 spawnPos = transform.position + dir * spawnOffset;

        // instancia bullet
        var bulletGO = Instantiate(
            bulletPrefab,
            spawnPos,
            Quaternion.LookRotation(dir)
        );

        // configura velocidade e dano
        var rb = bulletGO.GetComponent<Rigidbody>();
        var bullet = bulletGO.GetComponent<Bullet>();
        bullet.damage = bulletDamage;
        rb.AddForce(dir * bulletSpeed, ForceMode.Impulse);
    }
}
