using System.Diagnostics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class UpdatPanel : WindowRoot
{
    public Image value;
    public HotUpdater hotUpdater;
    private Toggle isupdate;

    public override void Start()
    {
        isupdate = GetUIGameObject("Toggle").GetComponent<Toggle>();
        GetUIGameObject("updatebutton").GetComponent<Button>().onClick.AddListener(download);
    }

    public override void Update()
    {
        hotUpdater?.Update();
    }

    private void UpdateProgress(float value)
    {
        this.value.fillAmount = value;
    }
    private void UpdateEnd()
    {
        UnityEngine.Debug.Log("下载完毕");
    }
    public void download()
    {
        if (isupdate.isOn)
        {
            GetUIGameObject("update").SetActive(true);
            value = GetUIGameObject("Image").GetComponent<Image>();
            hotUpdater = new HotUpdater();
            hotUpdater.Download();

            hotUpdater.actionDownloadValue += UpdateProgress;
            hotUpdater.actionAllDownloadDone += UpdateEnd;
            hotUpdater.actionNothongUpdate += UpdateEnd;
        }
    }

}
