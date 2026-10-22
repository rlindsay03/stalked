using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ExitGate : MonoBehaviour
{
    [SerializeField] GameObject gateBlocker;
    [SerializeField] Light gateLight;
    [SerializeField] Color lockedColor = new Color(0.45f, 0.1f, 0.6f);  
    [SerializeField] Color openColor = new Color(0.75f, 0.35f, 1f);     
    [SerializeField] float openIntensityMultiplier = 3f;

    bool unlocked;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void Start()
    {
        if (gateLight) gateLight.color = lockedColor;
    }

    public void Unlock()
    {
        if (unlocked) return;
        unlocked = true;
        if (gateBlocker) gateBlocker.SetActive(false);
        if (gateLight)
        {
            gateLight.color = openColor;
            gateLight.intensity *= openIntensityMultiplier;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (unlocked && other.CompareTag("Player") && GameManager.Instance)
            GameManager.Instance.Win();
    }
}