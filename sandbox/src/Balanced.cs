using BalancedAPI.FluentAPI;
using System;

namespace treetop
{
    public class Balanced
    {
        public static void Main(string[] _)
        {
            var values = Start.Open().Close().Open().Open().Close().Open().Close().Close().Done<BalancedAPI.Balanced>();
            Start.Open().Open().Open().Close().Close().Close().Open().Close().Done<BalancedAPI.Balanced>();
            foreach (var value in values)
                Console.WriteLine(value);
        }
    }
}
