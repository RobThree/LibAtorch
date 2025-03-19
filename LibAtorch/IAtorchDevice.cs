
namespace LibAtorch;

public interface IAtorchDevice
{
    string Name { get; }
    SerialOptions SerialOptions { get; }
    Task Open(CancellationToken cancellationToken = default);
    void Close();
    bool IsOpen { get; }
    Task<bool> IsLoadEnabled(CancellationToken cancellationToken = default);
    Task<LoadState> ReadLoad(CancellationToken cancellationToken = default);
    Task<double> ReadCapacityMiliAmpHours(CancellationToken cancellationToken = default);
    Task<double> ReadCapacityMiliWattHours(CancellationToken cancellationToken = default);
    Task<double> ReadCurrent(CancellationToken cancellationToken = default);
    Task<double> ReadCurrentSetting(CancellationToken cancellationToken = default);
    Task<double> ReadCutoffVoltageSetting(CancellationToken cancellationToken = default);
    Task<TimeSpan> ReadElapsedTime(CancellationToken cancellationToken = default);
    Task<int> ReadMosfetTemperature(CancellationToken cancellationToken = default);
    Task<TimeSpan> ReadTimerSetting(CancellationToken cancellationToken = default);
    Task<double> ReadVoltage(CancellationToken cancellationToken = default);
    Task ResetCounters(CancellationToken cancellationToken = default);
    Task SetCurrent(double current, CancellationToken cancellationToken = default);
    Task SetCurrentIfChanged(double current, CancellationToken cancellationToken = default);
    Task SetCutoffVoltage(double cutoffVoltage, CancellationToken cancellationToken = default);
    Task SetCutoffVoltageIfChanged(double cutoffVoltage, CancellationToken cancellationToken = default);
    Task SetLoad(LoadState state, CancellationToken cancellationToken = default);
    Task SetLoadIfChanged(LoadState loadState, CancellationToken cancellationToken = default);
    Task SetTimeout(TimeSpan timeOut, CancellationToken cancellationToken = default);
    Task SetTimeoutIfChanged(TimeSpan timeOut, CancellationToken cancellationToken = default);
    Task EnsureLoadOff(TimeSpan retryTime);
}