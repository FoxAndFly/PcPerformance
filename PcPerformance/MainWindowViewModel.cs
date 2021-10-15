using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PcPerformance
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _cpuName = "";
        private string _cpuTemp = "";
        private string _cpuLoad = "";

        private string _gpuName = "";
        private string _gpuClock = "";
        private string _gpuMemoryClock = "";
        private string _gpuTemp = "";
        private string _gpuLoad = "";
        private string _gpuFanRpm = "";
        private string _gpuFanPercent = "";

        public string CpuName
        {
            get => _cpuName;
            set => SetField(ref _cpuName, value);
        }

        public string CpuTemp
        {
            get => _cpuTemp;
            set => SetField(ref _cpuTemp, value);
        }

        public string CpuLoad
        {
            get => _cpuLoad;
            set => SetField(ref _cpuLoad, value);
        }

        public string GpuName
        {
            get => _gpuName;
            set => SetField(ref _gpuName, value);
        }

        public string GpuClock
        {
            get => _gpuClock;
            set => SetField(ref _gpuClock, value);
        }

        public string GpuMemoryClock
        {
            get => _gpuMemoryClock;
            set => SetField(ref _gpuMemoryClock, value);
        }

        public string GpuTemp
        {
            get => _gpuTemp;
            set => SetField(ref _gpuTemp, value);
        }

        public string GpuLoad
        {
            get => _gpuLoad;
            set => SetField(ref _gpuLoad, value);
        }

        public string GpuFanRpm
        {
            get => _gpuFanRpm;
            set => SetField(ref _gpuFanRpm, value);
        }

        public string GpuFanPercent
        {
            get => _gpuFanPercent;
            set => SetField(ref _gpuFanPercent, value);
        }

        public ObservableCollection<Fan> Fans { get; set; } = new();

        #region boilerplate

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            OnPropertyChanged(propertyName);
        }

        #endregion
    }

    public class Fan
    {
        public int Number { get; }

        public string Name => $"Fan #{Number}";

        public string Rpm { get; set; } = "";
        public string Percent { get; set; } = "";

        public Fan(int number)
        {
            Number = number;
        }
    }
}
