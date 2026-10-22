using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FootstepAudio : MonoBehaviour
{
    [SerializeField] AudioSource source;
    [SerializeField] AudioClip[] footstepClips;
    [SerializeField] float stepDistance = 1.8f;
    [SerializeField, Range(0f, 1f)] float volume = 0.6f;
    [SerializeField, Range(0f, 0.3f)] float pitchVariation = 0.1f;

    CharacterController controller;
    float distanceSinceStep;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (!source) source = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!source || footstepClips == null || footstepClips.Length == 0) return;

        Vector3 v = controller.velocity;
        v.y = 0f;
        float speed = v.magnitude;
                if (!controller.isGrounded || speed < 0.1f)
        {
            if (source.isPlaying) source.Stop();
            distanceSinceStep = stepDistance * 0.5f; 
            return;
        }

        distanceSinceStep += speed * Time.deltaTime;
        if (distanceSinceStep >= stepDistance)
        {
            distanceSinceStep = 0f;
            source.clip = footstepClips[Random.Range(0, footstepClips.Length)];
            source.volume = volume;
            source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            source.Play();
        }
    }
}