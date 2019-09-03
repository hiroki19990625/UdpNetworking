namespace UdpNetworking
{
    public enum ClientState
    {
        Initialized,
        Listening,
        ConnectionWait,
        Connected,
        Error,
        Closed
    }
}