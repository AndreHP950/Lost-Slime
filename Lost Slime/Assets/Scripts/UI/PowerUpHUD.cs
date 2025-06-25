using System.Collections.Generic;
using UnityEngine;

public class PowerUpHUD : MonoBehaviour
{
    // Refer�ncias aos slots pr�-definidos (arraste-os do Editor)
    [SerializeField] private GameObject slotAgility;
    [SerializeField] private GameObject slotDoubleShot;
    [SerializeField] private GameObject slotFireRate;

    private PowerUpManager powerUpManager;

    private void Start()
    {
        powerUpManager = PowerUpManager.Instance;
        if (powerUpManager != null)
            powerUpManager.onPowerUpsChanged.AddListener(UpdatePowerUpDisplay);
        else
            Debug.LogError("PowerUpManager n�o encontrado!");

        // Desativa todos os slots no in�cio
        slotAgility.SetActive(false);
        slotDoubleShot.SetActive(false);
        slotFireRate.SetActive(false);

        UpdatePowerUpDisplay();
    }

    private void UpdatePowerUpDisplay()
    {
        // Obt�m os power ups ativos do manager
        List<PowerUpType> activePowerUps = powerUpManager.GetActivePowerUps();

        // Ativa ou desativa os slots conforme estiverem ativos
        slotAgility.SetActive(activePowerUps.Contains(PowerUpType.Agility));
        slotDoubleShot.SetActive(activePowerUps.Contains(PowerUpType.DoubleShot));
        slotFireRate.SetActive(activePowerUps.Contains(PowerUpType.FireRate));
    }
}
