using System;

namespace Prx.Tests
{
    internal class Program
    {
        private static void Main()
        {
            /*
            VMTests c = new Prx.Tests.VMTests();
            c.SetupCompilerEngine();
            c.Currying();
            c.TeardownCompilerEngine();
            Console.WriteLine("\t\t**\tFinished"); Console.ReadLine(); 
            return; // */

            //*
            CompilerParser c = new CompilerParser();
            c.SetupCompilerEngine();
            c.DeDereference();
            c.TeardownCompilerEngine();
            Console.WriteLine("\t\t**\tFinished");
            Console.ReadLine();
            return; // */
        }
    }
}