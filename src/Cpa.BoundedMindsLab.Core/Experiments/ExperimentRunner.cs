using Cpa.BoundedMindsLab.Observability;

namespace Cpa.BoundedMindsLab.Experiments;

public static class ExperimentRunner
{
    public static RunResult Run(
        IReadOnlyList<IExperiment> experiments,
        ulong seed,
        string outputDirectory,
        bool quiet = false,
        IExperimentFrameObserver? observer = null,
        ExperimentRunControl? control = null)
    {
        ArgumentNullException.ThrowIfNull(experiments);
        if (experiments.Count == 0)
        {
            throw new ArgumentException("Select at least one experiment.", nameof(experiments));
        }

        var fullOutput = Path.GetFullPath(outputDirectory);
        Directory.CreateDirectory(fullOutput);
        using var journal = new ExperimentFrameJournal(Path.Combine(fullOutput, "frames.ndjson"));
        var composite = new CompositeObserver(journal, observer);
        var context = new ExperimentContext(seed, fullOutput, quiet, composite, control);
        var results = new List<ExperimentResult>(experiments.Count);

        context.Emit(
            ExperimentFrameKind.RunStarted,
            message: $"Seed {seed}; {experiments.Count} experiment(s).",
            metrics: new Dictionary<string, double>(StringComparer.Ordinal)
            {
                ["seed"] = seed,
                ["experiment_count"] = experiments.Count,
            });

        try
        {
            foreach (var experiment in experiments)
            {
                results.Add(experiment.Run(context));
            }

            context.Emit(
                ExperimentFrameKind.RunCompleted,
                message: $"Completed {results.Count} experiment(s).",
                metrics: new Dictionary<string, double>(StringComparer.Ordinal)
                {
                    ["completed_experiment_count"] = results.Count,
                });
        }
        catch (OperationCanceledException) when (control?.IsCancellationRequested == true)
        {
            composite.Observe(new ExperimentFrame(
                ExperimentFrame.CurrentSchema,
                context.NextSequence,
                ExperimentFrameKind.RunCancelled,
                "run",
                "run",
                null,
                "cancelled",
                "Run cancelled at an observation boundary.",
                null,
                null,
                null,
                null));
            ArtifactWriter.WriteRun(new RunResult(seed, fullOutput, results), "cancelled");
            throw;
        }
        catch (Exception exception)
        {
            composite.Observe(new ExperimentFrame(
                ExperimentFrame.CurrentSchema,
                context.NextSequence,
                ExperimentFrameKind.RunFaulted,
                "run",
                "run",
                null,
                "faulted",
                exception.Message,
                null,
                null,
                null,
                null));
            ArtifactWriter.WriteRun(new RunResult(seed, fullOutput, results), "faulted");
            throw;
        }

        var run = new RunResult(seed, fullOutput, results);
        ArtifactWriter.WriteRun(run);
        if (!quiet)
        {
            foreach (var result in results)
            {
                Console.WriteLine($"{result.Name}: {result.Verdict} - {result.Interpretation}");
            }
        }

        return run;
    }

    private sealed class CompositeObserver : IExperimentFrameObserver
    {
        private readonly IExperimentFrameObserver _first;
        private readonly IExperimentFrameObserver? _second;

        public CompositeObserver(IExperimentFrameObserver first, IExperimentFrameObserver? second)
        {
            _first = first;
            _second = second;
        }

        public void Observe(ExperimentFrame frame)
        {
            _first.Observe(frame);
            _second?.Observe(frame);
        }
    }
}
