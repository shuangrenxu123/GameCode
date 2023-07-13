using HFSM;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test : MonoBehaviour
{
    StateMachine fsm;
    DataBase dataBase;
    private void Start()
    {
        dataBase = new DataBase();
        dataBase.SetData<bool>("sw", false);
        Test3();
    }
    private void Test1()
    {
        fsm = new StateMachine();
        fsm.database = dataBase;
        var state1 = new TestState();
        var state2 = new TestState();

        fsm.AddState("testState", state1);
        fsm.AddState("teststate2", state2);

        var S1toS2 = new StateTransition("testState", "teststate2");
        S1toS2.AddCondition(new StateCondition_Bool("sw", dataBase, true));

        fsm.AddTransition(S1toS2);

        fsm.Start();
    }
    private void Test2()
    {
        fsm = new StateMachine();
        var subfsm = new StateMachine();
        var state1 = new TestState();
        var state2 = new TestState();

        subfsm.AddState("testState", state1);
        subfsm.AddState("teststate2", state2);


        var S1toS2 = new StateTransition("testState", "teststate2");
        S1toS2.AddCondition(new StateCondition_Bool("sw", dataBase, true));

        subfsm.AddTransition(S1toS2);
        fsm.AddState("subfsm", subfsm);
        fsm.Start();
    }

    private void Test3()
    {
        fsm = new StateMachine() { name = "fsm"};

        var subfsm1 = new StateMachine();
        var subfsm2 = new StateMachine();

        var state1 = new TestState();
        var state2 = new TestState();
        var state3 = new TestState();
        var state4 = new TestState();

        subfsm1.AddState("state1", state1);
        subfsm1.AddState("state2", state2);

        subfsm2.AddState("state3", state3);
        subfsm2.AddState("state4", state4);

        fsm.AddState("subfsm1", subfsm1);
        fsm.AddState("subfsm2", subfsm2);

        var S1toS2 = new StateTransition("subfsm1", "subfsm2");
        S1toS2.AddCondition(new StateCondition_Bool("sw", dataBase, true));

        fsm.AddTransition(S1toS2);
        fsm.Start();
    }
    private void Update()
    {
        fsm.Update();
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            dataBase.SetData<bool>("sw",true);
            Debug.Log(111);
        }
    }
}
