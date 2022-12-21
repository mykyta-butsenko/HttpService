using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServiceServer.Listener
{
    internal interface IListenerService
    {
        public Task<IEnumerable<string>> Listen(ListenerServiceConfig serviceConfig);
    }
}
