using CanvasAPI;
using CanvasAPI.FluentAPI;
using System;

namespace sandbox
{
    class Example
    {
        public static void Main(string[] _)
        {
            var canvas = Start.Save().Draw().Draw().Save().Draw().Restore().Draw().Save().Draw().Draw().Restore().Done<Canvas>();
            foreach (var action in canvas) Console.WriteLine(action);
        }
    }
}
