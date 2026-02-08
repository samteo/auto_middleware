using AutoCount.Authentication; 
using AutoCount.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace AutoCountAPInvoiceAPI.Managers
{
    public class ItemManager
    {
        /// <summary>
        /// Gets the item code based on the item description.
        /// </summary>
        /// <param name="userSession">Active user session</param>
        /// <param name="description">Item description to search for</param>
        /// <param name="exactMatch">If true, performs exact match; otherwise performs partial match</param>
        /// <returns>Item code if found, otherwise null</returns>
        public string? GetItemCodeByDescription(UserSession userSession, string description, bool exactMatch = false)
        {
            if (userSession == null || !userSession.IsLogin)
            {
                Console.WriteLine("Invalid or expired session.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                Console.WriteLine("Description cannot be null or empty.");
                return null;
            }

            try
            {
                var dbSetting = userSession.DBSetting;
                string? itemCode = null;

                using (SqlConnection conn = new SqlConnection(dbSetting.ConnectionString))
                {
                    conn.Open();
                    
                    string query = exactMatch
                        ? "SELECT TOP 1 ItemCode FROM Item WHERE Description = @Description"
                        : "SELECT TOP 1 ItemCode FROM Item WHERE Description LIKE @Description";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Description", exactMatch ? description : $"%{description}%");

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                itemCode = reader.GetString(0);
                                Console.WriteLine($"Item found: {itemCode} for description: {description}");
                            }
                            else
                            {
                                Console.WriteLine($"No item found with description: {description}");
                            }
                        }
                    }
                }

                return itemCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding item: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets all item codes that match the description (supports multiple results).
        /// </summary>
        /// <param name="userSession">Active user session</param>
        /// <param name="description">Item description to search for</param>
        /// <param name="exactMatch">If true, performs exact match; otherwise performs partial match</param>
        /// <returns>List of item codes that match the description</returns>
        public List<string> GetItemCodesByDescription(UserSession userSession, string description, bool exactMatch = false)
        {
            var itemCodes = new List<string>();

            if (userSession == null || !userSession.IsLogin)
            {
                Console.WriteLine("Invalid or expired session.");
                return itemCodes;
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                Console.WriteLine("Description cannot be null or empty.");
                return itemCodes;
            }

            try
            {
                var dbSetting = userSession.DBSetting;

                using (SqlConnection conn = new SqlConnection(dbSetting.ConnectionString))
                {
                    conn.Open();
                    
                    string query = exactMatch
                        ? "SELECT ItemCode FROM Item WHERE Description = @Description"
                        : "SELECT ItemCode FROM Item WHERE Description LIKE @Description";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Description", exactMatch ? description : $"%{description}%");

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                itemCodes.Add(reader.GetString(0));
                            }
                        }
                    }
                }

                Console.WriteLine($"Found {itemCodes.Count} item(s) matching description: {description}");
                return itemCodes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding items: {ex.Message}");
                return itemCodes;
            }
        }

        /// <summary>
        /// Gets item details including code, description, and other properties.
        /// </summary>
        /// <param name="userSession">Active user session</param>
        /// <param name="description">Item description to search for</param>
        /// <param name="exactMatch">If true, performs exact match; otherwise performs partial match</param>
        /// <returns>Item information if found, otherwise null</returns>
        public ItemInfo? GetItemByDescription(UserSession userSession, string description, bool exactMatch = false)
        {
            if (userSession == null || !userSession.IsLogin)
            {
                Console.WriteLine("Invalid or expired session.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                Console.WriteLine("Description cannot be null or empty.");
                return null;
            }

            try
            {
                var dbSetting = userSession.DBSetting;
                ItemInfo? itemInfo = null;

                using (SqlConnection conn = new SqlConnection(dbSetting.ConnectionString))
                {
                    conn.Open();
                    
                    string query = exactMatch
                        ? "SELECT TOP 1 ItemCode, Description, PurchaseUOM FROM Item WHERE Description = @Description"
                        : "SELECT TOP 1 ItemCode, Description, PurchaseUOM FROM Item WHERE Description LIKE @Description";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Description", exactMatch ? description : $"%{description}%");

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                itemInfo = new ItemInfo
                                {
                                    ItemCode = reader.GetString(0),
                                    Description = reader.GetString(1),
                                    PurchaseUOM = reader.IsDBNull(2) ? string.Empty : reader.GetString(2)
                                };
                                Console.WriteLine($"Item found: {itemInfo.ItemCode} - {itemInfo.Description}");
                            }
                            else
                            {
                                Console.WriteLine($"No item found with description: {description}");
                            }
                        }
                    }
                }

                return itemInfo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding item: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets all items that match the description with full details.
        /// </summary>
        /// <param name="userSession">Active user session</param>
        /// <param name="description">Item description to search for</param>
        /// <param name="exactMatch">If true, performs exact match; otherwise performs partial match</param>
        /// <returns>List of item information that match the description</returns>
        public List<ItemInfo> GetAllItemsByDescription(UserSession userSession, string description, bool exactMatch = false)
        {
            var items = new List<ItemInfo>();

            if (userSession == null || !userSession.IsLogin)
            {
                Console.WriteLine("Invalid or expired session.");
                return items;
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                Console.WriteLine("Description cannot be null or empty.");
                return items;
            }

            try
            {
                var dbSetting = userSession.DBSetting;

                using (SqlConnection conn = new SqlConnection(dbSetting.ConnectionString))
                {
                    conn.Open();
                    
                    string query = exactMatch
                        ? "SELECT ItemCode, Description, PurchaseUOM FROM Item WHERE Description = @Description ORDER BY ItemCode"
                        : "SELECT ItemCode, Description, PurchaseUOM FROM Item WHERE Description LIKE @Description ORDER BY ItemCode";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Description", exactMatch ? description : $"%{description}%");

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                items.Add(new ItemInfo
                                {
                                    ItemCode = reader.GetString(0),
                                    Description = reader.GetString(1),
                                    PurchaseUOM = reader.IsDBNull(2) ? string.Empty : reader.GetString(2)
                                });
                            }
                        }
                    }
                }

                Console.WriteLine($"Found {items.Count} item(s) matching description: {description}");
                return items;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding items: {ex.Message}");
                return items;
            }
        }
        public List<ItemData> GetItemData(UserSession userSession)
        {
            if (userSession == null || !userSession.IsLogin)
            {
                Console.WriteLine("Invalid or expired session.");
                return new List<ItemData>();
            }

            try
            {
                var dbSetting = userSession.DBSetting;
                var items = new List<ItemData>();

                using (SqlConnection conn = new SqlConnection(dbSetting.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT ItemCode, Description, PurchaseUOM FROM Item", conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new ItemData
                                {
                                    ItemCode = reader.GetString(0),
                                    Description = reader.GetString(1),
                                    PurchaseUOM = reader.GetString(2)
                                };
                                items.Add(item);
                            }
                        }
                    }
                }

                return items;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding Item: {ex.Message}");
                return new List<ItemData>();
            }
        }
    

    public List<ItemData> GetAltItemData(UserSession userSession)
        {
            if (userSession == null || !userSession.IsLogin)
            {
                Console.WriteLine("Invalid or expired session.");
                return new List<ItemData>();
            }

            try
            {
                var dbSetting = userSession.DBSetting;
                var items = new List<ItemData>();

                using (SqlConnection conn = new SqlConnection(dbSetting.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT ItemCode, SubCode, UOM FROM ItemSubCode", conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new ItemData
                                {
                                    ItemCode = reader.GetString(0),
                                    Description = reader.GetString(1),
                                    PurchaseUOM = reader.GetString(2)
                                };
                                items.Add(item);
                            }
                        }
                    }
                }

                return items;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding AltItem: {ex.Message}");
                return new List<ItemData>();
            }
        }
    }

    /// <summary>
    /// Represents item information retrieved from the database.
    /// </summary>
    public class ItemInfo
    {
        public string ItemCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PurchaseUOM { get; set; } = string.Empty;
    }

    public class ItemData
    {
        public string ItemCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PurchaseUOM { get; set; } = string.Empty;
    }
}
