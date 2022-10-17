namespace IceEngine.Networking.Framework
{
    /// <summary>
    /// Network object with net id
    /// </summary>
    public abstract class NetworkObject : NetworkBehaviour
    {
        public override NetIDMark ID => _id;
        readonly NetIDMark _id = new();
    }
}
