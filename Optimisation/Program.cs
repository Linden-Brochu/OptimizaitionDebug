// See https://aka.ms/new-console-template for more information


using NodaTime;
using Optimisation;
using YahooQuotesApi;

var startDate = new LocalDate(2023, 1, 1);
var endDate = new LocalDate(2023, 2, 1);

var quotes = new[]
{
    "AAL",
    "DAL",
    "UAL",
    "LUV",
    "HA"
};

Simulation sim = new Simulation();
await sim.Simulate(quotes, "OFF", 30);
await sim.Simulate(new []{"GOOG", "AMZN", "META", "MSFT", "AAPL"}, "croesus-of-lydia", 3);