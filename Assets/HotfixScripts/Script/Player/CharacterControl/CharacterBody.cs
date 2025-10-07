using UnityEngine;
namespace CharacterController
{
    public class CharacterBody : MonoBehaviour
    {
        [SerializeField]
        Vector2 bodySize = new Vector2(1f, 2f);
        [SerializeField]
        float mass = 50f;

        public RigidbodyComponent RigidbodyComponent { get; private set; }

        public ColliderComponent ColliderComponent { get; private set; }

        public float Mass => mass;

        public Vector2 BodySize => bodySize;

        private void Awake()
        {
            ColliderComponent = gameObject.AddComponent<CapsuleColliderComponent3D>();
            RigidbodyComponent = gameObject.AddComponent<RigidbodyComponent3D>();
        }
        CharacterActor characterActor = null;

        private void OnValidate()
        {
            if (characterActor == null)
                characterActor = GetComponent<CharacterActor>();
            bodySize = new Vector2(
                Mathf.Max(bodySize.x, 0f),
                Mathf.Max(bodySize.y, bodySize.x + CharacterConstants.ColliderMinBottomOffset)
            );
        }
    }
}