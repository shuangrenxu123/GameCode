using TMPro;

public class PingPanel : UIWindowBase
{
    TMP_Text ping;
    public void SetPingValue(int value)
    {
        ping.text = value.ToString();
    }

    public override void OnCreate()
    {
        ping = GetComponent<TMP_Text>();
    }

    public override void OnUpdate()
    {

    }

    public override void OnDelete()
    {

    }

    public override void OnFocus()
    {

    }

    public override void OnFocusOtherUI()
    {

    }
}
