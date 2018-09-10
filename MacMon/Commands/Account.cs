using System.DirectoryServices;

namespace MacMon.Commands
{
    public class Account
    {
        public static void Reset(string computerName, string username, string newPassword) { 
            DirectoryEntry directoryEntry = new DirectoryEntry($"WinNT://{computerName}/{username}"); 
            directoryEntry.Invoke("SetPassword", newPassword);
        }
    }
}