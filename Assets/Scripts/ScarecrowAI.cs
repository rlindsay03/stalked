using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(NavMeshAgent))]
public class ScarecrowAI : MonoBehaviour
{
    public enum State { Idle, Chasing, Frozen, Caught }

    [Header("References")]
    [SerializeField] Transform player;
    [SerializeField] Camera playerCamera;
    [SerializeField] Renderer bodyRenderer;

    [Header("Behavior")]
    [SerializeField] float chaseSpeed = 4.5f;
    [SerializeField] float activationRange = 20f;
    [SerializeField] float hearingRange = 6f;
    [SerializeField] float giveUpTime = 6f;
    [SerializeField] float catchDistance = 1.3f;
    [SerializeField] float maxSightDistance = 25f;
    [SerializeField] LayerMask sightBlockers = ~0;

    [Header("Audio")]
    [SerializeField] AudioSource moveLoop;

    [Header("Events")]
    public UnityEvent onPlayerCaught;

    public State CurrentState { get; private set; } = State.Idle;

    NavMeshAgent agent;
    readonly Plane[] frustum = new Plane[6];
    readonly Vector3[] sightPoints = new Vector3[3];

    bool aware;               
    float lastSensedTime;     
    Vector3 lastKnownPos;     

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = chaseSpeed;

        if (!bodyRenderer) bodyRenderer = GetComponentInChildren<Renderer>();
        if (!playerCamera) playerCamera = Camera.main;
        if (!player)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p) player = p.transform;
        }
    }

    void Start()
    {
        agent.isStopped = true;
    }

    void Update()
    {
        if (CurrentState == State.Caught || !player || !playerCamera) return;

        float dist = FlatDistance(transform.position, player.position);

        bool sensed = dist <= hearingRange || (dist <= activationRange && CanSeePlayer());
        if (sensed)
        {
            aware = true;
            lastSensedTime = Time.time;
            lastKnownPos = player.position;
        }
        else if (aware && Time.time - lastSensedTime > giveUpTime)
        {
            aware = false;
        }

        State next;
        if (IsSeenByPlayer()) next = State.Frozen;
        else if (aware)       next = State.Chasing;
        else                  next = State.Idle;

        SetState(next);

        if (CurrentState == State.Chasing)
        {
            agent.SetDestination(sensed ? player.position : lastKnownPos);
            if (dist <= catchDistance) CatchPlayer();
        }
    }


    bool CanSeePlayer()
    {
        Vector3 eye = transform.position + Vector3.up * 1f;
        Vector3 target = playerCamera.transform.position;
        Vector3 toPlayer = target - eye;
        float d = toPlayer.magnitude;

        if (!Physics.Raycast(eye, toPlayer / d, out RaycastHit hit, d, sightBlockers, QueryTriggerInteraction.Ignore))
            return true;  

        return hit.transform == player || hit.transform.IsChildOf(player);
    }

    // ---------- sight check --------------------

    bool IsSeenByPlayer()
    {
        Bounds b = bodyRenderer
            ? bodyRenderer.bounds
            : new Bounds(transform.position + Vector3.up, new Vector3(1f, 2f, 1f));

         
        GeometryUtility.CalculateFrustumPlanes(playerCamera, frustum);
        if (!GeometryUtility.TestPlanesAABB(frustum, b)) return false;

         
        Vector3 eye = playerCamera.transform.position;
        float h = b.extents.y * 0.8f;
        sightPoints[0] = b.center + Vector3.up * h;
        sightPoints[1] = b.center;
        sightPoints[2] = b.center - Vector3.up * h;

        foreach (Vector3 point in sightPoints)
        {
            Vector3 toPoint = point - eye;
            float d = toPoint.magnitude;
            if (d > maxSightDistance) continue;

            if (!Physics.Raycast(eye, toPoint / d, out RaycastHit hit, d, sightBlockers, QueryTriggerInteraction.Ignore))
                return true;  

            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                return true;  
        }
        return false;
    }

    // ---------- state changes ----------

    void SetState(State next)
    {
        if (next == CurrentState) return;
        CurrentState = next;

        bool moving = next == State.Chasing;
        agent.isStopped = !moving;
        if (!moving) agent.velocity = Vector3.zero;  

        if (moveLoop)
        {
            if (moving && !moveLoop.isPlaying) moveLoop.Play();
            else if (!moving) moveLoop.Stop();
        }
    }

    void CatchPlayer()
    {
        CurrentState = State.Caught;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        if (moveLoop) moveLoop.Stop();

        onPlayerCaught.Invoke();

        if (GameManager.Instance)
            GameManager.Instance.Lose();
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    static float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f; b.y = 0f;
        return Vector3.Distance(a, b);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRange);
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, hearingRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, catchDistance);
    }
}