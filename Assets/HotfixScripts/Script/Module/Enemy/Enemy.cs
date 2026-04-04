using Animancer;
using Character.Controller;
using CharacterController;
using Fight;
using UnityEngine;

namespace Enemy
{
    public class Enemy : MonoBehaviour
    {
        [Header("身体组件")]
        public CharacterActor characterActor;
        public CombatEntity combatEntity;

        [Header("AI系统")]
        [SerializeField] private GameObject brainGo;

        [Header("动画系统")]
        public AnimancerComponent animancer;

        [SerializeField]
        private NPCStateMgr NPCStateMgr;
        IEnemyBrain enemyBrain;

        private void Awake()
        {
            enemyBrain = brainGo.GetComponent<IEnemyBrain>();
        }

        void Start()
        {
            InitializeBrain();
        }

        private void InitializeBrain()
        {
            if (enemyBrain != null)
            {
                enemyBrain.Initialize(this);
            }
        }

        private void OnDestroy()
        {
            if (enemyBrain != null)
            {
                enemyBrain.Shutdown();
            }
        }
    }
}
