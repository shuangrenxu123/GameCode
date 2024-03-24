using Downloader;
using UIWindow;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class UpdatPanel : UIWindowBase
{
    public Image value;
    public HotUpdater hotUpdater;
    private Toggle isupdate;

    private void UpdateProgress(float value)
    {
        this.value.fillAmount = value;
    }
    private void UpdateEnd()
    {
        UnityEngine.Debug.Log("下载完毕");
        SceneManager.LoadScene("CharacterControlTest");
    }
    public void Download()
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

    public override void OnCreate()
    {
        isupdate = GetUIGameObject("Toggle").GetComponent<Toggle>();
        GetUIGameObject("updatebutton").GetComponent<Button>().onClick.AddListener(Download);
    }

    public override void OnUpdate()
    {
        hotUpdater?.Update();
    }

}
