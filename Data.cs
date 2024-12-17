using System.Diagnostics;
using System.Globalization;

public static class TimeSpanExtensions
{
    public static string ToHoursMinutesString(this TimeSpan ts) =>
        $"{(int)ts.TotalHours:D2}:{Math.Abs(ts.Minutes):D2}";

    public static bool TotalMinutesEqual(this TimeSpan ts, TimeSpan other) =>
        (int)other.TotalMinutes == (int)ts.TotalMinutes;
}

public enum DayStatus
{
    OK,
    PENDING,
    INVALID,
}

public class TimeEntry
{
    public TimeSpan Start { get; set; }
    public TimeSpan? End { get; set; }
    public string? Task { get; set; }

    public override string ToString()
    {
        return $"{Start}-{End} {Task}";
    }
}

public class DayRecord : IEquatable<DayRecord>
{
    public DateTime Date { get; set; }
    public List<TimeEntry> Entries { get; set; } = new List<TimeEntry>();
    public DayStatus Status { get; set; } = DayStatus.OK;

    public MinutesTimeSpan TotalWorkTime { get; set; } = new();
    public MinutesTimeSpan TotalInterruptionTime { get; set; } = new();

    public string? OriginalInput { get; set; }

    public bool Equals(DayRecord? other)
    {
        Debug.Assert(other != null);
        return TotalWorkTime.Equals(other.TotalWorkTime)
            && TotalInterruptionTime.Equals(other.TotalInterruptionTime)
            && other.OriginalInput!.Equals(OriginalInput!);
    }

    public override string ToString()
    {
        return $"{Date.ToString("dd-MM")} Work={TotalWorkTime} Interruption={TotalInterruptionTime} Status={Status} Entries={Entries.Count}";
    }
}

public class Data
{
    public delegate void Notify();

    private const double WorkingDayHours = 8.0;
    private static readonly TimeSpan WorkingDay = TimeSpan.FromHours(WorkingDayHours);

    public event EventHandler? Reload;
    public event EventHandler<int>? RowChanged;

    public List<DayRecord> table { get; private set; } = new();

    public SimpleProp<MinutesTimeSpan> TotalWorkTime = new(new());
    public SimpleProp<MinutesTimeSpan> TotalInterruptionTime = new(new());

    private DayRecord ParseLine(string line)
    {
        var parts = line.Split(' ', 2);
        var date = DateTime.ParseExact(parts[0], "d.MM", CultureInfo.InvariantCulture);

        var dayRecord = new DayRecord { Date = date, OriginalInput = line };
        try
        {
            if (parts.Length > 1)
            {
                var entries = parts[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (var entry in entries)
                {
                    var timeTask = entry.Split(' ', 2);
                    var times = timeTask[0].Split('-');
                    var start = TimeSpan.Parse(times[0]);
                    TimeSpan? end =
                        times.Length > 1 && TimeSpan.TryParse(times[1], out var parsedEnd)
                            ? parsedEnd
                            : null;

                    dayRecord.Entries.Add(
                        new TimeEntry
                        {
                            Start = start,
                            End = end,
                            Task = timeTask.Length > 1 ? timeTask[1] : null,
                        }
                    );
                }
            }

            CalculateStatistics(dayRecord);
        }
        catch
        {
            dayRecord.Status = DayStatus.INVALID;
        }
        Trace.TraceInformation($"Entry: {dayRecord}");
        return dayRecord;
    }

    public void ParseInput(string input)
    {
        Trace.TraceInformation("ParseInput");
        var lines = input.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var records = new List<DayRecord>();
        DateTime today = DateTime.Today;

        MinutesTimeSpan totalWork = new();
        MinutesTimeSpan totalInterrupt = new();
        foreach (var line in lines)
        {
            var record = ParseLine(line);
            records.Add(record);
            totalWork += record.TotalWorkTime;
            totalInterrupt += record.TotalInterruptionTime;
        }

        Trace.TraceInformation(
            $"ParseInput setting TotalWorkTime={TotalWorkTime} TotalInterruptionTime={TotalInterruptionTime}"
        );
        TotalWorkTime.Value = totalWork;
        TotalInterruptionTime.Value = totalInterrupt;
        table = records;

        // always reload on parsing input
        Reload?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateTime()
    {
        var lastRow = table.Count - 1;
        var previous = table[lastRow];
        var newRecord = ParseLine(previous.OriginalInput!);

        if (!previous.Equals(newRecord))
        {
            Trace.TraceInformation("Last row updated");
            table[table.Count - 1] = newRecord;
            RowChanged?.Invoke(this, lastRow);
            TotalWorkTime.Value += newRecord.TotalWorkTime - previous.TotalWorkTime;
            TotalInterruptionTime.Value +=
                newRecord.TotalInterruptionTime - previous.TotalInterruptionTime;
        }
    }

    private void CalculateStatistics(DayRecord record)
    {
        TimeSpan totalWork = TimeSpan.Zero;
        TimeSpan interruption = TimeSpan.Zero;
        DateTime today = DateTime.Today;

        // TODO: foreach
        for (int i = 0; i < record.Entries.Count; i++)
        {
            var entry = record.Entries[i];
            if (!entry.End.HasValue)
            {
                if (record.Date == today)
                {
                    entry.End = DateTime.Now.TimeOfDay;
                    record.Status = DayStatus.PENDING;
                }
                else
                {
                    record.Status = DayStatus.INVALID;
                    continue;
                }
            }

            totalWork += entry.End.Value - entry.Start;

            if (i > 0)
            {
                var previous = record.Entries[i - 1];
                if (previous.End.HasValue)
                {
                    interruption += entry.Start - previous.End.Value;
                }
            }
        }

        record.TotalWorkTime.Assign(totalWork);
        record.TotalInterruptionTime.Assign(interruption);
    }
}
