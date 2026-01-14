using AutoCount.Authentication;
using AutoCount.Data;
using System;

namespace AutoCountAPInvoiceAPI.Managers
{
    internal class LoginManager
    {
        internal UserSession InitiateUserSessionUnattended(
            string serverName, 
            string dbName, 
            string autoCountUserLogin,    // AutoCount application user
            string autoCountUserPasswd,   // AutoCount application password
            string sqlUserLogin = "",     // SQL Server login (optional, empty = Windows Auth)
            string sqlUserPasswd = "")    // SQL Server password (optional)
        {
            AutoCount.MainEntry.Startup startup = new AutoCount.MainEntry.Startup();
            
            // Create DBSetting for SQL Server connection with proper constructor
            // If sqlUserLogin is empty, it will use Windows Authentication
            // If sqlUserLogin is provided, it will use SQL Server Authentication
            AutoCount.Data.DBSetting dbSetting = new AutoCount.Data.DBSetting(
                DBServerType.SQL2000, 
                serverName,
                sqlUserLogin,      // SQL Server UserID (empty string for Windows Auth)
                sqlUserPasswd,     // SQL Server Password (empty string for Windows Auth)
                dbName,
                usePool: true,     // Enable connection pooling
                maxPoolSize: 100   // Maximum pool size
            );
            
            // Create AutoCount UserSession
            AutoCount.Authentication.UserSession userSession = new AutoCount.Authentication.UserSession(dbSetting);

            try
            {
                // Login with AutoCount credentials (ADMIN/admin)
                if (userSession.Login(autoCountUserLogin, autoCountUserPasswd))
                {
                    startup.SubProjectStartup(userSession);
                    Console.WriteLine($"AutoCount login successful for user: {autoCountUserLogin}");
                    Console.WriteLine($"SQL Authentication: {(string.IsNullOrEmpty(sqlUserLogin) ? "Windows" : "SQL Server")}");
                }
                else
                {
                    Console.WriteLine("AutoCount login failed. Please check AutoCount user credentials.");
                }
            }
            catch (AutoCount.Data.CriticalSqlException sqlEx)
            {
                Console.WriteLine($"SQL Connection Error: {sqlEx.Message}");
                Console.WriteLine($"Server: {serverName}, Database: {dbName}");
                Console.WriteLine($"SQL User: {(string.IsNullOrEmpty(sqlUserLogin) ? "Windows Authentication" : sqlUserLogin)}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error during login: {ex.Message}");
                throw;
            }
            
            return userSession;
        }
    }
}

