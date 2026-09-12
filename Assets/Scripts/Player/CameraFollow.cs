using UnityEngine;

/// <summary>
/// Smoothly follows the player from behind/above with a fixed offset.
/// The camera tracks the player's sideways (lane) movement partially so the
/// character stays comfortably on screen even on a wide track.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 3.5f, -6f);
    public float positionSmooth = 10f;
    public float rotationSmooth = 5f;

    [Range(0f, 1f)]
    [Tooltip("How much the camera follows the player sideways: 0 = stays centred, 1 = fully follows.")]
    public float laneFollowFactor = 0.55f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + offset;
        desired.x = offset.x + target.position.x * laneFollowFactor;

        transform.position = Vector3.Lerp(transform.position, desired, positionSmooth * Time.deltaTime);

        Vector3 lookTarget = target.position + Vector3.up * 1.2f;
        lookTarget.x *= laneFollowFactor; // aim roughly down the track, biased toward the player
        Quaternion desiredRot = Quaternion.LookRotation(lookTarget - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotationSmooth * Time.deltaTime);
    }
}
