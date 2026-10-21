using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Lantern : MonoBehaviour
{
    [SerializeField] float bobHeight = 0.15f;
    [SerializeField] float bobSpeed = 2f;
    [SerializeField] float spinSpeed = 45f;

    Vector3 startPos;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance) GameManager.Instance.CollectLantern();
        Destroy(gameObject);
    }
}