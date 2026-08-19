using Cpa.BoundedMindsLab.Cli.Cli;
using Cpa.BoundedMindsLab.Experiments;
using Cpa.BoundedMindsLab.Verification;

try
{
    var parsed = CliArguments.Parse(args);
    if (parsed.Help)
    {
        PrintHelp();
        return 0;
    }

    if (parsed.List)
    {
        foreach (var experiment in ExperimentCatalog.All)
        {
            Console.WriteLine($"{experiment.Name}\n  {experiment.Question}");
        }

        return 0;
    }

    if (parsed.SelfTest)
    {
        var passed = SelfTestSuite.RunAll();
        Console.WriteLine($"All {passed.Count} self-tests passed.");
        foreach (var name in passed)
        {
            Console.WriteLine($"  PASS {name}");
        }

        return 0;
    }

    var experiments = parsed.All || parsed.Experiments.Count == 0
        ? ExperimentCatalog.All.ToArray()
        : parsed.Experiments.Select(ExperimentCatalog.Get).ToArray();

    if (parsed.ReplicationSeeds.Count > 0)
    {
        ReplicationRunner.Run(experiments, parsed.ReplicationSeeds, parsed.OutputDirectory);
        return 0;
    }

    ExperimentRunner.Run(experiments, parsed.Seed, parsed.OutputDirectory);
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception);
    return 1;
}

static void PrintHelp()
{
    Console.WriteLine("CPA Bounded Minds Lab 0.4.0");
    Console.WriteLine();
    Console.WriteLine("  --list");
    Console.WriteLine("  --self-test");
    Console.WriteLine("  --all");
    Console.WriteLine("  --experiment <name>   Repeat to select several experiments");
    Console.WriteLine("  --seed <ulong>        Single-history seed (default 101)");
    Console.WriteLine("  --replicate <csv>     Example: 101,211,307,401,503");
    Console.WriteLine("  --output <directory>");
}
