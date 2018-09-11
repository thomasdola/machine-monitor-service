using JsonFlatFileDataStore;
using Phoenix;

namespace MacMon.Jobs
{
    public class LocationMonitor
    {
        private readonly DataStore _store;
        private readonly Channel _channel;

        public LocationMonitor(DataStore store, Channel channel)
        {
            _store = store;
            _channel = channel;
        }
        
        public void OnStart(){}
        
        public void OnStop(){}
    }
}