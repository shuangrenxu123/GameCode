public class IntNumber : Number
{
    public IntNumber(int v, PropertySourceType type)
    {
        Value = v;
        this.Type = type;
    }
}
public enum PropertySourceType
{
    Equipe,
    Buff,
    Self,//ÁÙÊ±ÊıÖµ
}
