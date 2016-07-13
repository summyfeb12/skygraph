using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

public class Functions
{
    public int UploadFile(string Path, int BId)
    {
        try
        {
            DataTable dt_CustomerSalesData = new DataTable();
            DataTable dt_RetrievedTemp = new DataTable();

            using (CsvReader csv = new CsvReader(new StreamReader(@"C:\Files\PastWeather.csv"), true))
            {
                dt_RetrievedTemp.Load(csv);
            }

            using (CsvReader csv = new CsvReader(new StreamReader(Path), true))
            {
                dt_CustomerSalesData.Load(csv);
            }

            dt_CustomerSalesData.Columns.Add("Temperature");


            if (dt_CustomerSalesData != null && dt_RetrievedTemp != null && dt_RetrievedTemp.Rows.Count > 0 && dt_CustomerSalesData.Rows.Count > 0)
            {
                for (int i = 0; i < dt_CustomerSalesData.Rows.Count; i++)
                {
                    for (int j = 0; j < dt_RetrievedTemp.Rows.Count; j++)
                    {
                        if (Convert.ToDateTime(dt_CustomerSalesData.Rows[i]["Date"]) == Convert.ToDateTime(dt_RetrievedTemp.Rows[j]["date"]))
                        {
                            dt_CustomerSalesData.Rows[i]["Temperature"] = dt_RetrievedTemp.Rows[j]["temperature"];
                        }
                    }
                }

            }

            if (dt_CustomerSalesData != null && dt_CustomerSalesData.Rows.Count > 0)
            {
                for (int i = 0; i < dt_CustomerSalesData.Rows.Count; i++)
                {
                    DBLayer dbObj = new DBLayer();
                    dbObj.InsertSalesData(BId, Convert.ToDateTime(dt_CustomerSalesData.Rows[i]["Date"]), Convert.ToInt32(dt_CustomerSalesData.Rows[i]["Sales"]), Convert.ToInt32(dt_CustomerSalesData.Rows[i]["StaffOnDuty"]), Convert.ToInt32(dt_CustomerSalesData.Rows[i]["OpeningHours"]), Convert.ToInt32(dt_CustomerSalesData.Rows[i]["ClosingHours"]), Convert.ToInt32(dt_CustomerSalesData.Rows[i]["Temperature"]));
                }
            }
            else
            {
                return -2;
            }

        }
        catch (Exception ex)
        {
            return -1;
        }
        return 0;
    }

    public DataTable GetCompleteData(int businessId)
    {
        DataTable dt = new DataTable();
        try
        {
            DBLayer dbObj = new DBLayer();
            dt = dbObj.GetSalesData(businessId);
        }
        catch (Exception ex)
        {
            return null;
        }
        return dt;
    }
    
    public DataTable ConvertToDataTable<T>(IEnumerable<T> varlist)
    {
        DataTable dtReturn = new DataTable();

        // column names
        PropertyInfo[] oProps = null;

        if (varlist == null) return dtReturn;
        foreach (T rec in varlist)
        {
            // Use reflection to get property names, to create table, Only first time, others will follow
            if (oProps == null)
            {
                oProps = ((Type)rec.GetType()).GetProperties();
                foreach (PropertyInfo pi in oProps)
                {
                    Type colType = pi.PropertyType;


                    if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                    {
                        colType = colType.GetGenericArguments()[0];
                    }


                    dtReturn.Columns.Add(new DataColumn(pi.Name, colType));
                }
            }

            DataRow dr = dtReturn.NewRow();

            foreach (PropertyInfo pi in oProps)
            {
                dr[pi.Name] = pi.GetValue(rec, null) == null ? DBNull.Value : pi.GetValue
                (rec, null);
            }
            dtReturn.Rows.Add(dr);
        }
        return dtReturn;
    }
}
