using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyDeath : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Health>()
           .onDied.AddListener(() => Destroy(gameObject));
    }
}
