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
        if (CharacterBrain != null
            && CharacterBrain.TryGetInputCommand(CharacterInputType.OpenUI, out var openUICommand)
            && openUICommand.BoolValue)
        {
            CharacterBrain.EnableUIInput();
            // UIManager.Instance.OpenUI<GameUIMgr>(UIWindowGroup.Normal);
        }
        if (CharacterBrain != null
            && CharacterBrain.TryGetInputCommand(CharacterInputType.OpenConsole, out var consoleCommand)
            && consoleCommand.BoolValue)
        {
            CharacterBrain.EnableUIInput();
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

        return UIManager.Instance.OpenUI<CommandUI>("UI/CommandUI/CommandUI", UIWindowGroup.Normal);
    }
}
