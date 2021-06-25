using BasicModelOperations.Services;
using System;

namespace BasicModelOperations
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Basic Model Operations";
            CrudExample.Retrieve();
        }
    }
}
