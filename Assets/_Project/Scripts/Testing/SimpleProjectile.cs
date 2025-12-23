using UnityEngine;

namespace BEACON.Testing
{
    public class SimpleProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private float lifetime = 5f;

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            // Move forward (right relative to rotation)
            transform.Translate(Vector2.right * speed * Time.deltaTime);
        }
    }
}
