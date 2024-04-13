using Downloader;
using Assets;
using UIWindow;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class UpdatPanel :MonoBehaviour
{
    public Image value;
    FileDownloader downloader;
    //public HotUpdater hotUpdater;
    private Toggle isupdate;
    private string[] files;
    private string url =  $"http://43.139.97.52/dlls/";
    public Button button;
    private void Start()
    {
        files = new string[]
        {
            "ui",
        };
        button.onClick.AddListener(()=> Download());
        Debug.Log(1111);
    }
    private void UpdateProgress(float value)
    {
        this.value.fillAmount = value;
    }
    private void Update()
    {
        FileDownloader.OnUpdate();
        DataDownloader.OnUpdate();
    }
    private void UpdateEnd(FileDownloader fileDownloader)
    {
        Debug.Log(url + "scences");
        if (fileDownloader.status == DownloadStatus.Succeeded)
        {

            //var test = ResourcesManager.Instance.LoadAsset<GameObject>("ui", "playerState.prefab");
            //UIManager.Instance.OpenUI<PlayerStateUI>(test.GetComponent<PlayerStateUI>());
            //fileDownloader.StartDownload();
            DataDownloader d = new DataDownloader(End, End, true);
            d.StartDownload(url + "scences");
        }
        else
        {
            Debug.LogError(fileDownloader.status);
            Debug.LogError("Downloader Error");
        }
    }
    void End(DataDownloader d)
    {
        Debug.Log(url + "ui");
        if (d.status == DownloadStatus.Succeeded)
        {
            AssetBundle.LoadFromMemory(d.Data);
            SceneManager.LoadScene("CharacterControlTest");
        }
    }
    public void Download()
    {
        Debug.Log("开始下载");
        //if (isupdate.isOn)
        //{
            //GetUIGameObject("update").SetActive(true);
            //value = GetUIGameObject("Image").GetComponent<Image>();
            downloader = new FileDownloader(UpdateEnd, UpdateEnd, true);
            downloader.StartDownload(PathUtil.DownloadPath+PathUtil.ABRootPath, url + files[0], files[0]);
            //hotUpdater = new HotUpdater();
            //hotUpdater.Download();

            //hotUpdater.actionDownloadValue += UpdateProgress;
            //hotUpdater.actionAllDownloadDone += UpdateEnd;
            //hotUpdater.actionNothongUpdate += UpdateEnd;
        //}
    }
}
