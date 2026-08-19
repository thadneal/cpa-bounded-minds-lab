using System.IO;
using Cpa.BoundedMindsLab.Experiments;
using Cpa.BoundedMindsLab.Observability;

namespace Cpa.BoundedMindsLab.Desktop.Services;

public enum DesktopRunStatus
{
    Completed,
    Cancelled,
}

public sealed record DesktopRunCompletion(
    DesktopRunStatus Status,
    ReplicationReport? Report,
    int CompletedSeedCount,
    int PlannedSeedCount);

public sealed record DesktopSessionStatus(
    int ActiveSeedIndex,
    int SeedCount,
    int CompletedSeedCount,
    ulong? ActiveSeed);

public sealed record DesktopProtocolResult(
    ulong Seed,
    string Experiment,
    string Judgment,
    ExperimentVerdict Verdict,
    string Question,
    string Interpretation,
    int PassedAssertions,
    int FailedAssertions,
    IReadOnlyList<ExperimentAssertion> Assertions);

public sealed record DesktopProtocolResultSummary(
    string Experiment,
    int CompletedRuns,
    int Supported,
    int Mixed,
    int Refuted,
    int Inconclusive);

public sealed record DesktopProtocolResultsSnapshot(
    long Version,
    IReadOnlyList<DesktopProtocolResult> Results,
    IReadOnlyList<DesktopProtocolResultSummary> Summaries);

public sealed class DesktopRunCoordinator : IDisposable
{
    private readonly object _sync = new();
    private readonly DisplayTelemetryPipeline _telemetry = new();
    private ExperimentRunControl? _control;
    private Task<DesktopRunCompletion>? _runTask;
    private int _activeSeedIndex;
    private int _seedCount;
    private int _completedSeedCount;
    private ulong? _activeSeed;
    private readonly List<DesktopProtocolResult> _protocolResults = [];
    private long _protocolResultsVersion;
    private bool _disposed;

    public DisplayTelemetryPipeline Telemetry => _telemetry;

    public bool IsRunning
    {
        get
        {
            lock (_sync)
            {
                return _runTask is { IsCompleted: false };
            }
        }
    }

    public DesktopSessionStatus GetSessionStatus()
    {
        lock (_sync)
        {
            return new DesktopSessionStatus(
                _activeSeedIndex,
                _seedCount,
                _completedSeedCount,
                _activeSeed);
        }
    }

    public DesktopProtocolResultsSnapshot GetProtocolResults()
    {
        lock (_sync)
        {
            var results = _protocolResults.ToArray();
            var summaries = results
                .GroupBy(result => result.Experiment, StringComparer.Ordinal)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new DesktopProtocolResultSummary(
                    group.Key,
                    group.Count(),
                    group.Count(result => result.Verdict == ExperimentVerdict.Support),
                    group.Count(result => result.Verdict == ExperimentVerdict.Mixed),
                    group.Count(result => result.Verdict == ExperimentVerdict.Disconfirm),
                    group.Count(result => result.Verdict == ExperimentVerdict.Inconclusive)))
                .ToArray();
            return new DesktopProtocolResultsSnapshot(_protocolResultsVersion, results, summaries);
        }
    }

    public Task<DesktopRunCompletion> RunAsync(
        IReadOnlyList<string> experimentNames,
        IReadOnlyList<ulong> seeds,
        string outputDirectory,
        int boundaryDelayMilliseconds)
    {
        ArgumentNullException.ThrowIfNull(experimentNames);
        ArgumentNullException.ThrowIfNull(seeds);
        if (experimentNames.Count == 0)
        {
            throw new ArgumentException("Select at least one experiment.", nameof(experimentNames));
        }

        if (seeds.Count == 0)
        {
            throw new ArgumentException("Provide at least one seed.", nameof(seeds));
        }

        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_runTask is { IsCompleted: false })
            {
                throw new InvalidOperationException("An experiment session is already active.");
            }

            _telemetry.Reset();
            var control = new ExperimentRunControl(boundaryDelayMilliseconds);
            var experiments = experimentNames.Select(ExperimentCatalog.Get).ToArray();
            var seedArray = seeds.ToArray();
            var fullOutput = Path.GetFullPath(outputDirectory);
            var completion = new TaskCompletionSource<DesktopRunCompletion>(TaskCreationOptions.RunContinuationsAsynchronously);
            var thread = new Thread(() => RunSession(experiments, seedArray, fullOutput, control, completion))
            {
                IsBackground = true,
                Name = "CPA bounded-minds experiment session",
                Priority = ThreadPriority.BelowNormal,
            };

            _activeSeedIndex = 0;
            _seedCount = seedArray.Length;
            _completedSeedCount = 0;
            _activeSeed = null;
            _protocolResults.Clear();
            _protocolResultsVersion++;

            var task = completion.Task;
            _control = control;
            _runTask = task;
            _ = task.ContinueWith(
                _ => ReleaseControl(task, control),
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
            thread.Start();
            return task;
        }
    }

    public bool Pause() => WithControl(static control => control.Pause());

    public bool Resume() => WithControl(static control => control.Resume());

    public bool Step() => WithControl(static control => control.Step());

    public bool Cancel() => WithControl(static control => control.Cancel());

    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _control?.Cancel();
        }

        _telemetry.Dispose();
    }

    private void RunSession(
        IExperiment[] experiments,
        ulong[] seeds,
        string outputDirectory,
        ExperimentRunControl control,
        TaskCompletionSource<DesktopRunCompletion> completion)
    {
        var completedRuns = new List<RunResult>(seeds.Length);
        var experimentNames = experiments.Select(experiment => experiment.Name).ToArray();
        Directory.CreateDirectory(outputDirectory);
        ArtifactWriter.WriteSessionManifest(
            outputDirectory,
            seeds,
            [],
            experimentNames,
            "running",
            null);

        try
        {
            for (var index = 0; index < seeds.Length; index++)
            {
                var seed = seeds[index];
                _telemetry.Reset();
                SetActiveSeed(index + 1, seeds.Length, completedRuns.Count, seed);
                ArtifactWriter.WriteSessionManifest(
                    outputDirectory,
                    seeds,
                    completedRuns.Select(run => run.Seed).ToArray(),
                    experimentNames,
                    "running",
                    seed);

                var run = ExperimentRunner.Run(
                    experiments,
                    seed,
                    Path.Combine(outputDirectory, $"seed-{seed}"),
                    quiet: true,
                    _telemetry,
                    control);
                completedRuns.Add(run);
                RecordProtocolResults(run);
                SetActiveSeed(index + 1, seeds.Length, completedRuns.Count, seed);
            }

            var report = ReplicationRunner.CreateReport(experiments, completedRuns);
            ArtifactWriter.WriteReplication(report, outputDirectory);
            ArtifactWriter.WriteSessionManifest(
                outputDirectory,
                seeds,
                completedRuns.Select(run => run.Seed).ToArray(),
                experimentNames,
                "completed",
                null);
            SetActiveSeed(seeds.Length, seeds.Length, completedRuns.Count, null);
            completion.TrySetResult(new DesktopRunCompletion(
                DesktopRunStatus.Completed,
                report,
                completedRuns.Count,
                seeds.Length));
        }
        catch (OperationCanceledException) when (control.IsCancellationRequested)
        {
            var interruptedSeed = GetActiveSeed();
            ArtifactWriter.WriteSessionManifest(
                outputDirectory,
                seeds,
                completedRuns.Select(run => run.Seed).ToArray(),
                experimentNames,
                "cancelled",
                interruptedSeed);
            SetActiveSeed(Math.Min(completedRuns.Count + 1, seeds.Length), seeds.Length, completedRuns.Count, null);
            completion.TrySetResult(new DesktopRunCompletion(
                DesktopRunStatus.Cancelled,
                null,
                completedRuns.Count,
                seeds.Length));
        }
        catch (Exception exception)
        {
            var faultedSeed = GetActiveSeed();
            ArtifactWriter.WriteSessionManifest(
                outputDirectory,
                seeds,
                completedRuns.Select(run => run.Seed).ToArray(),
                experimentNames,
                "faulted",
                faultedSeed);
            SetActiveSeed(Math.Min(completedRuns.Count + 1, seeds.Length), seeds.Length, completedRuns.Count, null);
            completion.TrySetException(exception);
        }
    }


    private void RecordProtocolResults(RunResult run)
    {
        lock (_sync)
        {
            foreach (var result in run.Experiments)
            {
                var passed = result.Assertions.Count(assertion => assertion.Passed);
                _protocolResults.Add(new DesktopProtocolResult(
                    run.Seed,
                    result.Name,
                    DisplayJudgment(result.Verdict),
                    result.Verdict,
                    result.Question,
                    result.Interpretation,
                    passed,
                    result.Assertions.Count - passed,
                    result.Assertions.ToArray()));
            }

            _protocolResultsVersion++;
        }
    }

    private static string DisplayJudgment(ExperimentVerdict verdict) => verdict switch
    {
        ExperimentVerdict.Support => "Supported",
        ExperimentVerdict.Mixed => "Mixed",
        ExperimentVerdict.Disconfirm => "Refuted",
        ExperimentVerdict.Inconclusive => "Inconclusive",
        _ => verdict.ToString(),
    };

    private bool WithControl(Action<ExperimentRunControl> action)
    {
        lock (_sync)
        {
            if (_control is null)
            {
                return false;
            }

            action(_control);
            return true;
        }
    }

    private ulong? GetActiveSeed()
    {
        lock (_sync)
        {
            return _activeSeed;
        }
    }

    private void SetActiveSeed(int activeSeedIndex, int seedCount, int completedSeedCount, ulong? activeSeed)
    {
        lock (_sync)
        {
            _activeSeedIndex = activeSeedIndex;
            _seedCount = seedCount;
            _completedSeedCount = completedSeedCount;
            _activeSeed = activeSeed;
        }
    }

    private void ReleaseControl(Task<DesktopRunCompletion> task, ExperimentRunControl control)
    {
        lock (_sync)
        {
            if (ReferenceEquals(_runTask, task))
            {
                _runTask = null;
                _control = null;
            }
        }

        control.Dispose();
    }
}
