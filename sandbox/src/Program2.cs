using AmbiguousAPI;
using AmbiguousAPI.FluentAPI;
using System;

namespace treetop
{
    public class Program2
    {
        public static void OtherMain(string[] _)
        {
            //Start.Done<Ambiguous>();
            Start.a().b().c().d().Done<Ambiguous>();
            Start.a().b().c().c().d().d().Done<Ambiguous>();
            Start.a().a().b().c().d().d().Done<Ambiguous>();
            //Start.a().a().b().c().c().d().d().Done<Ambiguous>();
            //Start.a().b().b().c().d().d().Done<Ambiguous>();
            Start //
                .a().a().a().a().a().a().a().a().a().a() //
                .a().a().a().a().a().a().a().a().a().a() //
                .a().a().a().a().a() //
                .b().b().b().b().b().b().b().b().b().b() //
                .b().b().b().b().b().b().b().b().b().b() //
                .b().b().b().b().b() //
                .c().c().c().c().c().c().c().c().c().c() //
                .c().c().c().c().c().c().c().c().c().c() //
                .c().c().c().c().c() //
                .d().d().d().d().d().d().d().d().d().d() //
                .d().d().d().d().d().d().d().d().d().d() //
                .d().d().d().d().d() //
                .Done<Ambiguous>();
            Start //
                .a().a().a().a().a().a().a().a().a().a() //
                .a().a().a().a().a().a().a().a().a().a() //
                .a().a().a().a().a() //
                .b().b().b().b().b().b().b().b().b().b() //
                .b().b().b().b().b().b().b().b().b().b() //
                .b().b().b().b().b() //
                .c().c().c().c().c().c().c().c().c().c() //
                .c().c().c().c().c().c().c().c().c().c() //
                .c().c().c().c().c() //
                .d().d().d().d().d().d().d().d().d().d() //
                .d().d().d().d().d().d().d().d().d().d() //
                .d().d().d().d().d() //
                .Done<Ambiguous>();
            Start //
                .a().a().a().a().a().a().a().a().a().a() //
                .a().a().a().a().a().a().a().a().a().a() //
                .a().a().a().a().a() //
                .b().b().b().b().b().b().b().b().b().b() //
                .b().b().b().b().b().b().b().b().b().b() //
                .b().b().b().b().b() //
                .c().c().c().c().c().c().c().c().c().c() //
                .c().c().c().c().c().c().c().c().c().c() //
                .c().c().c().c().c() //
                .d().d().d().d().d().d().d().d().d().d() //
                .d().d().d().d().d().d().d().d().d().d() //
                .d().d().d().d().d() //
                .Done<Ambiguous>();
            Start //
                .a().a().a().a().a().a().a().a().a().a() //
                .a().a().a().a().a().a().a().a().a().a() //
                .a().a().a().a().a() //
                .b().b().b().b().b().b().b().b().b().b() //
                .b().b().b().b().b().b().b().b().b().b() //
                .b().b().b().b().b() //
                .c().c().c().c().c().c().c().c().c().c() //
                .c().c().c().c().c().c().c().c().c().c() //
                .c().c().c().c().c() //
                .d().d().d().d().d().d().d().d().d().d() //
                .d().d().d().d().d().d().d().d().d().d() //
                .d().d().d().d().d() //
                .Done<Ambiguous>();
            Start //
                .a().a().a().a().a().a().a().a().a().a() //
                .a().a().a().a().a().a().a().a().a().a() //
                .a().a().a().a().a() //
                .b().b().b().b().b().b().b().b().b().b() //
                .b().b().b().b().b().b().b().b().b().b() //
                .b().b().b().b().b() //
                .c().c().c().c().c().c().c().c().c().c() //
                .c().c().c().c().c().c().c().c().c().c() //
                .c().c().c().c().c() //
                .d().d().d().d().d().d().d().d().d().d() //
                .d().d().d().d().d().d().d().d().d().d() //
                .d().d().d().d().d() //
                .Done<Ambiguous>();
        }
    }
}
