public class EnemyController : CharacterLocomotionManager
{
    private void Awake()
    {
        entity = GetComponent<Enemy>();

    }
}
