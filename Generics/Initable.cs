namespace Repos.Unity_HelperLib.Generics
{
    public interface Initable
    {
        public void Init();
    }

    public interface Initable<in T>
    {
        public void Init(T arg);
    }
    
    public interface Initable<in T, in T1>
    {
        public void Init(T arg, T1 arg1);
    }
}