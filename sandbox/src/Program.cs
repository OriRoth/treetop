using BalancedAPI;
using BalancedAPI.FluentAPI;
using System;

namespace treetop
{
    public class Program
    {
        public static void OtherMain(string[] _)
        {
            var values = Start.Open().Close().Open().Open().Close().Open().Close().Close().Done<Balanced>();
            Start.Open().Open().Open().Close().Close().Close().Open().Close().Done<Balanced>();
            foreach (var value in values)
                Console.WriteLine(value);
        }
    }
}
