using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PowerUpHUD : MonoBehaviour
{
    [SerializeField] private RectTransform powerUpsContainer; // Container criado para os ícones
    [SerializeField] private GameObject powerUpIconPrefab; // Prefab com Image (e opcionalmente um fundo)

    // Dicionário para armazenar os ícones exibidos
    private Dictionary<PowerUpType, GameObject> powerUpIcons = new Dictionary<PowerUpType, GameObject>();
    private PowerUpManager powerUpManager;

    private void Start()
    {
        powerUpManager = PowerUpManager.Instance;
        if (powerUpManager != null)
            powerUpManager.onPowerUpsChanged.AddListener(UpdatePowerUpDisplay);
        else
            Debug.LogError("PowerUpManager não encontrado!");

        UpdatePowerUpDisplay();
    }

    private void UpdatePowerUpDisplay()
    {
        // Remove todos os ícones existentes
        foreach (Transform child in powerUpsContainer)
            Destroy(child.gameObject);
        powerUpIcons.Clear();

        if (powerUpManager == null)
            return;

        List<PowerUpType> activePowerUps = powerUpManager.GetActivePowerUps();
        if (activePowerUps.Count == 0)
            return;

        float containerHeight = powerUpsContainer.rect.height;
        float iconHeight = containerHeight / activePowerUps.Count;

        for (int i = 0; i < activePowerUps.Count; i++)
        {
            PowerUpType type = activePowerUps[i];
            var info = powerUpManager.GetPowerUpInfo(type);
            if (info != null)
            {
                GameObject iconObj = Instantiate(powerUpIconPrefab, powerUpsContainer);
                Image iconImage = iconObj.GetComponentInChildren<Image>();
                if (iconImage != null)
                    iconImage.sprite = info.hudIcon;

                // Ajusta o RectTransform para distribuir verticalmente
                RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                float posY = -((iconHeight * i) + (iconHeight / 2));
                iconRect.anchoredPosition = new Vector2(0, posY);
                iconRect.sizeDelta = new Vector2(iconRect.sizeDelta.x, iconHeight * 0.8f);

                // Desativa timer se existir
                var timerText = iconObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (timerText != null)
                    timerText.gameObject.SetActive(false);

                powerUpIcons[type] = iconObj;
            }
        }
    }

}
