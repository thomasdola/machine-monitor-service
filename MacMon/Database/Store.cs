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
            db.InsertDefaultProfile();
            db.InsertDefaultIdentity();
            db.InsertDefaultJob();
            return db._dataStore;
        }
        
        private async void InsertDefaultProfile()
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

        private async void InsertDefaultIdentity()
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
        
        private async void InsertDefaultJob()
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
                    Network = true, UserActivity = true, 
                    Services = new List<Process>(), 
                    Applications = new List<Process>()
                };
                await _dataStore.InsertItemAsync(JobKey, job);
            }
        }
    }
}