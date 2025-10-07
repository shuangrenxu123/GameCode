using Assets;
using CharacterController;
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
            CharacterBrain.EnableUIInput();
            var ui = ResourcesManager.Instance.LoadAsset<GameObject>("ui", "GameUI.prefab");
            UIManager.Instance.OpenUI<GameUIMgr>(ui.GetComponent<GameUIMgr>());
        }
        if (InputActions.OpenConsoleUI.Started)
        {
            CharacterBrain.EnableUIInput();
            //var ui = Resources.Load<GameObject>("ConsoleUI");

            var ui = ResourcesManager.Instance.LoadAsset<GameObject>("ui", "ConsoleUI.prefab");
            UIManager.Instance.OpenUI<CommandUI>(ui.GetComponent<CommandUI>());

        }
    }
}
