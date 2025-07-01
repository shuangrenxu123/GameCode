using ObjectPool;

public class DefaultNetWorkPackage : IReferenceObject
{
    public string SenderId;
    public int MsgId;
    public object Msgobj;

    public void OnInit()
    {
    }

    public void OnRelease()
    {
    }
}
