using Inedo.ProGet.Extensibility.UserDirectories;

/*
 * This user directory has the following behavior:
 * 
 * For all user names that contain an @ character, this will return a dummy IUserDirectoryUser, and the passwords for these dummy users are all "BadPassword"
 * For all other users, it will use whatever users are stored in ProGet's built in user directory in the database.
 * 
 * The persisted XML for this directory is:
 *   <SampleProGetExtension.UserDirectories.SampleUserDirectory Assembly="SampleProGetExtension"></SampleProGetExtension.UserDirectories.SampleUserDirectory>
 */

namespace SampleProGetExtension.UserDirectories
{
    public sealed class SampleUserDirectory : BuiltInDirectory
    {
        public override string GetDescription() => "Example Directory";

        // ProGet typically calls this method to validate that a user name is valid, and actually refers to a user (not a group)
        public override IUserDirectoryUser TryGetUser(string userName) => this.TryGetExternalUser(userName) ?? base.TryGetUser(userName);

        // This is called to validate both the user name as in TryGetUser, but also the password
        public override IUserDirectoryUser TryGetAndValidateUser(string userName, string password) => this.TryGetAndValidateExternalUser(userName, password) ?? base.TryGetAndValidateUser(userName, password);

        private IUserDirectoryUser TryGetExternalUser(string userName)
        {
            // Check in a naieve way for user names that look like an email address
            if (userName.Contains("@"))
            {
                // Return this as a valid user in the system
                return new BasicUserInfo(userName, null, userName);
            }

            // Indicate that the user is not in the external directory
            return null;
        }

        private IUserDirectoryUser TryGetAndValidateExternalUser(string userName, string password)
        {
            // Check in a naieve way for user names that look like an email address
            if (userName.Contains("@"))
            {
                // TODO: replace with actual password check
                if (password == "BadPassword")
                {
                    // Return this as a valid user in the system
                    return new BasicUserInfo(userName, null, userName);
                }
            }

            // Indicate that the user is not in the external directory
            return null;
        }
    }
}
