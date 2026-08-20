using Cpa.BoundedMindsLab.Cli.Cli;
using Cpa.BoundedMindsLab.Challenge;
using Cpa.BoundedMindsLab.Experiments;
using Cpa.BoundedMindsLab.Falsification;
using Cpa.BoundedMindsLab.Verification;
using Cpa.BoundedMindsLab.Validation;

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

    var specialModes = (parsed.Validation ? 1 : 0) + (parsed.Challenge ? 1 : 0) + (parsed.Falsify ? 1 : 0) + (parsed.StrategicValidation ? 1 : 0) + (parsed.StrategicFalsify ? 1 : 0);
    if (specialModes > 1)
    {
        throw new ArgumentException("--validation, --challenge, --falsify, --p08-validation, and --p08-falsify are mutually exclusive.");
    }

    if (parsed.Validation)
    {
        ValidationRunner.RunHoldout(parsed.OutputDirectory);
        return 0;
    }

    if (parsed.Challenge)
    {
        ChallengeRunner.RunV1(parsed.OutputDirectory);
        return 0;
    }

    if (parsed.Falsify)
    {
        ParameterizedFalsificationRunner.RunV1(parsed.OutputDirectory);
        return 0;
    }

    if (parsed.StrategicValidation)
    {
        StrategicInfluenceValidationRunner.RunHoldout(parsed.OutputDirectory);
        return 0;
    }

    if (parsed.StrategicFalsify)
    {
        StrategicInfluenceFalsificationRunner.RunV1(parsed.OutputDirectory);
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
    Console.WriteLine("CPA Bounded Minds Lab 0.13.0");
    Console.WriteLine();
    Console.WriteLine("  --list");
    Console.WriteLine("  --self-test");
    Console.WriteLine("  --all");
    Console.WriteLine("  --validation          Reproduce frozen Protocols 01-07 on the consumed 20-seed holdout-v1 set");
    Console.WriteLine("  --challenge           Reproduce consumed challenge-v1 adversarial seed sweeps");
    Console.WriteLine("  --falsify             Reproduce consumed parameterized-falsification-v1 causal sweeps");
    Console.WriteLine("  --p08-validation      Reproduce consumed Protocol 08 p08-holdout-v1");
    Console.WriteLine("  --p08-falsify         Reproduce consumed Protocol 08 strategic-influence failure surfaces");
    Console.WriteLine("  --experiment <name>   Repeat to select several experiments; current development target is 09-authority-ancestry-circular-standing");
    Console.WriteLine("  --seed <ulong>        Single-history seed (default 101)");
    Console.WriteLine("  --replicate <csv>     Explicit replication seeds; 101,211,307,401,503 is development-v1");
    Console.WriteLine("  --output <directory>");
}
