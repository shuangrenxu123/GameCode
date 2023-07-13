using HFSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    public class TestState : StateBase
    {
        public override void Enter()
        {
            base.Enter();
            Debug.Log(name + "Enter");
        }
        public override void Exit()
        {
            base.Exit();
            Debug.Log(name + "Exit");
        }
        public override void Update()
        {
            base.Update();
            Debug.Log(name + "Update");
        }
        public override void Init()
        {
            base.Init();
            Debug.Log(name + "Init");
        }
    }