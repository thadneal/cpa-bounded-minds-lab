namespace Cpa.BoundedMindsLab.Cli.Cli;

public sealed record CliArguments(
    bool List,
    bool SelfTest,
    bool Validation,
    bool Challenge,
    bool Falsify,
    bool StrategicValidation,
    bool StrategicFalsify,
    bool AuthorityValidation,
    bool AuthorityFalsify,
    bool Help,
    bool All,
    IReadOnlyList<string> Experiments,
    ulong Seed,
    IReadOnlyList<ulong> ReplicationSeeds,
    string OutputDirectory)
{
    public static CliArguments Parse(string[] args)
    {
        var list = false;
        var selfTest = false;
        var validation = false;
        var challenge = false;
        var falsify = false;
        var strategicValidation = false;
        var strategicFalsify = false;
        var authorityValidation = false;
        var authorityFalsify = false;
        var help = false;
        var all = false;
        var experiments = new List<string>();
        var seed = 101UL;
        var replicationSeeds = new List<ulong>();
        var output = Path.Combine(Environment.CurrentDirectory, "artifacts", $"run-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}");

        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index];
            switch (argument)
            {
                case "--list":
                    list = true;
                    break;
                case "--self-test":
                    selfTest = true;
                    break;
                case "--all":
                    all = true;
                    break;
                case "--validation":
                    validation = true;
                    break;
                case "--challenge":
                    challenge = true;
                    break;
                case "--falsify":
                    falsify = true;
                    break;
                case "--p08-validation":
                    strategicValidation = true;
                    break;
                case "--p08-falsify":
                    strategicFalsify = true;
                    break;
                case "--p09-validation":
                    authorityValidation = true;
                    break;
                case "--p09-falsify":
                    authorityFalsify = true;
                    break;
                case "--experiment":
                    experiments.Add(RequireValue(args, ref index, argument));
                    break;
                case "--seed":
                    seed = ParseSeed(RequireValue(args, ref index, argument));
                    break;
                case "--replicate":
                    replicationSeeds.AddRange(ParseSeeds(RequireValue(args, ref index, argument)));
                    break;
                case "--output":
                    output = RequireValue(args, ref index, argument);
                    break;
                case "--help":
                case "-h":
                    help = true;
                    break;
                default:
                    throw new ArgumentException($"Unknown argument '{argument}'.");
            }
        }

        return new CliArguments(list, selfTest, validation, challenge, falsify, strategicValidation, strategicFalsify, authorityValidation, authorityFalsify, help, all, experiments, seed, replicationSeeds, output);
    }

    private static string RequireValue(string[] args, ref int index, string argument)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"{argument} requires a value.");
        }

        index++;
        return args[index];
    }

    private static ulong ParseSeed(string value) => ulong.TryParse(value, out var seed)
        ? seed
        : throw new ArgumentException($"Invalid seed '{value}'.");

    private static ulong[] ParseSeeds(string value)
    {
        var seeds = value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ParseSeed)
            .Distinct()
            .ToArray();
        if (seeds.Length == 0)
        {
            throw new ArgumentException("--replicate requires at least one seed.");
        }

        return seeds;
    }
}
