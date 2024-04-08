using Resources;
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
            var ui = ResourcesManager.Instance.LoadAsset<GameObject>("ui","GameUI.prefab");
            UIManager.Instance.OpenUI<GameUIMgr>(ui.GetComponent<GameUIMgr>());

        }
    }
}
