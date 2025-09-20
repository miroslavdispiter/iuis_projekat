using MVVM1;
using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.ViewModel
{
    public class MeasurementGraphViewModel : BindableBase
    {
        public ObservableCollection<Entity> Entities { get; set; }
        private Entity _selectedEntity;

        public Entity SelectedEntity
        {
            get => _selectedEntity;
            set
            {
                if (_selectedEntity != value)
                {
                    _selectedEntity = value;
                    OnPropertyChanged(nameof(SelectedEntity));
                    StatusMessage = $"Selected entity: {SelectedEntity?.Name ?? "None"}";

                    LoadMeasurementsForEntity();
                }
            }
        }

        public ObservableCollection<GraphPoint> Measurements { get; set; }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public MeasurementGraphViewModel(ObservableCollection<Entity> sharedEntities)
        {
            Entities = sharedEntities;
            Measurements = new ObservableCollection<GraphPoint>();
            StatusMessage = "Measurement Graph initialized.";

            if (Entities.Any())
                SelectedEntity = Entities.First();
        }

        private void LoadMeasurementsForEntity()
        {
            Measurements.Clear();
            if (SelectedEntity == null) return;

            string logFile = "log.txt";
            if (!File.Exists(logFile))
            {
                StatusMessage = "Log file not found.";
                return;
            }

            var lines = File.ReadAllLines(logFile)
                .Where(l => l.Contains($"Entity_{SelectedEntity.Id}"))
                .Reverse().Take(10).Reverse()
                .ToList();

            if (lines.Count == 0)
            {
                StatusMessage = $"No data for entity {SelectedEntity.Name}";
                return;
            }

            var parsedLines = lines.Select(line =>
            {
                try
                {
                    var parts = line.Split(',');
                    DateTime ts = DateTime.ParseExact(parts[0].Trim(), "dd.MM.yyyy HH:mm:ss", null);
                    double val = double.Parse(parts[2].Trim());
                    return new { Timestamp = ts, Value = val };
                }
                catch { return null; }
            }).Where(x => x != null).ToList();

            if (parsedLines.Count == 0)
            {
                StatusMessage = $"No valid data for {SelectedEntity.Name}";
                return;
            }

            double maxValue = Math.Max(10, parsedLines.Max(x => x.Value));
            double xStep = 500.0 / (parsedLines.Count - 1);

            int index = 0;
            foreach (var item in parsedLines)
            {
                Measurements.Add(new GraphPoint
                {
                    Timestamp = item.Timestamp,
                    Value = item.Value,
                    RelativeX = 50 + index * xStep,
                    RelativeY = 250 - (item.Value / maxValue) * 200
                });

                index++;
            }

            StatusMessage = $"Loaded {Measurements.Count} points for {SelectedEntity.Name}";
        }
    }
}
