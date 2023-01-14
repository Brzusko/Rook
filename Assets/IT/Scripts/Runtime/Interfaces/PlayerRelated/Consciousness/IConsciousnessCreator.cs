using FishNet.Connection;

namespace IT.Interfaces
{
    public interface IConsciousnessCreator
    {
        public IPlayerConsciousness CreateConsciousness(NetworkConnection connection);
    }
}
