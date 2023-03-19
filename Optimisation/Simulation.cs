using NodaTime;
using YahooQuotesApi;

namespace Optimisation;

public class Simulation
{
    public async Task Simulate(string[] names, string a, int maxDayLimit)
    {
        var startDate = new LocalDate(2023, 1, 1);
        var endDate = new LocalDate(2023, 2, 1);

        var quotes = names;

        var dates = new List<LocalDate>();

        for (var i = 0; i < 31; i++)
        {
            dates.Add(new LocalDate(2023, 1, i + 1));
        }

        var exceptDates = new List<LocalDate>(dates);

        YahooQuotes yahooQuotes = new YahooQuotesBuilder()
            .WithHistoryStartDate(Instant.FromUtc(startDate.Year, startDate.Month, startDate.Day, 0, 0))
            .Build();

        Dictionary<string,Security?> securities = await yahooQuotes.GetAsync(quotes, Histories.PriceHistory);

        if (securities.Any(s => s.Value is null))
        {
            throw new Exception();
        }

        var values = securities.Select(s =>
        {
            foreach (var priceTick in s.Value.PriceHistory.Value)
            {
                exceptDates.Remove(priceTick.Date);
            }
            var q = new Quote
            {
                Name = s.Key,
                Values = s.Value.PriceHistory.Value.Where(value => value.Date < endDate).Select(value => value.Open).ToArray()
            };
            return q;
        }).ToArray();

        dates = dates.Except(exceptDates).ToList();

        var profits = new List<RangeProfit>();

        Parallel.For(0, values.Length, (index) =>
        {
            Quote q;
            lock (quotes)
            {
                q = values[index];
            }

            for (var i = 0; i < q.Values.Length - 1; i++)
            {
                LocalDate d;
                lock (dates)
                {
                    d = dates[i];
                }

                var initialPrice = q.Values[i];
                var highers = q.Values.Select((p, priceIndex) => (p, priceIndex)).Where(p => p.p > initialPrice && d < dates[p.priceIndex]).ToArray();
                lock (profits)
                {
                    profits.AddRange(highers.Select(price => new RangeProfit
                    {
                        EndDate = dates[price.priceIndex],
                        Name = q.Name,
                        StartDate = d,
                        Profit = price.p - initialPrice,
                    }));
                }
            }
        });

        var currentDate = dates[0];
        var actions = new List<CroesusApiAction>();

        while (currentDate < endDate)
        {
            var profit = profits.Where(p => p.StartDate == currentDate && p.StartDate.PlusDays(maxDayLimit) >= p.EndDate).MaxBy(p => p.Profit / (p.EndDate.Day - p.StartDate.Day));
            if (profit is not null)
            {
                actions.Add(new CroesusApiAction
                {
                    action = "BUY",
                    date = profit.StartDate.ToString("yyyy-MM-dd", null),
                    ticker = profit.Name
                });
                actions.Add(new CroesusApiAction
                {
                    action = "SELL",
                    date = profit.EndDate.ToString("yyyy-MM-dd", null),
                    ticker = profit.Name
                });

                currentDate = profit.EndDate.PlusDays(1);
            }
            else
            {
                currentDate = currentDate.PlusDays(1);
            }
    
        }

        var response = await CroesusApi.PostCroesusValidation(actions, a);

        Console.WriteLine(a);
        StreamReader s = new StreamReader(response.Content.ReadAsStream());
        Console.WriteLine(s.ReadToEnd());
    }
}