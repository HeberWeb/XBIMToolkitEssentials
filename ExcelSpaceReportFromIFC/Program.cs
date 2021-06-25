using Services.ExcelSpaceReportFromIFC;
using System;

namespace ExcelSpaceReportFromIFC
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Excel Space Report From IFC";
            ExcelReportIFC.Export();
        }
    }
}
