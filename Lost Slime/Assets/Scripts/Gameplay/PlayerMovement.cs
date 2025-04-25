using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody rb;
    private Vector3 inputVector;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation; // evita virar o “Slime”
    }

    void Update()
    {
        // leitura de input (WASD / setas)
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        inputVector = new Vector3(h, 0f, v).normalized;
    }

    void FixedUpdate()
    {
        // aplica movimento
        rb.MovePosition(rb.position + inputVector * moveSpeed * Time.fixedDeltaTime);
    }
}
