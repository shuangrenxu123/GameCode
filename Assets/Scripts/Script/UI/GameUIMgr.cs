using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameUIMgr : WindowRoot
{
    private string activePanle = string.Empty;
    public override void Start()
    {
        gameObject.SetActive(false);
        GetUIEvnetListener("Network").PointerClick += OpenNetworkPanel;
    }

    private void OpenNetworkPanel(PointerEventData eventData)
    {
        if(activePanle == "Network")
        {
            activePanle = string.Empty;
            WindowsManager.Instance.DisableWindow<NetPanel>();
        }
        else
        {
            activePanle = "Network";
            WindowsManager.Instance.EnableWindow<NetPanel>();
        }
    }

    public override void Update()
    {
    }

    public override void UpdateWindow()
    {
    }
}
