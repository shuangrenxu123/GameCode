using Character.Player;
using Fight;
using UIWindow;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.CharacterState
{
    public class MainHud : UIWindowBase
    {
        [SerializeField]
        Image hpBar;

        [SerializeField]
        Image mpBar;

        [SerializeField]
        Image energyBar;


        CombatEntity player;
        public override void OnInit(object userData)
        {
            player = Player.Instance.CombatEntity;

        }

    }


}


