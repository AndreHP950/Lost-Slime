using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

// Tipos de power ups dispon�veis no jogo
public enum PowerUpType
{
    Agility,    // Aumenta velocidade do jogador em 30%
    DoubleShot, // Dispara dois tiros lado a lado
    FireRate    // Reduz cooldown de tiro em 30%
}

// Informa��es sobre cada power up
[System.Serializable]
public class PowerUpInfo
{
    public PowerUpType type;
    public string name;
    public Sprite hudIcon;
    [Tooltip("Dura��o em segundos. 0 = permanente")]
    public float duration = 0f; // Agora 0 ou qualquer valor indica que � permanente; iremos ignorar essa propriedade.
}

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    [SerializeField] private int maxPowerUps = 3;
    [SerializeField] public List<PowerUpInfo> powerUpDefinitions = new List<PowerUpInfo>();
    [SerializeField] private bool debugMode = false; // Para testes

    // Evento disparado quando a cole��o de power ups � alterada
    public UnityEvent onPowerUpsChanged = new UnityEvent();

    // Lista de power ups ativos (permanentes)
    private HashSet<PowerUpType> activePowerUps = new HashSet<PowerUpType>();

    // Refer�ncias aos componentes do player
    private PlayerMovement playerMovement;
    private PlayerAttack playerAttack;

    private void Awake()
    {
        // Implementa��o de Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Encontra as refer�ncias do player
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
            playerAttack = player.GetComponent<PlayerAttack>();
        }
        else
        {
            Debug.LogError("Player n�o encontrado! Certifique-se de que ele tem a tag 'Player'");
        }

        if (debugMode)
        {
            Debug.Log("Modo de Depura��o ativo! Pressione 1, 2, 3 para ativar power ups.");
        }
    }

    private void Update()
    {
        // Uso apenas para testes
        if (debugMode)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                AddPowerUp(PowerUpType.Agility);

            if (Input.GetKeyDown(KeyCode.Alpha2))
                AddPowerUp(PowerUpType.DoubleShot);

            if (Input.GetKeyDown(KeyCode.Alpha3))
                AddPowerUp(PowerUpType.FireRate);
        }
    }

    // Adiciona um novo power up de forma permanente
    public bool AddPowerUp(PowerUpType type)
    {
        // Se o jogador j� tem o n�mero m�ximo de power ups e o novo n�o est� ativo, n�o adiciona mais
        if (activePowerUps.Count >= maxPowerUps && !activePowerUps.Contains(type))
            return false;

        // Se j� est� ativo, n�o faz nada (ou pode atualizar algum visual se desejar)
        if (activePowerUps.Contains(type))
            return true;

        activePowerUps.Add(type);

        // Aplica o efeito do power up
        ApplyPowerUpEffect(type);

        // Notifica os listeners
        onPowerUpsChanged.Invoke();

        PowerUpInfo info = GetPowerUpInfo(type);
        Debug.Log($"Power Up {info?.name} ativado permanentemente!");
        return true;
    }

    // Remove um power up
    public bool RemovePowerUp(PowerUpType type)
    {
        if (!activePowerUps.Contains(type))
            return false;

        activePowerUps.Remove(type);

        // Remove os efeitos
        RemovePowerUpEffect(type);

        // Notifica os listeners
        onPowerUpsChanged.Invoke();

        Debug.Log($"Power Up {GetPowerUpInfo(type)?.name} removido!");
        return true;
    }

    // Retorna a lista de power ups ativos
    public List<PowerUpType> GetActivePowerUps()
    {
        return activePowerUps.ToList();
    }

    // Retorna as informa��es de um power up espec�fico
    public PowerUpInfo GetPowerUpInfo(PowerUpType type)
    {
        return powerUpDefinitions.Find(p => p.type == type);
    }

    // Para power ups permanentes, n�o h� tempo restante, ent�o retorna 0 (ou pode ser omitido)
    public float GetRemainingTime(PowerUpType type)
    {
        return 0f;
    }

    // Aplica o efeito do power up
    private void ApplyPowerUpEffect(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.Agility:
                if (playerMovement != null)
                    playerMovement.ApplySpeedMultiplier(1.3f); // +30% velocidade
                break;
            case PowerUpType.DoubleShot:
                if (playerAttack != null)
                    playerAttack.EnableDoubleShot(true);
                break;
            case PowerUpType.FireRate:
                if (playerAttack != null)
                    playerAttack.ApplyFireRateMultiplier(1.3f); // +30% taxa de tiro
                break;
        }
    }

    // Remove o efeito do power up
    private void RemovePowerUpEffect(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.Agility:
                if (playerMovement != null)
                    playerMovement.RemoveSpeedMultiplier();
                break;
            case PowerUpType.DoubleShot:
                if (playerAttack != null)
                    playerAttack.EnableDoubleShot(false);
                break;
            case PowerUpType.FireRate:
                if (playerAttack != null)
                    playerAttack.RemoveFireRateMultiplier();
                break;
        }
    }
}
