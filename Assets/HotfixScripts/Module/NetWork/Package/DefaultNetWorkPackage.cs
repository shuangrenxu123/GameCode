using ObjectPool;

public class DefaultNetWorkPackage : IReferenceObject
{
    public string SenderId;
    public int MsgId;
    public object MsgObj;

    public void OnInit()
    {
    }

    public void OnRelease()
    {
    }
}
