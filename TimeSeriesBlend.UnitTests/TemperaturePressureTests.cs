using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimeSeriesBlend.Core;

namespace TimeSeriesBlend.UnitTests
{
    [TestClass]
    public class TemperaturePressureTests
    {

        [TestMethod]
        public void FindMaxTemperature()
        {
            var vh = new TempPress();
            var volumeCalculator = new SeriesConnector<TempPress>(vh);        

            volumeCalculator
                .BeginPeriod("Temperature-Hours", StandardPeriods.Hour)
                    .Let("Temperature in Celsius", () => vh.TemperatureInCelsius)
                        .Assign((t) => TemperatureProvider.GetTemperature(t))
                    .End()
                    .Let("Absolute temperature in Kelvins", () => vh.TemperatureAbs)
                        .Assign(() => vh.TemperatureInCelsius + 273)
                    .End()
                    .Let("Pressure", () => vh.Pressure)
                        .Assign((t) => PressureProvider.GetPressure(t))
                    .End()
                    .Let("Volume of ballon", () => vh.Result)
                        .Assign(() => TempPress.C * vh.TemperatureAbs / vh.Pressure)
                        .Read((t, i) => Console.WriteLine($"{i:d2}  |   {t:dd.MM.yyyy}|    {vh.Result:n2}"))
                    .End()                    
               .EndPeriod();

            var c = SeriesConnector<TempPress>.Compile(volumeCalculator);
            c.Compute(new ComputationParameters
            {
                From = new DateTime(2000, 01, 01),
                Till = new DateTime(2000, 01, 02)
            });
                                
        }

        //[TestMethod]
        //public void T2()
        //{
        //    var g = new SeriesConnector<TempPress>(null);
        //    g
        //        .BeginPeriod("ddsfsd", null)
        //            .Let<string>("var1", null)
        //                .Assign<string>(() => "dfd")
        //                .Read(() => { })
        //                .Read(() => { })
        //            .End()
        //            .Let<int>("var2", null)
        //                .Assign<string>(() => "ddd")
        //            .End()
        //         .Summarize((DateTime d) => { })
        //         .BeginPeriod("ddd", null)
        //         .EndPeriod();

        //    g.BeginGroup("group1", (a) => null)
        //        .BeginPeriod(null)
        //            .Let<string>("var2", null)
        //                .Assign<string>(() => "ddd")
        //             .End()
        //        .EndPeriod()
        //        .BeginPeriod(null)
        //        .EndPeriod()
        //    .EndGroup() // group 1
        //    .BeginGroup("group2", () => null)
        //        .InPeriod("dfdf")
        //            .Let<int>("dd", null)
        //                .Assign<string>(() => "ddd")
        //                .Read(() => { })
        //            .End()
        //        .EndPeriod()
        //        .InPeriod("ccc")
        //        .EndPeriod()
        //        .InPeriod("ddd")
        //            .Let<int>("dd", null)
        //                .Assign<string>(() => "sss")
        //            .End()
        //        .EndPeriod()
        //    .EndGroup(); // group 2

        //    SeriesConnector<TempPress>.Compile(g);
        //    // var r = g.Compile();
        //    // r.Compute(null);
        //}
    }
}
