using System.DirectoryServices.AccountManagement;

namespace MacMon.Commands
{
    public class Account
    {
        public static void Reset(string username, string oldPassword, string newPassword) { 
            var Context = new PrincipalContext(ContextType.Machine);
            var User = UserPrincipal.FindByIdentity(Context, IdentityType.SamAccountName, username);
            User.ChangePassword(oldPassword, newPassword);
        }
    }
}