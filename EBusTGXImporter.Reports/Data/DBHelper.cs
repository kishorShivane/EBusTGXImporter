using EBusTGXImporter.Reports.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace EBusTGXImporter.Reports.Data
{
    public static class DBHelper
    {
        public static string GetConnectionString(string connecKey)
        {
            return ConfigurationManager.ConnectionStrings[connecKey].ConnectionString;
        }

        public static SqlConnection GetConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public static DateTime ConvertStringToDateSaFormat(string dateTime)
        {
            if (dateTime.Contains("/"))
            {
                dateTime = dateTime.Replace('/', '-');
            }
            const string pattern = "dd-MM-yyyy";
            DateTime.TryParseExact(dateTime, pattern, null,
                                   DateTimeStyles.None, out DateTime parsedDate);
            return parsedDate;
        }

        public static string ToLittleHex(string userInput)
        {
            if (userInput.Length == 6)
            {
                userInput = "00" + userInput;
            }
            if (userInput.Length == 7)
            {
                userInput = "0" + userInput;
            }
            if (userInput.Length == 5)
            {
                userInput = "000" + userInput;
            }
            if (userInput.Length == 4)
            {
                userInput = "0000" + userInput;
            }
            if (userInput.Length == 2)
            {
                userInput = "0000000" + userInput;
            }
            int len = userInput.Length;
            if (len < 8) { return userInput; }
            return userInput.Substring(len - 2, 2) + userInput.Substring(len - 4, 2) + userInput.Substring(len - 6, 2) + userInput.Substring(len - 8, 2);
        }

        public static List<SmartCardTransaction> GetSmartCardTransactions(Filters filter)
        {
            SqlConnection myConnection = null;
            List<SmartCardTransaction> result = new List<SmartCardTransaction>();
            SmartCardTransaction trans = null;
            DataTable dt = new DataTable();
            try
            {
                using (myConnection = GetConnection(GetConnectionString(Helper.Configuration.CONNECTION_KEY)))
                {
                    myConnection.Open();
                    SqlCommand cmd = new SqlCommand("EbusSmartCardTrans", myConnection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@smartCardNumBi", Convert.ToInt64(filter.SerialNumber).ToString("X"));
                    cmd.Parameters.AddWithValue("@smartCardNumLi", ToLittleHex(Convert.ToInt64(filter.SerialNumber).ToString("X")));
                    cmd.Parameters.AddWithValue("@fromDate", ConvertStringToDateSaFormat(filter.FromDate).ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@toDate", ConvertStringToDateSaFormat(filter.ToDate).ToString("yyyy-MM-dd"));
                    cmd.CommandTimeout = 500000;

                    //SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);

                    //while (dr.Read())
                    //{
                    foreach (DataRow dr in dt.Rows)
                    {
                        trans = new SmartCardTransaction();

                        if (dr["ClassID"] != null && dr["ClassID"].ToString() != string.Empty)
                        {
                            trans.ClassID = Convert.ToInt32(dr["ClassID"].ToString());
                        }

                        if (dr["ClassName"] != null && dr["ClassName"].ToString() != string.Empty)
                        {
                            trans.ClassName = dr["ClassName"].ToString();
                        }

                        if (dr["NonRevenue"] != null && dr["NonRevenue"].ToString() != string.Empty)
                        {
                            trans.NonRevenue = dr["NonRevenue"].ToString();
                        }
                        if (dr["Revenue"] != null && dr["Revenue"].ToString() != string.Empty)
                        {
                            trans.Revenue = dr["Revenue"].ToString();
                        }
                        if (dr["TransTime"] != null && dr["TransTime"].ToString() != string.Empty)
                        {
                            trans.TransDate = DateTime.Parse(dr["TransTime"].ToString());
                            trans.TDate = trans.TransDate.ToString("dd/MM/yyyy");
                            trans.TTime = trans.TransDate.ToString("hh:mm tt");
                        }
                        if (dr["SerialNumber"] != null && dr["SerialNumber"].ToString() != string.Empty)
                        {
                            trans.SerialNumber = dr["SerialNumber"].ToString();
                        }
                        if (dr["SerialNumberHex"] != null && dr["SerialNumberHex"].ToString() != string.Empty)
                        {
                            trans.SerialNumberHex = dr["SerialNumberHex"].ToString();
                        }

                        if (dr["RouteID"] != null && dr["RouteID"].ToString() != string.Empty)
                        {
                            trans.RouteID = dr["RouteID"].ToString();
                        }
                        if (dr["JourneyID"] != null && dr["JourneyID"].ToString() != string.Empty)
                        {
                            trans.JourneyID = dr["JourneyID"].ToString();
                        }
                        if (dr["OperatorID"] != null && dr["OperatorID"].ToString() != string.Empty)
                        {
                            trans.OperatorID = dr["OperatorID"].ToString();
                        }
                        if (dr["OperatorName"] != null && dr["OperatorName"].ToString() != string.Empty)
                        {
                            trans.OperatorName = dr["OperatorName"].ToString();
                        }
                        if (dr["ETMID"] != null && dr["ETMID"].ToString() != string.Empty)
                        {
                            trans.ETMID = dr["ETMID"].ToString();
                        }
                        if (dr["BusID"] != null && dr["BusID"].ToString() != string.Empty)
                        {
                            trans.BusID = dr["BusID"].ToString();
                        }
                        if (dr["DutyID"] != null && dr["DutyID"].ToString() != string.Empty)
                        {
                            trans.DutyID = dr["DutyID"].ToString();
                        }

                        if (dr["ModuleID"] != null && dr["ModuleID"].ToString() != string.Empty)
                        {
                            trans.ModuleID = dr["ModuleID"].ToString();
                        }
                        if (dr["RevenueBalance"] != null && dr["RevenueBalance"].ToString() != string.Empty)
                        {
                            trans.RevenueBalance = dr["RevenueBalance"].ToString();
                        }
                        if (dr["TransDay"] != null && dr["TransDay"].ToString() != string.Empty)
                        {
                            trans.TransDay = dr["TransDay"].ToString();
                        }
                        if (dr["AmountRecharged"] != null && dr["AmountRecharged"].ToString() != string.Empty)
                        {
                            trans.AmountRecharged = dr["AmountRecharged"].ToString();
                        }
                        if (dr["TripsRecharged"] != null && dr["TripsRecharged"].ToString() != string.Empty)
                        {
                            trans.TripsRecharged = dr["TripsRecharged"].ToString();
                        }
                        result.Add(trans);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                myConnection.Close();
            }
            return result;
        }
    }
}