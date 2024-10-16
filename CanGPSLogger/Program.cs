// See https://aka.ms/new-console-template for more information

using CanGPSLogger;
using NmeaParser;
using Peak.Can.Basic;
using Splat;
using Splat.Serilog;
using Serilog;


Console.WriteLine("Hello, World!");




Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
Locator.CurrentMutable.UseSerilogFullLogger();

CanReader canReader = new CanReader();
//GPSReader gPSReader = new GPSReader();


while (true) { 
    await Task.Delay(100);
}