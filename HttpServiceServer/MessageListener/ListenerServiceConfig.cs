namespace HttpServiceServer.MessageListener
{
    internal class ListenerServiceConfig
    {
#pragma warning disable CS8618
        public string HostName { get; set; }
#pragma warning restore CS8618
        public int Port { get; set; }
        public int Backlog { get; set; }
    }
}
