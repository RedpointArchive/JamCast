namespace JamCast.Services
{
    public interface IRole
    {
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
