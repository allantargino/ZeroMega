using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Azure.ActiveDirectory.GraphClient.Extensions;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace GraphConsoleApp
{
    public class Program
    {
        private static string extension = "AccountNumber";
        private static string extensionName = string.Format("extension_{0}_{1}", Constants.ClientId.Replace("-", ""), extension);

        [STAThread]
        private static void Main()
        {

            #region 1. Setup Active Directory Client

            ActiveDirectoryClient activeDirectoryClient;
            try
            {
                activeDirectoryClient = AuthenticationHelper.GetActiveDirectoryClientAsApplication();
            }
            catch (AuthenticationException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Acquiring a token failed with the following error: {0}", ex.Message);
                Console.ReadKey();
                return;
            }

            #endregion

            #region 2. Search For The Application

            Application appObject = new Application();
            List<IApplication> retrievedApps = null;
            try
            {
                retrievedApps = activeDirectoryClient.Applications
                    .Where(app => app.AppId.Equals(Constants.ClientId))
                    .ExecuteAsync().Result.CurrentPage.ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine("\nError getting new app {0} {1}", e.Message,
                    e.InnerException != null ? e.InnerException.Message : "");
            }
            // should only find one app with the specified client id
            if (retrievedApps != null && retrievedApps.Count == 1)
            {
                appObject = (Application)retrievedApps.First();
            }
            else
            {
                Console.WriteLine("App {0} not found.", Constants.ClientId);
                return;
            }

            #endregion

            #region 3. Search or Create an Extension Property

            try
            {
                // Locate for the property extension:
                IEnumerable<IExtensionProperty> allExtProperties = activeDirectoryClient.GetAvailableExtensionPropertiesAsync(false).Result;
                IExtensionProperty extProperty = allExtProperties.Where(
                    extProp => extProp.Name == extensionName).FirstOrDefault();

                //If the property doesn't exist:
                if (extProperty == null)
                {
                    ExtensionProperty newExtProperty = new ExtensionProperty
                    {
                        Name = extension,
                        DataType = "String",
                        TargetObjects = { "User" }
                    };

                    appObject.ExtensionProperties.Add(newExtProperty);
                    appObject.UpdateAsync().Wait();
                    Console.WriteLine("\nUser object extended successfully.");
                }
                else
                {
                    Console.WriteLine("\nProperty {0} was found.", extension);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\nError extending the user object {0} {1}", e.Message,
                    e.InnerException != null ? e.InnerException.Message : "");
                return;
            }

            #endregion

            #region 4. User Extension Manipulation

            while (true)
            {
                #region 4.1 Search User by UPN

                Console.WriteLine("\nEnter the user mail: ");
                string userName = Console.ReadLine();
                Console.WriteLine("\nEnter the value for {0}: ", extension);
                string value = Console.ReadLine();

                // search for a single user by UPN
                string searchString = userName;
                Console.WriteLine("\n Retrieving user with UPN {0}", searchString);
                User retrievedUser = new User();
                List<IUser> retrievedUsers = null;
                try
                {
                    retrievedUsers = activeDirectoryClient.Users
                        .Where(user => user.UserPrincipalName.Equals(searchString))
                        .ExecuteAsync().Result.CurrentPage.ToList();
                }
                catch (Exception e)
                {
                    Console.WriteLine("\nError getting new user {0} {1}", e.Message,
                        e.InnerException != null ? e.InnerException.Message : "");
                }

                // should only find one user with the specified UPN
                if (retrievedUsers != null && retrievedUsers.Count == 1)
                {
                    retrievedUser = (User)retrievedUsers.First();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error getting user {0}", searchString);
                    Console.ReadKey();
                    return;
                }

                #endregion

                #region 4.2 Manipulate an Extension Property

                try
                {
                    if (retrievedUser != null && retrievedUser.ObjectId != null)
                    {
                        retrievedUser.SetExtendedProperty(extensionName, value);
                        retrievedUser.UpdateAsync().Wait();
                        Console.WriteLine("\nUser {0}'s extended property set successully.", retrievedUser.DisplayName);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("\nError Updating the user object {0} {1}", e.Message,
                        e.InnerException != null ? e.InnerException.Message : "");
                }

                #endregion

                #region 4.3 Get an Extension Property

                try
                {
                    if (retrievedUser != null && retrievedUser.ObjectId != null)
                    {
                        IReadOnlyDictionary<string, object> extendedProperties = retrievedUser.GetExtendedProperties();
                        object extendedProperty = extendedProperties[extensionName];
                        Console.WriteLine("\n Retrieved User {0}'s extended property value is: {1}.", retrievedUser.DisplayName,
                            extendedProperty);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("\nError Updating the user object {0} {1}", e.Message,
                        e.InnerException != null ? e.InnerException.Message : "");
                }

                #endregion

                Console.WriteLine("\nDo you want to set the property for more users? [Y/N]");
                var answer = Console.ReadKey();
                if (answer.KeyChar.ToString().ToUpper() == "N")
                {
                    break;
                }
            }
            #endregion

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nCompleted. Press Any Key to Exit.");
            Console.ReadKey();

        }
    }
}