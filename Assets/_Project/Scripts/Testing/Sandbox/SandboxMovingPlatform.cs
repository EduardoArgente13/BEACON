using UnityEngine;

/// <summary>
/// Simple ping-pong moving platform for sandbox layouts.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class SandboxMovingPlatform : MonoBehaviour
{
    public Vector2 offset = new Vector2(4, 0);
    public float speed = 1.5f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private float t;
    private bool forward = true;

    private void Start()
    {
        startPos = transform.position;
        targetPos = startPos + (Vector3)offset;
    }

    private void FixedUpdate()
    {
        t += Time.fixedDeltaTime * speed * (forward ? 1f : -1f);
        float lerp = Mathf.PingPong(t, 1f);
        transform.position = Vector3.Lerp(startPos, targetPos, lerp);

        if (lerp >= 0.999f) forward = false;
        else if (lerp <= 0.001f) forward = true;
    }
}
