using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;
using System.Data;
using System.Web.Helpers;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using LumenWorks.Framework.IO.Csv;
using System.Net.Mail;
using System.Net;
using ClosedXML.Excel;

namespace SkyGraphNG.Controllers
{
    public class ServicesController : Controller
    {
        static string message = "";
        static DataTable datatable = new DataTable();
        static int BId;
        int returnValue;

        // GET: Upload
        public ActionResult Index()
        {
            if(Request.Cookies.Count > 0)
            {
                BId = Convert.ToInt32(Request.Cookies.Get("BId").Value);
                DrawCharts();
                DrawPredictionCharts();

                ViewBag.Message = message;
                ViewBag.Data = datatable;
                message = "";
            }
            return View();
        }

        public void UploadFile()
        {
            if (Request.Files.Count > 0)
            {
                //message = "Please wait till the file gets submitted";
                var file = Request.Files[0];
                var fileName = Path.GetFileName(file.FileName);

                if (file != null && file.ContentLength > 0 && Path.GetExtension(fileName) == ".csv")
                {
                    var path = Path.Combine(Server.MapPath("~/Files/"), fileName);
                    file.SaveAs(path);
                    Functions f = new Functions();
                    returnValue = f.UploadFile(path, BId);
                }
            }
            if (returnValue == -1)
                message = "Error: Unable to upload file!";
            else if (returnValue == -2)
                message = "Error: Internal error!";
            else
            {
                message = "Success: Sales data uploaded successfully and you can view the data";
                DrawCharts();
                DrawPredictionCharts();
            }
            Response.Redirect("/Services");
        }

        public void DrawCharts()
        {
            
            Functions f = new Functions();

            DataTable dt = f.GetCompleteData(BId);
            DataTable dtCloned = dt.Clone();
            dtCloned.Columns["Temperature"].DataType = typeof(Int32);
            dtCloned.Columns["Sales"].DataType = typeof(Int32);
            foreach (DataRow row in dt.Rows)
            {
                dtCloned.ImportRow(row);
            }

            var query = from r in dtCloned.AsEnumerable()
                        group r by r.Field<int>("Temperature") into groupedTable
                        select new
                        {
                            Temperature = groupedTable.Key,
                            AvgSales = groupedTable.Average(s => s.Field<int>("Sales"))
                        };
            DataTable dt1 = f.ConvertToDataTable(query);

            List<string> xAxis = new List<string>();
            List<string> yAxis = new List<string>();

            foreach (DataRow row in dt1.Rows)
            {
                xAxis.Add(Convert.ToDecimal(row[0].ToString()).ToString("#.##"));
                yAxis.Add(Convert.ToDecimal(row[1].ToString()).ToString("#.##"));
            }

            var chart = new Chart(width: 600, height: 320)
                .AddSeries(
                            chartType: "column",
                            xValue: xAxis,
                            yValues: yAxis)
                            .GetBytes("png");

            using (Image image = Image.FromStream(new MemoryStream(chart)))
            {
                image.Save(Server.MapPath("~/Files/G1.png"), ImageFormat.Png);
            }

            var dtCloned_ = dt.Clone();
            dtCloned_.Columns["NoOfEmployee"].DataType = typeof(Int32);
            dtCloned_.Columns["Sales"].DataType = typeof(Int32);
            foreach (DataRow row in dt.Rows)
            {
                dtCloned_.ImportRow(row);
            }

            var query1 = from r in dtCloned_.AsEnumerable()
                        group r by r.Field<int>("NoOfEmployee") into groupedTable
                        select new
                        {
                            NoOfEmployee = groupedTable.Key,
                            AvgSales = groupedTable.Average(s => s.Field<int>("Sales"))
                        };
            DataTable dt2 = f.ConvertToDataTable(query1);

            xAxis.Clear();
            yAxis.Clear();

            foreach (DataRow row in dt2.Rows)
            {
                xAxis.Add(Convert.ToDecimal(row[0].ToString()).ToString("#.##"));
                yAxis.Add(Convert.ToDecimal(row[1].ToString()).ToString("#.##"));
            }

            chart = new Chart(width: 600, height: 320)
               .AddSeries(
                           chartType: "column",
                           xValue: xAxis,
                           yValues: yAxis)
                           .GetBytes("png");
            using (Image image = Image.FromStream(new MemoryStream(chart)))
            {
                image.Save(Server.MapPath("~/Files/G2.png"), ImageFormat.Png);
            }
        }

        public void DrawPredictionCharts()
        {
            DataTable dt = PredictFunction();
            List<string> xAxis = new List<string>();
            List<string> yAxis = new List<string>();
            List<string> xAxis1 = new List<string>();
            List<string> yAxis1 = new List<string>();
            datatable = dt;

            foreach (DataRow row in dt.Rows)
            {
                xAxis.Add(Convert.ToDateTime(row["date"].ToString()).ToShortDateString());
                yAxis.Add(Convert.ToDecimal(row["sales"].ToString()).ToString("#.##"));

                xAxis1.Add(Convert.ToDecimal(row["sales"].ToString()).ToString("#.##"));
                yAxis1.Add(Convert.ToDecimal(row["workforce"].ToString()).ToString("#.##"));
            }

            var chart = new Chart(width: 600, height: 320)
                .AddSeries(
                            chartType: "column",
                            xValue: xAxis,
                            yValues: yAxis)
                            .GetBytes("png");
            

            using (Image image = Image.FromStream(new MemoryStream(chart)))
            {
                image.Save(Server.MapPath("~/Files/G3.png"), ImageFormat.Png);
            }

            chart = new Chart(width: 600, height: 320)
               .AddSeries(
                           chartType: "column",
                           xValue: xAxis1,
                           yValues: yAxis1)
                           .GetBytes("png");
            using (Image image = Image.FromStream(new MemoryStream(chart)))
            {
                image.Save(Server.MapPath("~/Files/G4.png"), ImageFormat.Png);
            }
        }

        public void Export()
        {
            string attachment = "attachment; filename=FinalReport.xls";
            Response.ClearContent();
            Response.AddHeader("content-disposition", attachment);
            Response.ContentType = "application/vnd.ms-excel";
            string tab = "";
            foreach (DataColumn dc in datatable.Columns)
            {
                Response.Write(tab + dc.ColumnName);
                tab = "\t";
            }
            Response.Write("\n");
            int j;
            foreach (DataRow dr in datatable.Rows)
            {
                tab = "";
                for (j = 0; j < datatable.Columns.Count; j++)
                {
                    Response.Write(tab + dr[j].ToString());
                    tab = "\t";
                }
                Response.Write("\n");
            }
            Response.End();
        }

        public void SendEmail()
        {
            XLWorkbook wb = new XLWorkbook();
            wb.Worksheets.Add(datatable, "Report");
            wb.SaveAs(@"C:\Files\Report.xlsx");

            using (MailMessage mm = new MailMessage("skygraph123.1992@gmail.com", Request.Cookies.Get("Email").Value))
            {
                mm.Body = "Sky Graph Email report";
                mm.Subject = "Final Report";

                mm.Attachments.Add(new Attachment(@"C:\Files\Report.xlsx"));
                mm.IsBodyHtml = false;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                NetworkCredential NetworkCred = new NetworkCredential("skygraphservices@gmail.com", "skygraph123");
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = NetworkCred;
                smtp.Port = 587;
                smtp.Send(mm);
                //ClientScript.RegisterStartupScript(GetType(), "alert", "alert('Email sent.');", true);
                message = "Success: Mail sent to your id";
                Response.Redirect("/Services");
            }
        }

        private DataTable PredictFunction()
        {
            Functions f = new Functions();
            DataTable dt = f.GetCompleteData(BId);
            if (dt != null && dt.Rows.Count > 0)
            {
                int minTemp = Convert.ToInt32(dt.Compute("min(Temperature)", string.Empty));
                int maxTemp = Convert.ToInt32(dt.Compute("max(Temperature)", string.Empty));

                int salesForMinTemp = Convert.ToInt32(dt.Compute("Avg(Sales)", "Temperature = " + minTemp.ToString()));
                int salesForMaxTemp = Convert.ToInt32(dt.Compute("Avg(Sales)", "Temperature = " + maxTemp.ToString()));

                int minStartBHour = Convert.ToInt32(dt.Compute("min(OpeningHours)", string.Empty));
                int maxCloseBHour = Convert.ToInt32(dt.Compute("max(OpeningHours)", string.Empty));


                int minSales = Convert.ToInt32(dt.Compute("min(Sales)", string.Empty));
                int maxSales = Convert.ToInt32(dt.Compute("max(Sales)", string.Empty));
                int staffForMinSales = Convert.ToInt32(dt.Compute("Avg(NoOfEmployee)", "Sales = " + minSales.ToString()));
                int staffForMaxSales = Convert.ToInt32(dt.Compute("Avg(NoOfEmployee)", "Sales = " + maxSales.ToString()));
                int startHourForMinSales = Convert.ToInt32(dt.Compute("Avg(OpeningHours)", "Sales = " + minSales.ToString()));
                int startHourForMaxSales = Convert.ToInt32(dt.Compute("Avg(OpeningHours)", "Sales = " + maxSales.ToString()));
                int closeHourForMinSales = Convert.ToInt32(dt.Compute("Avg(ClosingHours)", "Sales = " + minSales.ToString()));
                int closeHourForMaxSales = Convert.ToInt32(dt.Compute("Avg(ClosingHours)", "Sales = " + maxSales.ToString()));


                float slope = (float)(salesForMaxTemp - salesForMinTemp) / (maxTemp - minTemp);
                float lamda = (float)salesForMinTemp - (minTemp * slope);

                float slope_staff = (float)(staffForMaxSales - staffForMinSales) / (maxSales - minSales);
                float lamda_staff = (float)staffForMinSales - (minSales * slope_staff);

                float slope_startHours = (float)(startHourForMaxSales - startHourForMinSales) / (maxSales - minSales);
                float lamda_startHours = (float)startHourForMinSales - (minSales * slope_startHours);

                float slope_closeHours = (float)(closeHourForMaxSales - closeHourForMinSales) / (maxSales - minSales);
                float lamda_closeHours = (float)closeHourForMinSales - (minSales * slope_closeHours);

                Process myProcess = new Process();


                myProcess.StartInfo.UseShellExecute = true;

                myProcess.StartInfo.FileName = @"C:\Python27\Predictweather.bat";
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                myProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(@"C:\Python27\Predictweather.bat");

                myProcess.Start();
                while (!myProcess.HasExited)
                {
                    System.Threading.Thread.Sleep(1000);
                }
                DataTable dt_Predicted = new DataTable();
                using (CsvReader csv = new CsvReader(new StreamReader(@"C:\Python27\FUT.csv"), true))
                {
                    dt_Predicted.Load(csv);
                }
                dt_Predicted.Columns.Add("Sales");
                dt_Predicted.Columns.Add("WorkForce");
                dt_Predicted.Columns.Add("OpeningHours");
                dt_Predicted.Columns.Add("ClosingHours");
                if (dt_Predicted != null && dt_Predicted.Rows.Count > 0)
                {
                    for (int i = 0; i < dt_Predicted.Rows.Count; i++)
                    {
                        dt_Predicted.Rows[i]["Sales"] = Convert.ToInt32((slope * Convert.ToInt32(dt_Predicted.Rows[i]["temperature"])) + lamda);
                        dt_Predicted.Rows[i]["WorkForce"] = Convert.ToInt32((slope_staff * Convert.ToInt32(dt_Predicted.Rows[i]["Sales"])) + lamda_staff);

                        dt_Predicted.Rows[i]["OpeningHours"] = Convert.ToInt32((slope_startHours * Convert.ToInt32(dt_Predicted.Rows[i]["Sales"])) + lamda_startHours);
                        dt_Predicted.Rows[i]["ClosingHours"] = Convert.ToInt32((slope_closeHours * Convert.ToInt32(dt_Predicted.Rows[i]["Sales"])) + lamda_closeHours);

                    }
                }
                return dt_Predicted;
            }
            return dt;
        }
    }
}