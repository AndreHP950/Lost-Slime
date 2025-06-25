using UnityEngine;

public class LuzPiscando : MonoBehaviour
{
    public Light luz;
    [Header("Intervalo de piscada (segundos)")]
    public float intervaloMin = 0.1f;
    public float intervaloMax = 0.7f;

    private float timer;
    private bool ligada = true;

    void Start()
    {
        if (luz == null)
            luz = GetComponent<Light>();
        SortearNovoIntervalo();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            ligada = !ligada;
            luz.enabled = ligada;
            SortearNovoIntervalo();
        }
    }

    void SortearNovoIntervalo()
    {
        timer = Random.Range(intervaloMin, intervaloMax);
    }
}
