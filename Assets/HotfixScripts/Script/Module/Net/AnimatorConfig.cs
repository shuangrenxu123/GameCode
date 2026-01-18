using Animancer;
using UnityEngine;

public class AnimatorConfig : MonoBehaviour
{
    [SerializeField]
    public LinearMixerTransition normalMoveAnimator;
    [SerializeField]
    public LinearMixerTransition crouchMoveAnimator;
    [SerializeField]
    public MixerState<Vector2> lockEnemyAnimator;
}
