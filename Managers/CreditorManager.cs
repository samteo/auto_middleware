using AutoCount.ARAP.Creditor;
using AutoCount.Authentication;
using AutoCount.Data;
using System.Data.SqlClient;
using AutoCount.GL;
using System;

namespace AutoCountAPInvoiceAPI.Managers
{
    public class CreditorSource
    {
        public required string ControlAccount { get; set; }
        public required string CompanyName { get; set; }
        public string Addr1 { get; set; } = string.Empty;
        public string Addr2 { get; set; } = string.Empty;
        public string Addr3 { get; set; } = string.Empty;
        public string Addr4 { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
    public class CreditorData
    {
        public string AccNo { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
    }

    internal class CreditorManager
    {
        public string? NewCreditor(AutoCount.Authentication.UserSession userSession, CreditorSource source)
        {
            if (userSession == null || !userSession.IsLogin)
            {
                Console.WriteLine("Invalid or expired session. Please log in again.");
                return null;
            }

            var existingCreditor = FindCreditorByCompanyName(userSession, source.CompanyName);
            if (existingCreditor != null)
            {
                Console.WriteLine($"Creditor with company name '{source.CompanyName}' already exists: {existingCreditor.AccNo}");
                return existingCreditor.AccNo;
            }


            string? newCreditorCode = GetNewCreditorCode(userSession, source.ControlAccount, source.CompanyName);
            if (newCreditorCode == null)
            {
                Console.WriteLine("Failed to generate new creditor code.");
                return null;
            }

            string userId = userSession.LoginUserID;
            AutoCount.ARAP.Creditor.CreditorDataAccess cmd =
                AutoCount.ARAP.Creditor.CreditorDataAccess.Create(userSession, userSession.DBSetting);
            AutoCount.ARAP.Creditor.CreditorEntity creditor = cmd.NewCreditor();

            creditor.ControlAccount = source.ControlAccount;
            creditor.AccNo = newCreditorCode;
            creditor.CompanyName = source.CompanyName;
            creditor.Address1 = source.Addr1;
            creditor.Address2 = source.Addr2;
            creditor.Address3 = source.Addr3;
            creditor.Address4 = source.Addr4;
            creditor.Phone1 = source.Phone;
            creditor.Phone2 = source.Mobile;
            creditor.Attention = source.ContactPerson;
            creditor.EmailAddress = source.Email;
            creditor.CurrencyCode = AccountBookLocalCurrency(userSession.DBSetting);

            try
            {
                cmd.SaveCreditor(creditor, userId);
                Console.WriteLine($"Creditor {creditor.AccNo} created successfully.");
                return newCreditorCode;
            }
            catch (AutoCount.AppException ex)
            {
                Console.WriteLine($"Error creating creditor: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return null;
            }
        }

        public string? GetNewCreditorCode(AutoCount.Authentication.UserSession userSession, string controlAccNo, string companyName)
        {
            try
            {
                return AutoCount.GL.AccountCodeHelper.Create(userSession.DBSetting)
                    .GetNextCreditorCode(controlAccNo, companyName);
            }
            catch (AutoCount.GL.InvalidAutoDebtorCodeFormatException ex)
            {
                Console.WriteLine($"Invalid creditor code format: {ex.Message}");
                return null;
            }
            catch (AutoCount.Data.DataAccessException ex)
            {
                Console.WriteLine($"Data access error: {ex.Message}");
                return null;
            }
        }

        private string AccountBookLocalCurrency(DBSetting dbSetting)
        {
            // Placeholder: Replace with actual currency code or implementation
            return "MYR";
        }

        public CreditorEntity? FindCreditorByCompanyName(UserSession userSession, string companyName)
        {
            if (userSession == null || !userSession.IsLogin)
            {
                Console.WriteLine("Invalid or expired session.");
                return null;
            }

            try
            {
                var dbSetting = userSession.DBSetting;
                var accNoList = new List<string>();

                using (SqlConnection conn = new SqlConnection(dbSetting.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT AccNo FROM Creditor", conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                accNoList.Add(reader.GetString(0));
                            }
                        }
                    }
                }

                var dataAccess = CreditorDataAccess.Create(userSession, dbSetting);

                foreach (var accNo in accNoList)
                {
                    var creditor = dataAccess.GetCreditor(accNo);
                    if (creditor != null && string.Equals(creditor.CompanyName, companyName, StringComparison.OrdinalIgnoreCase))
                    {
                        return creditor;
                    }
                }

                Console.WriteLine("No creditor found with company name: " + companyName);
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding creditor: {ex.Message}");
                return null;
            }
        }

        public List<CreditorData> GetCreditorData(UserSession userSession)
        {
            if (userSession == null || !userSession.IsLogin)
            {
                Console.WriteLine("Invalid or expired session.");
                return new List<CreditorData>();
            }

            try
            {
                var dbSetting = userSession.DBSetting;
                var creditors = new List<CreditorData>();

                using (SqlConnection conn = new SqlConnection(dbSetting.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT AccNo, CompanyName FROM Creditor", conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var creditor = new CreditorData
                                {
                                    AccNo = reader.GetString(0),
                                    CompanyName = reader.GetString(1)
                                };
                                creditors.Add(creditor);
                            }
                        }
                    }
                }

                return creditors;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding creditor: {ex.Message}");
                return new List<CreditorData>();
            }
        }
    }
}

