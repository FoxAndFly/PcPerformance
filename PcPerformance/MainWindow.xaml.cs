using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using OpenHardwareMonitor.Hardware;
using PcPerformance.Helper;

namespace PcPerformance
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly MainWindowViewModel _vm;
        private readonly Dictionary<int, Fan> _fans = new();
        private readonly Regex _fanNumberRegex = new(@"^Fan.*#(?<number>\d+)", RegexOptions.IgnoreCase);
        private readonly Computer _computer;

        private readonly DispatcherTimer _timer = new(DispatcherPriority.Render)
        {
            Interval = new TimeSpan(0, 0, 0, 0, 500)
        };

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _vm = new MainWindowViewModel();

            var updateVisitor = new UpdateVisitor();
            _computer = new Computer();
            _computer.Open();
            _computer.CPUEnabled = true;
            _computer.GPUEnabled = true;
            _computer.MainboardEnabled = true;
            _computer.Accept(updateVisitor);

            var timer = _timer;
            timer.Tick += Tick;
            timer.Start();
            Tick(_timer, EventArgs.Empty);
        }

        private void Tick(object sender, EventArgs e)
        {
            foreach (var hardware in _computer.Hardware)
            {
                hardware.Update();
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (hardware.HardwareType)
                {
                    case HardwareType.CPU:
                    {
                        _vm.CpuName = $"CPU — {hardware.Name}";
                        foreach (var sensor in hardware.Sensors)
                        {
                            // Debug.WriteLine("--------");
                            // Debug.WriteLine(sensor.SensorType);
                            // Debug.WriteLine(sensor.Name);
                            // Debug.WriteLine(sensor.Value.ToString());
                            switch (sensor.Name)
                            {
                                case "CPU Total":
                                    _vm.CpuLoad = PercentString(sensor.Value);
                                    break;
                                case "CPU Package":
                                    _vm.CpuTemp = CelsiusString(sensor.Value);
                                    break;
                            }
                        }

                        break;
                    }
                    case HardwareType.GpuNvidia:
                    {
                        var gpuName = hardware.Name.Replace("NVIDIA NVIDIA", "NVIDIA");
                        _vm.GpuName = $"GPU — {gpuName}";
                        foreach (var sensor in hardware.Sensors)
                        {
                            // Debug.WriteLine("--------");
                            // Debug.WriteLine(sensor.SensorType);
                            // Debug.WriteLine(sensor.Name);
                            // Debug.WriteLine(sensor.Value.ToString());
                            switch (sensor.Name)
                            {
                                case "GPU Core" when sensor.SensorType == SensorType.Clock:
                                {
                                    if (sensor.Value != null)
                                        _vm.GpuClock = $"{Math.Round(sensor.Value.Value)} MHz";
                                    break;
                                }
                                case "GPU Core" when sensor.SensorType == SensorType.Load:
                                    _vm.GpuLoad = PercentString(sensor.Value);
                                    break;
                                case "GPU Core" when sensor.SensorType == SensorType.Temperature:
                                    _vm.GpuTemp = CelsiusString(sensor.Value);
                                    break;
                                case "GPU Memory" when sensor.SensorType == SensorType.Clock:
                                {
                                    _vm.GpuMemoryClock = MHzString(sensor.Value);
                                    break;
                                }
                                case "GPU Core" when sensor.SensorType == SensorType.Temperature:
                                    _vm.GpuTemp = CelsiusString(sensor.Value);
                                    break;
                                case "GPU" when sensor.SensorType == SensorType.Fan:
                                {
                                    _vm.GpuFanRpm = RpmString(sensor.Value);
                                    break;
                                }
                                case "GPU Fan" when sensor.SensorType == SensorType.Control:
                                {
                                    _vm.GpuFanPercent = PercentString(sensor.Value);
                                    break;
                                }
                            }
                        }

                        break;
                    }
                    case HardwareType.Mainboard:
                    {
                        // Debug.WriteLine("--------");
                        // Debug.WriteLine(hardware.Name);
                        foreach (var subHardware in hardware.SubHardware)
                        {
                            subHardware.Update();
                            foreach (var sensor in subHardware.Sensors)
                            {
                                if (sensor.SensorType is not (SensorType.Fan or SensorType.Control))
                                    continue;

                                var fanNumberStr = _fanNumberRegex.Match(sensor.Name).Groups["number"].Value;
                                if (fanNumberStr is null or "")
                                    continue;

                                var fanNumber = int.Parse(fanNumberStr);
                                var fan = _fans.GetValueOrDefault(fanNumber);
                                if (fan == null)
                                {
                                    fan = new Fan(fanNumber);
                                    _fans.Add(fanNumber, fan);
                                }

                                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                                switch (sensor.SensorType)
                                {
                                    case SensorType.Fan:
                                        fan.Rpm = RpmString(sensor.Value);
                                        break;
                                    case SensorType.Control:
                                        fan.Percent = PercentString(sensor.Value);
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }

                            var comparer = new Func<Fan, Fan, bool>((fan1, fan2) => fan1.Number == fan2.Number);
                            var mapper = new Func<Fan, Fan>(fan => fan);
                            var updater = new Action<Fan, Fan>((existingFan, newFan) =>
                            {
                                existingFan.Percent = newFan.Percent;
                                existingFan.Rpm = newFan.Rpm;
                            });

                            _vm.Fans.Clear();
                            _fans
                                .Where(pair => pair.Value.Percent != "" && pair.Value.Rpm != "")
                                .Select(pair => pair.Value)
                                .ToList()
                                .ForEach(fan => { _vm.Fans.Add(fan); });

                            // _vm.Fans.UpdateItems(
                            //     _fans
                            //         .Where(pair => pair.Value.Percent != "" && pair.Value.Rpm != "")
                            //         .Select(pair => pair.Value).ToList(),
                            //     comparer,
                            //     mapper,
                            //     updater
                            // );
                            break;
                        }

                        break;
                    }
                }
            }
        }

        private static string CelsiusString(float? value)
        {
            return value == null ? "" : $"{Math.Round(value.Value, 1)} ℃";
        }

        private static string PercentString(float? value)
        {
            return value == null ? "" : $"{value.Value:0.0} %";
        }

        private static string MHzString(float? value)
        {
            return value == null ? "" : $"{Math.Round(value.Value)} MHz";
        }

        private static string RpmString(float? value)
        {
            return value == null ? "" : $"{Math.Round(value.Value)} RPM";
        }

        public class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }

            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
            }

            public void VisitSensor(ISensor sensor)
            {
            }

            public void VisitParameter(IParameter parameter)
            {
            }
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _timer.Stop();
            _computer.Close();
        }
    }
}
