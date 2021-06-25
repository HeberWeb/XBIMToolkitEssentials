using System;
using UsingLINQForOptimalPerformance.Services;

namespace UsingLINQForOptimalPerformance
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Using LINQ For Optimal Performance";
            TestsWithAndWithoutLINQ.SelectTypeSelection();
        }
    }
}
