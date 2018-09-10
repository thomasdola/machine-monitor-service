using System;
using MacMon.Models;
using RestSharp;

namespace MacMon.Services.Http
{
    public interface IMacMonApi
    {
        Identity Register(string username, string password);
        Identity GetIdentity(string username, string password);
        Job GetJob(JWT jwt);
        LogOut Logout(JWT jwt);
    }
    
    public class MacMonApi : IMacMonApi
    {
        private const string BaseUrl = "http://localhost:4000/api/windows";

        public T Execute<T>(RestRequest request) where T : new()
        {
            var client = new RestClient {BaseUrl = new Uri(BaseUrl)};
            var response = client.Execute<T>(request);

            if (response.ErrorException == null) return response.Data;
            
            const string message = "Error retrieving response.  Check inner details for more info.";
            var mamoException = new ApplicationException(message, response.ErrorException);
            throw mamoException;

        }

        public Identity Register(string username, string password)
        {
            var request = new RestRequest {Resource = "/auth/register", Method = Method.POST};
            request.AddParameter("name", username);
            request.AddParameter("password", password);
            return Execute<Identity>(request);
        }

        public Identity GetIdentity(string username, string password)
        {
            var request = new RestRequest {Resource = "/auth/login", Method = Method.POST};
            request.AddParameter("name", username);
            request.AddParameter("password", password);
            return Execute<Identity>(request);
        }

        public Job GetJob(JWT jwt)
        {
            var request = new RestRequest {Resource = "/jobs"};
            request.AddHeader("Authorization", $"Bearer {jwt.Token}");
            return Execute<Job>(request);
        }

        public LogOut Logout(JWT jwt)
        {
            var request = new RestRequest {Resource = "/auth/logout", Method = Method.POST};
            request.AddHeader("Authorization", $"Bearer {jwt.Token}");
            return Execute<LogOut>(request);
        }
    }
}