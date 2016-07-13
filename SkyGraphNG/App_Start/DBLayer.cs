using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

/// <summary>
/// Summary description for DBLayer
/// </summary>
public class DBLayer
{
    string conStr;
    public DBLayer()
    {
        conStr = ConfigurationManager.ConnectionStrings["SkyGraphConnectionString"].ConnectionString;
    }

    public int SignUp(string firstName, string lastName, string userId, string password, string city, int zip,out int result)
    {
        SqlConnection conObj = new SqlConnection(conStr);
        try
        {
            int businessId = 0;
            int ret = VerifyLogin(userId, password, "SignUp", out businessId);
            if (businessId == 0)
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conObj;
                cmd.CommandText = "usp_InsertUserDetails";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Password", password);
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@LastName", lastName);
                cmd.Parameters.AddWithValue("@City", city);
                cmd.Parameters.AddWithValue("@Zip", zip);
                SqlParameter prmObj = new SqlParameter("@ret", SqlDbType.Int);
                prmObj.Direction = ParameterDirection.Output;

                cmd.Parameters.Add(prmObj);
                conObj.Open();
                ret = cmd.ExecuteNonQuery();
                result = Convert.ToInt32(prmObj.Value);
                if (ret == 1)
                {
                    return 1;
                }
            }
            else
            {
                result = -100;
                return 0;
            }
        }
        catch (Exception ex)
        {
            result = -100;
            return -1;
        }
        finally
        {
            conObj.Close();
        }
        return 0;
    }
    
    public int VerifyLogin(string userName, string password,string type, out int businessId)
    {
        SqlConnection conObj = new SqlConnection(conStr);
        businessId = -1;
        try
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conObj;
            cmd.CommandText = "usp_VerifyDetails";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@username", userName.Trim());
            cmd.Parameters.AddWithValue("@pwd", password.Trim());
            cmd.Parameters.AddWithValue("@verificationType", type.Trim());
            SqlParameter prmObj = new SqlParameter("@Bid", SqlDbType.Int);
            prmObj.Direction = ParameterDirection.Output;

            cmd.Parameters.Add(prmObj);
            conObj.Open();
            int ret = Convert.ToInt32(cmd.ExecuteScalar());
            businessId = Convert.ToInt32(prmObj.Value);
            return ret;
        }
        catch (Exception ex)
        {
            return -1;
        }
        finally
        {
            conObj.Close();
        }
    }
    public int InsertSalesData(int businessId, DateTime salesDate, int sales, int staffOnDuty, int openingHours, int closingHours, int temperature)
    {
        SqlConnection conObj = new SqlConnection(conStr);
        try
        {
            SqlCommand cmdObj = new SqlCommand("usp_SalesData", conObj);
            cmdObj.CommandType = CommandType.StoredProcedure;
            cmdObj.Parameters.AddWithValue("@BusinessId", businessId);
            cmdObj.Parameters.AddWithValue("@SalesDate", salesDate);
            cmdObj.Parameters.AddWithValue("@Sales", sales);
            cmdObj.Parameters.AddWithValue("@NoOfStaff", staffOnDuty);
            cmdObj.Parameters.AddWithValue("@OpeningHours", openingHours);
            cmdObj.Parameters.AddWithValue("@ClosingHours", closingHours);
            cmdObj.Parameters.AddWithValue("@Temp", temperature);
            cmdObj.Parameters.AddWithValue("@Type", "Insert");

            conObj.Open();
            int res = cmdObj.ExecuteNonQuery();
            if (res > 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        catch (Exception ex)
        {
            return -1;
        }
        finally
        {
            conObj.Close();
        }
    }
    public DataTable GetSalesData(int businessId)
    {
        SqlConnection conObj = new SqlConnection(conStr);
        DataTable dt=new DataTable();
        try
        {
            SqlCommand cmdObj = new SqlCommand("usp_SalesData", conObj);
            cmdObj.CommandType = CommandType.StoredProcedure;
            cmdObj.Parameters.AddWithValue("@BusinessId", businessId);
            cmdObj.Parameters.AddWithValue("@Type","Get");

            SqlDataAdapter da = new SqlDataAdapter(cmdObj);
            da.Fill(dt);
        }
        catch (Exception ex)
        {
            dt = null;
        }
        return dt;
    }
}