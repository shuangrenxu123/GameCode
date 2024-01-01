public class AudioPanel : WindowRoot
{
    public override void Start()
    {
        WindowsManager.Instance.DisableWindow<NetPanel>();
    }

    public override void Update()
    {

    }
}
