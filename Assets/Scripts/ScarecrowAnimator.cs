using UnityEngine;

[RequireComponent(typeof(ScarecrowAI))]
public class ScarecrowAnimator : MonoBehaviour
{
    [Tooltip("The model's Animator. Found automatically on children if empty.")]
    [SerializeField] Animator animator;
    [Tooltip("Bool parameter in the Animator Controller that switches Idle <-> Walk.")]
    [SerializeField] string movingParameter = "Moving";
    [Tooltip("Stop-motion frame rate. 12 looks hand-animated. Set to 0 for smooth animation.")]
    [SerializeField] float stopMotionFps = 12f;

    [Tooltip("On: snaps back into the Idle pose (T-pose) the instant it's seen. Off: freezes mid-stride.")]
    [SerializeField] bool snapToIdleWhenSeen = true;

    ScarecrowAI ai;
    bool wasFrozen;
    int movingHash;
    bool hasMovingParam;
    float timer;

    void Awake()
    {
        ai = GetComponent<ScarecrowAI>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!animator)
        {
            Debug.LogWarning($"{name}: no Animator found on the model.", this);
            enabled = false;
            return;
        }

        movingHash = Animator.StringToHash(movingParameter);
        foreach (AnimatorControllerParameter p in animator.parameters)
            if (p.nameHash == movingHash && p.type == AnimatorControllerParameterType.Bool)
                hasMovingParam = true;

        if (!hasMovingParam)
            Debug.LogWarning($"{name}: Animator has no bool parameter called '{movingParameter}'.", this);

        animator.applyRootMotion = false; 
        animator.enabled = false;         
    }

    void Update()
    {
        ScarecrowAI.State state = ai.CurrentState;

        if (hasMovingParam)
            animator.SetBool(movingHash, state == ScarecrowAI.State.Chasing);

        if (state == ScarecrowAI.State.Frozen)
        {
            if (snapToIdleWhenSeen && !wasFrozen)
                animator.Update(0.02f); 
            wasFrozen = true;
            return; 
        }
        wasFrozen = false;

        if (stopMotionFps <= 0f)
        {
            animator.Update(Time.deltaTime);
            return;
        }

        timer += Time.deltaTime;
        float step = 1f / stopMotionFps;
        if (timer >= step)
        {
            animator.Update(timer);
            timer = 0f;
        }
    }
}