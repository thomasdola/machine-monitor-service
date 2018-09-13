using System;
using System.Collections.Generic;
using JsonFlatFileDataStore;
using Phoenix;
using System.Device.Location;
using MacMon.Database;
using MacMon.Models;
using MacMon.Services.WebSocket;

namespace MacMon.Jobs
{
    public class LocationMonitor
    {
        private readonly DataStore _store;
        private readonly Channel _channel;
        private GeoCoordinateWatcher _geoCoordinateWatcher;

        public const string Latitude = "latitude";
        public const string Longitude = "longitude";
        public const string Timestamp = "timestamp";

        public LocationMonitor(DataStore store, Channel channel)
        {
            _store = store;
            _channel = channel;
        }

        public void OnStart()
        {
            _geoCoordinateWatcher = new GeoCoordinateWatcher();

            _geoCoordinateWatcher.PositionChanged += watcher_PositionChanged;
            _geoCoordinateWatcher.Start();
        }

        public void OnStop()
        {
            _geoCoordinateWatcher.PositionChanged -= watcher_PositionChanged;
        }
        
        private void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            var timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            SavePosition(e.Position.Location.Latitude, e.Position.Location.Longitude, timeStamp);
        }

        private void SavePosition(double latitude, double longitude, long timestamp)
        {
            if (Network.Connection.IsAvailable())
            {
                var data = new Dictionary<string, object>
                {
                    {Latitude, latitude},
                    {Longitude, longitude},
                    {Timestamp, timestamp}
                };
                Server(data);
            }
            else
            {
                var position = new Position
                {
                    Lat = latitude,
                    Long = longitude,
                    Timestamp = timestamp
                };
                Locally(position);
            }
        }

        private void Server(Dictionary<string, object> data)
        {
            if (_channel.canPush)
            {
                _channel.Push(MacMonWebSocket.LocationStatusChanged, data);
            }
            else
            {
                var position = new Position
                {
                    Lat = (double) data[Latitude],
                    Long = (double) data[Longitude],
                    Timestamp = (long) data[Timestamp]
                };
                Locally(position);
            }
        }

        private async void Locally(Position position)
        {
            var positions = _store.GetCollection<Position>(Store.MachinePositionsKey);
            await positions.InsertOneAsync(position);
        }
    }
}