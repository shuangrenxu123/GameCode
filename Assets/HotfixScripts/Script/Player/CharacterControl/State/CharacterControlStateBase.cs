using HFSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControlStateBase : StateBase
{
    bool overrideAnimatorController = true;

    public CharacterActor CharacterActor{ get; protected set; }


    /// <summary>
    /// ��ö���������
    /// </summary>
    public RuntimeAnimatorController RuntimeAnimatorController { get; set; }
    public bool OverrideAnimatorController => overrideAnimatorController;
    protected CharacterBrain CharacterBrain = null;
    public CharacterActions CharacterActions{ get {
            return CharacterBrain == null ?
                new CharacterActions() : CharacterBrain.CharacterActions;
        } 
    }
    public CharacterStateController_New CharacterStateController { get;protected set; }

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

}