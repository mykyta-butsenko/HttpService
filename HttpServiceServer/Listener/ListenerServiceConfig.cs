﻿namespace HttpServiceServer.Listener
{
    internal class ListenerServiceConfig
    {
        public string HostName { get; set; }
        public int Port { get; set; }
        public int Backlog { get; set; }
    }
}
