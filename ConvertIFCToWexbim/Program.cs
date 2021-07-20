using ConvertIFCToWexbim.Services;
using System;

namespace ConvertIFCToWexbim
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "ConvertIFC To Wexbim";
            IFCToWexbim.Execute();
        }
    }
}
