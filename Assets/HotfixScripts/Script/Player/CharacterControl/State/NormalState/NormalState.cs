using HFSM;

namespace CharacterControllerStateMachine
{
    public class NormalState : StateMachine<StateType>
    {
        public override void Init()
        {
            var movemnetState = new MovementState()
            {
                database = database,
            };
        }
        private void AddCondition(CharacterStateController_New controller, string name, DataBase database, StateType form, StateType to)
        {
            var cond = new StateCondition_Bool(name, database, true);
            var transition = new StateTransition<StateType>(form, to);

            var cond2 = new StateCondition_Bool(name, database, false);
            var transition2 = new StateTransition<StateType>(to, form);
            transition.AddCondition(cond);
            transition2.AddCondition(cond2);

            controller.AddTransition(transition);
            controller.AddTransition(transition2);
        }
    }

}