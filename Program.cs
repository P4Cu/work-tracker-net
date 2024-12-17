using System.Diagnostics;
using Gtk;

Grid ToGrid(Data data)
{
  Grid grid = new();
  grid.RowSpacing = 6;
  grid.ColumnSpacing = 6;
  grid.RowHomogeneous = true;

  MinutesTimeSpan expected = new(
      TimeSpan.FromTicks(TimeSpan.FromHours(8).Ticks * data.table.Count)
  );

  grid.Attach(new Label("Date"), 2, 0, 1, 1);
  grid.Attach(new Label("Work"), 3, 0, 1, 1);
  grid.Attach(new Label("Inter"), 4, 0, 1, 1);
  grid.Attach(new Label("Status"), 5, 0, 1, 1);
  int i = 1;
  foreach (var record in data.table)
  {
    Label a = new("🪄");
    a.TooltipText = record.OriginalInput;
    grid.Attach(a, 1, i, 1, 1);

    grid.Attach(new Label(record.Date.ToString("dd-MM")), 2, i, 1, 1);

    grid.Attach(new Label(record.TotalWorkTime.ToString()), 3, i, 1, 1);
    grid.Attach(new Label(record.TotalInterruptionTime.ToString()), 4, i, 1, 1);
    grid.Attach(new Label(record.Status.ToString()), 5, i, 1, 1);
    i++;
  }

  grid.Attach(new Label("====="), 3, i, 1, 1);
  grid.Attach(new Label("====="), 4, i, 1, 1);
  i++;

  var totalWorkTimeLabel = new Label(data.TotalWorkTime.ToString());
  grid.Attach(totalWorkTimeLabel, 3, i, 1, 1);
  var TotalInterruptionTimeLabel = new Label(data.TotalInterruptionTime.ToString());
  grid.Attach(TotalInterruptionTimeLabel, 4, i, 1, 1);
  grid.Attach(new Label("TOTAL"), 5, i, 1, 1);
  i++;

  grid.Attach(new Label(expected.ToString()), 3, i, 1, 1);
  grid.Attach(new Label("EXPECTED"), 5, i, 1, 1);
  i++;

  grid.Attach(new Label("====="), 3, i, 1, 1);
  grid.Attach(new Label("====="), 4, i, 1, 1);
  i++;

  var totalDifferenceLabel = new Label((data.TotalWorkTime.Value - expected).ToString());
  grid.Attach(totalDifferenceLabel, 3, i, 1, 1);
  grid.Attach(new Label("DIFFERENCE"), 5, i, 1, 1);
  i++;

  data.TotalWorkTime.Changed += (obj, v) =>
  {
    totalWorkTimeLabel.Text = v.ToString();
    totalDifferenceLabel.Text = (v - expected).ToString();
    grid.ShowAll();
  };
  data.TotalInterruptionTime.Changed += (obj, v) =>
  {
    TotalInterruptionTimeLabel.Text = v.ToString();
    grid.ShowAll();
  };

  return grid;
}

Button EditButton(string file)
{
  Button button = new Button("Edit");
  button.Clicked += (sender, e) =>
  {
    try
    {
      // Use Process.Start to open the file with the default system editor
      Process.Start(new ProcessStartInfo { FileName = file, UseShellExecute = true });
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error opening file: {ex.Message}");
    }
  };
  return button;
}

Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
Trace.AutoFlush = true;
Trace.Indent();

Storage storage = new();
Data data = new();

data.ParseInput(storage.ReadFile());

Application.Init(); // Initialize the GTK application

// Create the main window
var win = new Window("Work timer");

win.SetPosition(WindowPosition.Center);

Grid mainView = new();
Grid resultsGrid = ToGrid(data);
mainView.Attach(resultsGrid, 1, 1, 1, 1);

Grid rightGrid = new();
rightGrid.Attach(EditButton(storage.FilePath), 1, 1, 1, 1);
mainView.Attach(rightGrid, 2, 1, 1, 1);

win.Add(mainView);

var refresh = () =>
{
  data.ParseInput(storage.ReadFile());
};

data.Reload += (_, _) =>
{
  mainView.Remove(resultsGrid);
  resultsGrid = ToGrid(data);
  mainView.Attach(resultsGrid, 1, 1, 1, 1);
  mainView.ShowAll();
};
data.RowChanged += (_, row) =>
{
  var record = data.table.Last();
  Trace.TraceWarning($"ROWCHANGED! {record}");
  var i = row + 1; // header
  var grid = resultsGrid;

  var updateLabel = (int column) =>
  {
    var l = grid.GetChildAt(column, i) as Label;
    return l!;
  };

  updateLabel(1).TooltipText = record.OriginalInput;
  updateLabel(2).Text = record.Date.ToString("dd-MM");
  updateLabel(3).Text = record.TotalWorkTime.ToString();
  updateLabel(4).Text = record.TotalInterruptionTime.ToString();
  updateLabel(5).Text = record.Status.ToString();

  grid.ShowAll();
};

storage.FileChanged += () => refresh();

// Show everything
win.ShowAll();

// Handle the window close event
win.DeleteEvent += (sender, e) =>
{
  Gtk.Application.Quit();
};

_ = GLib.Timeout.Add(
    60000,
    () =>
    {
      data.UpdateTime();
      return true;
    }
);

Gtk.Application.Run();
