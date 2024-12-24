using HFSM;

namespace CharacterControllerStateMachine
{
    public class CharacterControlStateBase : StateBase<StateType>
    {
        public CharacterActor CharacterActor { get; protected set; }
        public AnimactorHelper Animancer { get; set; }
        protected CharacterBrain CharacterBrain = null;
        public CharacterActions CharacterActions
        {
            get
            {
                return CharacterBrain == null ?
                    new CharacterActions() : CharacterBrain.CharacterActions;
            }
        }
        public CharacterStateController_New CharacterStateController { get; protected set; }
        protected CCAnimatorConfig animatorConfig => CharacterStateController.stateManger.animatorConfig;
        protected NetTranform netHelper => CharacterStateController.stateManger.NetHelper;
        //public Dictionary<string, ClipTransition> animators;
        public override void Init()
        {
            CharacterActor = (parentMachine as CharacterStateController_New).CharacterActor;
            CharacterBrain = (parentMachine as CharacterStateController_New).CharacterBrain;
            CharacterStateController = (parentMachine as CharacterStateController_New);
        }
        public virtual void UpdateIK(int layerIndex)
        {

        }
        /// <summary>
        /// ����ѭ��������ִ��
        /// </summary>
        public virtual void PostUpdate()
        {
        }
        /// <summary>
        /// �÷���������ѭ��֮ǰִ��
        /// </summary>
        public virtual void PreUpdate()
        {
        }

        /// <summary>
        /// �÷�����������ģ��ǰִ��
        /// </summary>
        public virtual void PreCharacterSimulation()
        {
        }

        /// <summary>
        /// �˷����ڽ�ɫ����ģ��֮�����С�
        /// </summary>
        public virtual void PostCharacterSimulation()
        {
        }
        //public void AddStateAnimators(List<ClipTransition> anims)
        //{
        //    if (anims == null || anims.Count == 0)
        //    {
        //        Debug.LogError("���Ӷ���ʧ��" + name);
        //        return;
        //    }
        //    if (animators == null)
        //        animators = new Dictionary<string, ClipTransition>(anims.Count);
        //    for (int i = 0; i < anims.Count; i++)
        //    {
        //        animators.Add(anims[i].Clip.name, anims[i]);
        //    }
        //}
    }
}