using Assets;
using CharacterController;
using UIPanel.Console;
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
            var console = GetOrOpenCommandWindow();
            console?.ShowInputPanel();
        }
    }

    private CommandUI GetOrOpenCommandWindow()
    {
        var existing = UIManager.Instance.GetUIWindow<CommandUI>();
        if (existing != null)
        {
            return existing;
        }

        var ui = Resources.Load<GameObject>("UI/ConsolePanel/ConsoleUI");
        if (ui == null)
        {
            Debug.LogWarning("ConsoleUI prefab not found in Resources/UI/ConsolePanel");
            return null;
        }

        return UIManager.Instance.OpenUI<CommandUI>(ui.GetComponent<CommandUI>());
    }
}
