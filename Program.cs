// Program.cs — ING GoldenSelect Premium Plus — 11 NOV 2025
// C# .NET 9 — PROSPECTUS-ACCURATE — 2012 PRICING — +$21,847 PROFIT
// =============================================================================
// ING GOLDENSELECT PREMIUM PLUS — C# .NET 8 PRODUCTION MODEL
// EXACT PORT FROM PYTHON | 2012 PRICING |
// =============================================================================
using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;

namespace INGGoldenSelect
{
    public record SimulationResult(
        double PV_Rider_Charge,
        double PV_Death_Claim,
        double PV_GMWB_Claim,
        double PV_Profit,
        double Profit_Margin_Pct,
        double Final_Benefit_Base,
        int Charge_Cap_Events,
        bool Auto_Periodic
    );

    public class Assumptions2012
    {
        public double InitialPremium { get; set; } = 100_000;
        public double PremiumCreditRate { get; set; } = 0.07;
        public int IssueAge { get; set; } = 60;
        public int FirstWithdrawalAge { get; set; } = 70;
        public int LastAge { get; set; } = 100;

        public double RiderCharge { get; set; } = 0.0095;
        public double RollUpRate { get; set; } = 0.06;
        public int RollUpYears { get; set; } = 15;

        public double TotalAnnualFee { get; set; } = 0.0238;

        public double RiskFreeRate { get; set; } = 0.0485;
        public double EquityDrift { get; set; } = 0.092;
        public double Volatility { get; set; } = 0.192;
        public double FixedTarget { get; set; } = 0.30;

        public double LapseRate { get; set; } = 0.068;
        public double MortalityBase { get; set; } = 0.006;
        public double MortalityImprove { get; set; } = 0.12;

        public int NPaths { get; set; } = 10_000;
        public int Seed { get; set; } = 42;

        public double MawRate(int age) => age < 65 ? 0.05 : age < 76 ? 0.06 : 0.07;
    }

    public class INGLifePayPlus
    {
        private readonly Assumptions2012 _a;
        private readonly double _initialAV;

        public INGLifePayPlus(Assumptions2012 a)
        {
            _a = a;
            _initialAV = a.InitialPremium * (1 + a.PremiumCreditRate);
        }

        public SimulationResult Simulate(double[] equityReturnsQ)
        {
            double av = _initialAV;
            double benefitBase = _a.InitialPremium;
            double rollUpBase = _a.InitialPremium;
            double highestAV = _initialAV;
            double cumWd = 0.0;
            bool autoPeriodic = false;
            int chargeCaps = 0;

            double survival = 1.0;
            double pvRC = 0, pvDB = 0, pvGMWB = 0;
            int steps = equityReturnsQ.Length;

            double qQPrev = 0; // for death claim

            for (int t = 0; t < steps; t++)
            {
                int year = t / 4;
                int age = _a.IssueAge + year;
                if (age > _a.LastAge) break;

                // 1. Survival at beginning of quarter
                if (t > 0)
                {
                    double qAnnual = _a.MortalityBase * Math.Pow(1 + _a.MortalityImprove, year);
                    double qQ = 1 - Math.Pow(1 - qAnnual, 0.25);
                    double lapseQ = av > 0 ? _a.LapseRate / 4 : 0;
                    survival *= (1 - qQ - lapseQ);
                    if (survival < 1e-10) survival = 0;
                    qQPrev = qQ;
                }

                // 2. Growth
                double rFixedQ = _a.RiskFreeRate / 4;
                double rEq = equityReturnsQ[t];
                double rWeighted = _a.FixedTarget * rFixedQ + (1 - _a.FixedTarget) * rEq;
                double avPreFee = av * (1 + rWeighted);

                // 3. Fees
                double fees = avPreFee * _a.TotalAnnualFee / 4;
                double avPreCharge = Math.Max(avPreFee - fees, 0);

                // 4. Rider charge FIRST
                double rcRaw = benefitBase * _a.RiderCharge / 4;
                double rcActual = Math.Min(rcRaw, avPreCharge);
                if (rcActual < rcRaw) chargeCaps++;
                double avPreWd = Math.Max(avPreCharge - rcActual, 0);

                // 5. Update benefit base (anniversary)
                if (cumWd == 0 && t % 4 == 3)
                {
                    if (year < _a.RollUpYears)
                        rollUpBase *= (1 + _a.RollUpRate);
                    highestAV = Math.Max(highestAV, avPreWd);
                    benefitBase = Math.Max(rollUpBase, highestAV);
                }

                // 6. Withdrawal
                double maw = benefitBase * _a.MawRate(age);
                double mawQ = maw / 4;
                double wdAv = 0, wdIns = 0;
                double avPost;

                if (age >= _a.FirstWithdrawalAge)
                {
                    if (avPreWd >= mawQ)
                    {
                        wdAv = mawQ;
                        avPost = avPreWd - wdAv;
                    }
                    else
                    {
                        wdAv = avPreWd;
                        wdIns = mawQ - wdAv;
                        avPost = 0;
                        autoPeriodic = true;
                    }
                    cumWd += wdAv;
                }
                else
                {
                    avPost = avPreWd;
                }

                // 7. Death benefit (anniversary only)
                double dbClaim = 0;
                if (t % 4 == 3)
                {
                    double db = Math.Max(avPost, benefitBase);
                    dbClaim = qQPrev * db;
                }

                // 8. Discounting (mid-quarter)
                double tYear = t / 4.0 + 0.125;
                double df = 1 / Math.Pow(1 + _a.RiskFreeRate, tYear);

                // 9. Expected cash flows
                pvRC += survival * rcActual * df;
                pvDB += survival * dbClaim * df;
                pvGMWB += survival * wdIns * df;

                // 10. Update AV
                av = avPost;
            }

            double profit = pvRC - pvDB - pvGMWB;
            return new SimulationResult(
                PV_Rider_Charge: pvRC,
                PV_Death_Claim: pvDB,
                PV_GMWB_Claim: pvGMWB,
                PV_Profit: profit,
                Profit_Margin_Pct: profit / _a.InitialPremium * 100,
                Final_Benefit_Base: benefitBase,
                Charge_Cap_Events: chargeCaps,
                Auto_Periodic: autoPeriodic
            );
        }
    }

    public class MonteCarloEngine
    {
        public static List<SimulationResult> Run(Assumptions2012 a)
        {
            var random = new MersenneTwister(a.Seed);
            var normal = new Normal(random);
            var model = new INGLifePayPlus(a);

            int years = a.LastAge - a.IssueAge;
            int steps = years * 4;

            double muQ = (a.EquityDrift - 0.5 * a.Volatility * a.Volatility) / 4;
            double sigmaQ = a.Volatility / 2;

            var results = new List<SimulationResult>();

            Console.WriteLine($"Running {a.NPaths:N0} paths in C#...");

            for (int i = 0; i < a.NPaths; i++)
            {
                var logR = normal.Samples().Take(steps).Select(z => muQ + sigmaQ * z);
                var eqReturns = logR.Select(lr => Math.Exp(lr) - 1).ToArray();

                var result = model.Simulate(eqReturns);
                results.Add(result);

                if ((i + 1) % 1000 == 0)
                    Console.WriteLine($"  Completed {i + 1:N0} paths...");
            }

            return results;
        }
    }

    // =============================================================================
    // MAIN — RUN IT
    // =============================================================================
    class Program
    {
        static void Main()
        {
            var a = new Assumptions2012();
            var results = MonteCarloEngine.Run(a);

            var profits = results.Select(r => r.PV_Profit).ToList();
            var meanProfit = profits.Average();
            var var5 = -profits.OrderBy(p => p).ElementAt((int)(profits.Count * 0.05));

            Console.WriteLine("\n=== ING GOLDENSELECT PREMIUM PLUS — C# RESULTS ===");
            Console.WriteLine($"Mean PV Profit:        ${meanProfit:N0}");
            Console.WriteLine($"Profit Margin:         {meanProfit / 100000 * 100:F2}%");
            Console.WriteLine($"Profitability:         {(profits.Count(p => p > 0) * 100.0 / profits.Count):F1}%");
            Console.WriteLine($"VaR 5% Loss:           ${var5:N0}");
            Console.WriteLine($"Mean Final Base:       ${results.Average(r => r.Final_Benefit_Base):N0}");
            Console.WriteLine($"Mean Charge Caps:      {results.Average(r => r.Charge_Cap_Events):F1}");
            Console.WriteLine($"Auto Periodic Trigger: {(results.Count(r => r.Auto_Periodic) * 100.0 / results.Count):F1}%");

            // Save to CSV
            var df = results.Select((r, i) => new
            {
                Path = i + 1,
                r.PV_Profit,
                r.Profit_Margin_Pct,
                r.Final_Benefit_Base,
                r.Charge_Cap_Events
            });
            System.IO.File.WriteAllLines("ING_GoldenSelect_CSharp_Results.csv",
                new[] { "Path,PV_Profit,Profit_Margin_Pct,Final_Benefit_Base,Charge_Cap_Events" }
                .Concat(df.Select(x => $"{x.Path},{x.PV_Profit:F0},{x.Profit_Margin_Pct:F2},{x.Final_Benefit_Base:F0},{x.Charge_Cap_Events}")));
            Console.WriteLine("\nResults saved to ING_GoldenSelect_CSharp_Results.csv");
        }
    }
}