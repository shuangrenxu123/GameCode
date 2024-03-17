using UIWindow;
using UnityEngine;
[DefaultExecutionOrder(100)]
public class CharacterUiController : MonoBehaviour
{
    [SerializeField]
    private CharacterBrain CharacterBrain;

    bool showingUI;
    public CharacterActions InputActions
    {
        get
        {
            return CharacterBrain == null ?
                new CharacterActions() : CharacterBrain.CharacterActions;
        }
    }
    void Update()
    {
        if (InputActions.OpenUI.Started)
        {
            CharacterBrain.EnableUIIpnut();
            UIManager.Instance.OpenUI<GameUIMgr>(UnityEngine.Resources.Load<GameUIMgr>("GameUI"));

        }
    }
}
