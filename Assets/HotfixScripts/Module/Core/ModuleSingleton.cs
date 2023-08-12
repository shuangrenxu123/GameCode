public abstract class ModuleSingleton<T> where T : class, IModule
{
    private static T _instance;
    public static T Instance//每个注册的模块的实例
    {
        get
        {
            if (_instance == null)
                throw new System.Exception("不存在实例" + typeof(T));
            return _instance;
        }
    }
    public ModuleSingleton()
    {
        if (_instance != null)
        {
            throw new System.Exception("已经存在实例");
        }
        _instance = this as T;
    }
}
