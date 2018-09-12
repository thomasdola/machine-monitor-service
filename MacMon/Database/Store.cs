using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using JsonFlatFileDataStore;
using MacMon.Models;

namespace MacMon.Database
{
    public class Store
    {
        public const string IdentityKey = "identity";
        public const string JobKey = "job";
        public const string ProfileKey = "profile";
        public const string UserActivitiesKey = "activities";
        public const string MachinePositionsKey = "positions";
        
        private readonly DataStore _dataStore;
        private readonly string _databasePath;
        
        public Store()
        {
            var location = Assembly.GetEntryAssembly().Location;
            var directoryPath = Path.GetDirectoryName(location);
            if (directoryPath != null) _databasePath = Path.Combine(directoryPath, "database.json");
            _dataStore = new DataStore(_databasePath);
        }
        
        public static DataStore InitStore()
        {
            var db = new Store();
            db.InitProfile();
            db.InitIdentity();
            db.InitJobs();
            db.InitEmptyMachinePositions();
            db.InitEmptyUserActivities();
            return db._dataStore;
        }
        
        private async void InitProfile()
        {
            Console.WriteLine("try to get or insert profile");
            try
            {
                _dataStore.GetItem<Profile>(ProfileKey);
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Inserting default profile");
                var profile = new Profile
                {
                    Username = System.Security.Principal.WindowsIdentity.GetCurrent().User?.ToString(), 
                    Name = System.Security.Principal.WindowsIdentity.GetCurrent().Name, 
                    Password = ""
                };
                await _dataStore.InsertItemAsync(ProfileKey, profile);
            }
        }

        private async void InitIdentity()
        {
            Console.WriteLine("try to get or insert identity");
            try
            {
                _dataStore.GetItem<Identity>(IdentityKey);
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Inserting default identity");
                var id = new Identity {Jwt = new JWT(), Uuid = null};
                await _dataStore.InsertItemAsync(IdentityKey, id);
            }
        }
        
        private async void InitJobs()
        {
            Console.WriteLine("try to get or insert job");
            try
            {
                _dataStore.GetItem<Job>(JobKey);
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Inserting default job");
                var job = new Job
                {
                    Services = new List<Process>(), 
                    Applications = new List<Process>()
                };
                await _dataStore.InsertItemAsync(JobKey, job);
            }
        }

        private async void InitEmptyMachinePositions()
        {
            Console.WriteLine("try to get or insert positions");
            try
            {
                _dataStore.GetCollection<Position>(MachinePositionsKey);
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Inserting empty positions");
                var positions = new List<Position>();
                await _dataStore.InsertItemAsync(MachinePositionsKey, positions);
            }
        }

        private async void InitEmptyUserActivities()
        {
            Console.WriteLine("try to get or insert user activities");
            try
            {
                _dataStore.GetCollection<UserActivity>(UserActivitiesKey);
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Inserting empty  user activities");
                var userActivities = new List<UserActivity>();
                await _dataStore.InsertItemAsync(UserActivitiesKey, userActivities);
            }
        }
    }
}