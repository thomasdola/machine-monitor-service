using System;
using System.DirectoryServices.AccountManagement;

namespace MacMon.Commands
{
    public static class Account
    {
        public static void Reset(string username, string oldPassword, string newPassword) { 
            var context = new PrincipalContext(ContextType.Machine);
            var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
            if (user != null)
            {
                user.ChangePassword(oldPassword, newPassword);
            }
            else
            {
                throw new Exception("User Not Found");
            }
        }
    }
}