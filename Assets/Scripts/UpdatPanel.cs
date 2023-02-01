using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class UpdatPanel : WindowRoot
{
    public Image value;
    public HotUpdater hotUpdater;
    private Toggle isupdate;

    public override void Start()
    {
        isupdate = GetUI("Toggle").GetComponent<Toggle>();
        GetUI("updatebutton").GetComponent<Button>().onClick.AddListener(download);
    }

    public override void Update()
    {
        if (hotUpdater != null)
            hotUpdater.Update();
    }

    public override void UpdateWindow()
    {

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
            GetUI("update").SetActive(true);
            hotUpdater = new HotUpdater();
            hotUpdater.Init();
            value = GetUI("Image").GetComponent<Image>();
            hotUpdater.actionDownloadValue += UpdateProgress;
            hotUpdater.actionAllDownloadDone += UpdateEnd;
            hotUpdater.actionNothongUpdate += UpdateEnd;
            hotUpdater.Start();
        }
        else
        {
            SceneManager.LoadScene("Fight");
        }
    }

}
