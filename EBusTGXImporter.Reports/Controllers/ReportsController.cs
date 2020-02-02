using EBusTGXImporter.Reports.Data;
using EBusTGXImporter.Reports.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Mvc;

namespace EBusTGXImporter.Reports.Controllers
{
    public class ReportsController : Controller
    {
        //
        // GET: /Reports/

        public ActionResult SmartCardTransaction()
        {
            ViewBag.Page = "SmartCard";
            Filters filter = new Filters();
            return View(filter);
        }

        [HttpPost]
        public FileResult SmartCardTransaction(Filters filter)
        {
            List<SmartCardTransaction> list = DBHelper.GetSmartCardTransactions(filter);
            string htmlContent = GetHtmlContent(list, filter);

            switch (filter.FileType)
            {
                case "PDF":
                    return ExportToPDF(htmlContent);
                default:
                    return File(Encoding.ASCII.GetBytes(htmlContent), "application/vnd.ms-excel", "SmartCardTransaction.xls");
            }
        }

        protected FileResult ExportToPDF(string htmlContent)
        {
            using (MemoryStream stream = new System.IO.MemoryStream())
            {
                StringReader sr = new StringReader(htmlContent);
                Document pdfDoc = new Document(PageSize.LETTER, 15f, 15f, 100f, 10f);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();
                XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                pdfDoc.Close();
                return File(stream.ToArray(), "application/pdf", "SmartCardTransaction.pdf");
            }
        }

        private string GetHtmlContent(List<SmartCardTransaction> list, Filters filter)
        {
            StringBuilder htmlContent = new StringBuilder();
            htmlContent.Append("<HTML>");
            htmlContent.Append("<Body>");
            htmlContent.Append("<TABLE border='1' cellpadding='4'>");
            htmlContent.Append("<TBODY>");
            if (filter.FileType == "PDF")
            {
                htmlContent.Append("<TR>");
                htmlContent.Append("<TD colspan='10' style='text-align:center;'> Smart Card Transaction </TD><TD rowspan='4' align='center'> <img src='http://41.76.215.229/EbusBackOffice/Images/logo_comp1.png' height=80 width=80></img></TD>");
                htmlContent.Append("</TR>");
                htmlContent.Append("<TR>");
                htmlContent.Append("<TD colspan='10' style='text-align:center;'> Algoa Bus Company </TD>");
                htmlContent.Append("</TR>");
                htmlContent.Append("<TR>");
                htmlContent.Append("<TD colspan='10'>Date Range: " + filter.FromDate + " to " + filter.ToDate + "</TD>");
                htmlContent.Append("</TR>");
                htmlContent.Append("<TR>");
                htmlContent.Append("<TD colspan='10'>Smart Card: " + filter.SerialNumber + " </TD>");
                htmlContent.Append("</TR>");
            }
            else
            {
                htmlContent.Append("<TR>");
                htmlContent.Append("<TD colspan='11' style='text-align:center;'> Smart Card Transaction </TD>");
                htmlContent.Append("</TR>");
                htmlContent.Append("<TR>");
                htmlContent.Append("<TD colspan='11' style='text-align:center;'> Algoa Bus Company </TD>");
                htmlContent.Append("</TR>");
                htmlContent.Append("<TR>");
                htmlContent.Append("<TD colspan='11'>Date Range: " + filter.FromDate + " to " + filter.ToDate + "</TD>");
                htmlContent.Append("</TR>");
                htmlContent.Append("<TR>");
                htmlContent.Append("<TD colspan='11'>Smart Card: " + filter.SerialNumber + " </TD>");
                htmlContent.Append("</TR>");
            }

            htmlContent.Append("<TR>");
            htmlContent.Append("<TH>Type</TH>");
            htmlContent.Append("<TH>Duty</TH>");
            htmlContent.Append("<TH>Route</TH>");
            htmlContent.Append("<TH>Driver</TH>");
            htmlContent.Append("<TH>BusID</TH>");
            htmlContent.Append("<TH>Date</TH>");
            htmlContent.Append("<TH>Time</TH>");
            htmlContent.Append("<TH>Revenue</TH>");
            htmlContent.Append("<TH>Non Revenue</TH>");
            htmlContent.Append("<TH>Recharge Quantity</TH>");
            htmlContent.Append("<TH>Revenue Balance</TH>");
            htmlContent.Append("</TR>");
            foreach (SmartCardTransaction item in list)
            {
                htmlContent.Append("<TR>");
                htmlContent.Append("<TD>" + item.ClassName + "</TD>");
                htmlContent.Append("<TD>" + item.DutyID + "</TD>");
                htmlContent.Append("<TD>" + item.RouteID + "</TD>");
                htmlContent.Append("<TD>" + item.OperatorID + "</TD>");
                htmlContent.Append("<TD>" + item.BusID + "</TD>");
                htmlContent.Append("<TD>" + item.TDate + "</TD>");
                htmlContent.Append("<TD>" + item.TTime + "</TD>");
                htmlContent.Append("<TD>" + string.Format("{0:0.00}", Convert.ToDecimal(item.Revenue)) + "</TD>");
                htmlContent.Append("<TD>" + string.Format("{0:0.00}", Convert.ToDecimal(item.NonRevenue)) + "</TD>");
                htmlContent.Append("<TD>" + (item.ClassID.ToString() == "733" ? "0" : string.Format("{0:0.00}", Convert.ToDecimal(item.Revenue))) + "</TD>");
                htmlContent.Append("<TD>" + string.Format("{0:0.00}", Convert.ToDecimal(item.RevenueBalance)) + "</TD>");
                htmlContent.Append("</TR>");
            }
            htmlContent.Append("</TBODY>");
            htmlContent.Append("</TABLE>");
            htmlContent.Append("<P></P>");
            htmlContent.Append("<P style='text-align:right'>" + DateTime.Now + "</P>");
            htmlContent.Append("</Body>");
            htmlContent.Append("</HTML>");
            return htmlContent.ToString();
        }
    }
}
