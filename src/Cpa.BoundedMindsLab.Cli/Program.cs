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

    var specialModes = (parsed.Validation ? 1 : 0) + (parsed.Challenge ? 1 : 0) + (parsed.Falsify ? 1 : 0) + (parsed.StrategicValidation ? 1 : 0) + (parsed.StrategicFalsify ? 1 : 0) + (parsed.AuthorityValidation ? 1 : 0) + (parsed.AuthorityFalsify ? 1 : 0);
    if (specialModes > 1)
    {
        throw new ArgumentException("Validation, challenge, and falsification special modes are mutually exclusive.");
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

    if (parsed.AuthorityValidation)
    {
        AuthorityAncestryValidationRunner.RunHoldout(parsed.OutputDirectory);
        return 0;
    }

    if (parsed.AuthorityFalsify)
    {
        AuthorityAncestryFalsificationRunner.RunV1(parsed.OutputDirectory);
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
    Console.WriteLine("CPA Bounded Minds Lab 0.14.0");
    Console.WriteLine();
    Console.WriteLine("  --list");
    Console.WriteLine("  --self-test");
    Console.WriteLine("  --all");
    Console.WriteLine("  --validation          Reproduce frozen Protocols 01-07 on the consumed 20-seed holdout-v1 set");
    Console.WriteLine("  --challenge           Reproduce consumed challenge-v1 adversarial seed sweeps");
    Console.WriteLine("  --falsify             Reproduce consumed parameterized-falsification-v1 causal sweeps");
    Console.WriteLine("  --p08-validation      Reproduce consumed Protocol 08 p08-holdout-v1");
    Console.WriteLine("  --p08-falsify         Reproduce consumed Protocol 08 strategic-influence failure surfaces");
    Console.WriteLine("  --p09-validation      Run frozen Protocol 09 on fresh p09-holdout-v1 (first execution consumes it)");
    Console.WriteLine("  --p09-falsify         Map frozen Protocol 09 operating-envelope surfaces after preserving holdout output");
    Console.WriteLine("  --experiment <name>   Repeat to select one or more ordinary experiment runs (Protocols 01-09 are frozen)");
    Console.WriteLine("  --seed <ulong>        Single-history seed (default 101)");
    Console.WriteLine("  --replicate <csv>     Explicit replication seeds; 101,211,307,401,503 is development-v1");
    Console.WriteLine("  --output <directory>");
}
