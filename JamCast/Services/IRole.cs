namespace JamCast.Services
{
    public interface IRole
    {
        string Status { get; }

        void Update();

        void End();
    }

    public interface IProjectorRole : IRole
    {   
    }

    public interface IClientRole : IRole
    {
    }
}
