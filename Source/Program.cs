using System;

namespace MusicToVideo
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("Welcome to MusicToVideo!");
            Console.WriteLine("A CLI tool for converting music to video");
            
            if (args.Length > 0)
            {
                Console.WriteLine($"Arguments provided: {string.Join(" ", args)}");
            }
            else
            {
                Console.WriteLine("No arguments provided. Use --help to see available options.");
            }
            
            return 0;
        }
    }
}
