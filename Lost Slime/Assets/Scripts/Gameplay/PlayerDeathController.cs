using UnityEngine;
using UnityEngine.Events;

public class PlayerDeathController : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private Health health;                  // seu componente Health
    [SerializeField] private PlayerMovement movement;        // seu controlador de movimento
    [SerializeField] private PlayerAttack attack;            // seu controlador de tiro

    [Header("UI")]
    [SerializeField] private GameObject deathPanel;          // painel com botão “Ressuscitar”

    private Vector3 spawnPoint;

    void Awake()
    {
        // lembra onde nasceu
        spawnPoint = transform.position;

        // referencia automática se não arrastou
        if (health == null) health = GetComponent<Health>();
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (attack == null) attack = GetComponent<PlayerAttack>();

        // fecha o painel no começo
        if (deathPanel != null) deathPanel.SetActive(false);

        // inscreve no evento onDied
        health.onDied.AddListener(OnPlayerDied);
    }

    private void OnPlayerDied()
    {
        // bloqueia controles
        movement.enabled = false;
        attack.enabled = false;

        // mostra UI de morte
        if (deathPanel != null)
            deathPanel.SetActive(true);
    }

    // este método será chamado pelo botão “Ressuscitar”
    public void Respawn()
    {
        // reposiciona
        transform.position = spawnPoint;
        // reseta vida
        health.Apply(health.MaxHealth);
        // oculta painel e reativa controles
        if (deathPanel != null)
            deathPanel.SetActive(false);
        movement.enabled = true;
        attack.enabled = true;
    }
}
