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
        if (hotUpdater != null)
            hotUpdater.Update();
    }

    private void UpdateProgress(float value)
    {
        this.value.fillAmount = value;
    }
    private void UpdateEnd()
    {
        SceneManager.LoadScene("Fight");
        //StartCoroutine(ILRuntimeHelp.LoadILRuntime(OnILRuntimeInitialized));
    }
    public void download()
    {
        if (isupdate.isOn)
        {
            GetUIGameObject("update").SetActive(true);
            hotUpdater = new HotUpdater();
            //hotUpdater.Init();
            value = GetUIGameObject("Image").GetComponent<Image>();
            hotUpdater.actionDownloadValue += UpdateProgress;
            hotUpdater.actionAllDownloadDone += UpdateEnd;
            hotUpdater.actionNothongUpdate += UpdateEnd;
            //hotUpdater.Start();
        }
    }

}
