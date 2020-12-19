using CanvasAPI;
using CanvasAPI.FluentAPI;
using System;

namespace sandbox
{
	class Example
    {
        public static void Main(string[] _)
        {
            var canvas = Start.Draw().Draw().Save().Draw().Restore().Draw().Save().Draw().Draw().Done<Canvas>();
            foreach (var action in canvas) Console.WriteLine(action);
        }
    }
}
