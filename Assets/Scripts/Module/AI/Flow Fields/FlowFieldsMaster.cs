using UnityEngine;
namespace FindPath
{
    public class FlowFieldsMaster : MonoBehaviour
    {
        public int width, height;
        public ActorGroup group;
        public GridMap gridMap;
        private Algorithm algorithm;
        public GameObject prefab;

        public Vector2[] goals = new Vector2[]
        {
            new Vector2(20,20)
        };
        private void Awake()
        {
            gridMap = new GridMap(width, height);
            group = new ActorGroup();
            algorithm = new Algorithm(gridMap, goals);
        }
        private void Start()
        {
            gridMap.Generate();
            Checkobstacles();
            algorithm.GenerateDistance();
            algorithm.GenerateVector();
            for (int i = 0; i < 100; i++)
            {
                var a = Instantiate(prefab, new Vector3(1, 0, 2), Quaternion.identity).GetComponent<Actor>();
                a.speed = Random.Range(1.5f, 2f);
                group.AddActor(a);
            }
        }
        private void Update()
        {
            AssignVelocities();
            group.Moveactors();
        }
        public void AssignVelocities()
        {
            for (int i = 0; i < group.actors.Count; i++)
            {
                var actor = group.actors[i];
                var pos = actor.position;
                var cell = gridMap.FindCellByPosition(new Vector2((int)pos.x, (int)pos.y));
                actor.direction = (cell != null) ? cell.direction : Vector2.zero;
            }
        }
        public void Checkobstacles()
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var colliders = Physics.OverlapBox(new Vector3(i, 0, j), new Vector3(0.5f, 0.5f, 0.5f));
                    if (colliders.Length != 0)
                    {
                        gridMap.cells[i, j].UnPassable = true;
                    }
                }
            }
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (Application.isPlaying)
                    {
                        if (gridMap.cells[i, j].UnPassable == true)
                            Gizmos.color = Color.red;
                        else
                        {
                            Gizmos.color = Color.white;
                        }
                    }
                    Gizmos.DrawWireCube(new Vector3(i, 0, j), new Vector3(1, 0, 1));
                }
            }
        }
    }
}
