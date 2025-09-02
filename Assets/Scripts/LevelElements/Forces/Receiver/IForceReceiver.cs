public interface IForceReceiver
{
    void RegisterForceSource(IForceSource source);
    void UnregisterForceSource(IForceSource source);
}
