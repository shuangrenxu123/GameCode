using UnityEngine;

namespace FindPath
{
    public class Actor : MonoBehaviour
    {
        public float speed = 1f;

        public Vector2 position
        {
            get
            {
                return new Vector2(transform.position.x, transform.position.z);
            }
            set
            {
                transform.position = new Vector3(value.x, 0, value.y);
            }
        }

        public Vector2 direction;

        public void Move()
        {
            position += direction * speed * Time.deltaTime;
        }
    }
}