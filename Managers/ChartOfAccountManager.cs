using AutoCount.Authentication;
using AutoCount.Authentication;
using AutoCount.Data;
using AutoCount.Data;
using AutoCount.GL.AccountInquiry;
using AutoCount.GL.AccountMaintenance;
using AutoCount.GL.AccountMaintenance;
using AutoCount.PlugIn;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
namespace AutoCountAPInvoiceAPI.Managers
{
    public class ChartOfAccount
    {
        public string AccNo { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        //public string AccountType { get; set; } = string.Empty;
        //public bool IsActive { get; set; }
        //public bool IsControlAccount { get; set; }
    }

   
    public class AccountSource
    {
        public string AccNo { get; set; }
        public string Description { get; set; }
        public string AccType { get; set; } // e.g., "AST", "EXP", etc.
        //public string CurrencyCode { get; set; } = "MYR"; // default value
        //public string CashFlowCategory { get; set; } = "OperatingActivities"; // optional override
        //public string SpecialAccType { get; set; } = "Normal"; // optional override
    }


    public class ChartOfAccountManager
    {


        public string NewAccountEntry(UserSession userSession, AccountSource accountSource)
        {
            if (userSession == null || !userSession.IsLogin)
            {
                Console.WriteLine("Invalid or expired session. Please log in again.");
                return null;
            }

            if (string.IsNullOrEmpty(accountSource.AccNo) || string.IsNullOrEmpty(accountSource.Description) || string.IsNullOrEmpty(accountSource.AccType))
            {
                Console.WriteLine("AccNo, Description, and AccType are required.");
                return null;
            }

            var cmd = AccountCommand.Create(userSession, userSession.DBSetting);

            if (cmd.IsAccountExists(accountSource.AccNo))
            {
                Console.WriteLine($"Account {accountSource.AccNo} already exists.");
                return null;
            }

            var account = cmd.NewAccount();
            var row = account.Table.Rows[0];

            try
            {
                row["AccNo"] = accountSource.AccNo;
                row["Description"] = accountSource.Description;
                row["AccType"] = accountSource.AccType;
                //row["CurrencyCode"] = accountSource.CurrencyCode;

                //row["SpecialAccType"] = AccountCommand.SpecialAccTypeToObject(
                //    Enum.TryParse(accountSource.SpecialAccType, out SpecialAccType special) ? special : SpecialAccType.Normal
                //);

                //row["CashFlowCategory"] = AccountCommand.CashFlowCategoryToObject(
                //    Enum.TryParse(accountSource.CashFlowCategory, out CashFlowCategory cf) ? cf : CashFlowCategory.OperatingActivities
                //);

                row.EndEdit();

                cmd.SaveAccount(userSession.DBSetting, account);
                Console.WriteLine($"Account {accountSource.AccNo} created successfully.");
                return accountSource.AccNo;
            }
            catch (AutoCount.AppException ex)
            {
                Console.WriteLine($"AutoCount error creating account: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return null;
            }
        }
    

        public List<ChartOfAccount> GetChartOfAccounts(UserSession userSession)
        {
            var result = new List<ChartOfAccount>();

            if (userSession == null || !userSession.IsLogin)
                return result;

            var connStr = userSession.DBSetting.ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string sql = @"
                        SELECT AccNo, Description
                        FROM GLMast
                        ORDER BY AccNo";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new ChartOfAccount
                            {
                                AccNo = reader["AccNo"].ToString() ?? "",
                                Description = reader["Description"].ToString() ?? "",
                                //AccountType = reader["AccountType"].ToString() ?? "",
                                //IsActive = Convert.ToBoolean(reader["IsActive"]),
                                //IsControlAccount = Convert.ToBoolean(reader["IsControlAccount"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error accessing COA via SQL: " + ex.Message);
            }

            return result;
        }
    }
}
