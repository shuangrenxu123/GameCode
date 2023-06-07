using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PingPanel : WindowRoot
{
    TMP_Text ping;
    public override void Start()
    {
        ping = GetComponent<TMP_Text>();
    }

    public override void Update()
    {

    }

    public void SetPingValue(int value)
    {
        ping.text = value.ToString();
    }
}
