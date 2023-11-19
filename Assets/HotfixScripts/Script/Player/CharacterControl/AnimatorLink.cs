using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class AnimatorLink : MonoBehaviour
{
    Animator _animator;
    bool _resetIKWeightFlag = false;
    public event System.Action OnAnimatorMoveEvent;
    public event System.Action<int> OnAnimatorIKEvent;

    public void ResetIkWeights() => _resetIKWeightFlag = true;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    private void OnAnimatorMove()
    {
        OnAnimatorMoveEvent?.Invoke();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (_resetIKWeightFlag)
        {
            _resetIKWeightFlag = false;
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0f);
            _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0f);
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
        }
        OnAnimatorIKEvent?.Invoke(layerIndex);
    }
}
