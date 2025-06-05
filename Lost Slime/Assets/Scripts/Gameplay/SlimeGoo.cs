using System;
using UnityEngine;

public class SlimeGoo : MonoBehaviour
{
    [Tooltip("Multiplicador de velocidade quando o jogador estiver na po�a de slime (ex: 0.3 para 30% da velocidade normal).")]
    [SerializeField] private float slowMultiplier = 0.3f;

    [Header("Som (opcional)")]
    [SerializeField] private AudioClip slimeWalkClip;

    private void OnTriggerEnter(Collider other)
    {
        // Verifica se o objeto que entrou � o jogador (assegure que a tag do jogador � "Player")
        if (other.CompareTag("Player"))
        {
            PlayerMovement pm = other.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                pm.ApplySpeedMultiplier(slowMultiplier);
                Debug.Log("Jogador entrou na po�a de slime. Velocidade reduzida para " + slowMultiplier * 100 + "% da velocidade normal.");
                // Toca o som de andar em slime, se definido
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySlimeWalk();

            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Ao sair da po�a de slime, restaura a velocidade original
        if (other.CompareTag("Player"))
        {
            PlayerMovement pm = other.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                pm.RemoveSpeedMultiplier();
            }
        }
    }
}
