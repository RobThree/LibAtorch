using LibAtorch.Exceptions;
using LibAtorch.Responses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.IO.Ports;

namespace LibAtorch;

public class AtorchDevice : IAtorchDevice
{
    private readonly SerialPort _serialport;
    private readonly ILogger<AtorchDevice> _logger;

    private static readonly byte[] _zerodata = [0x00, 0x00];
    private static readonly Request _enableload = new(CommandType.ToggleLoad, [(byte)LoadState.On, 0x00]);
    private static readonly Request _disableload = new(CommandType.ToggleLoad, [(byte)LoadState.Off, 0x00]);
    private static readonly Request _resetcounterscommand = new(CommandType.ResetCounters, _zerodata);

    /// <summary>
    /// Returns the name of the serial port
    /// </summary>
    public string Name => SerialOptions.PortName;

    /// <summary>
    /// Gets the serial options
    /// </summary>
    public SerialOptions SerialOptions { get; }


    /// <summary>
    /// Creates a new instance of the AtorchDevice
    /// </summary>
    /// <param name="serialOptions">The <see cref="SerialOptions"/> to configure the <see cref="AtorchDevice"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when null <paramref name="serialOptions"/> are passed</exception>
    public AtorchDevice(SerialOptions serialOptions)
        : this(NullLogger<AtorchDevice>.Instance, Options.Create(serialOptions)) { }

    /// <summary>
    /// Creates a new instance of the AtorchDevice
    /// </summary>
    /// <param name="serialOptions">The <see cref="SerialOptions"/> to configure the <see cref="AtorchDevice"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when null <paramref name="serialOptions"/> are passed</exception>
    public AtorchDevice(IOptions<SerialOptions> serialOptions)
        : this(NullLogger<AtorchDevice>.Instance, serialOptions) { }

    /// <summary>
    /// Creates a new instance of the AtorchDevice
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> to use to log events to.</param>
    /// <param name="serialOptions">The <see cref="SerialOptions"/> to configure the <see cref="AtorchDevice"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when a null <paramref name="logger"/> or <paramref name="serialOptions"/> are passed</exception>
    public AtorchDevice(ILogger<AtorchDevice> logger, SerialOptions serialOptions)
        : this(logger, Options.Create(serialOptions)) { }

    /// <summary>
    /// Creates a new instance of the AtorchDevice
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> to use to log events to.</param>
    /// <param name="serialOptions">The <see cref="SerialOptions"/> to configure the <see cref="AtorchDevice"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when a null <paramref name="logger"/> or <paramref name="serialOptions"/> are passed</exception>
    public AtorchDevice(ILogger<AtorchDevice> logger, IOptions<SerialOptions> serialOptions)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        SerialOptions = serialOptions.Value ?? throw new ArgumentNullException(nameof(serialOptions));
        _serialport = new SerialPort(
            SerialOptions.PortName,
            SerialOptions.BaudRate,
            SerialOptions.Parity.ToSystemParity(),
            SerialOptions.DataBits,
            SerialOptions.StopBits.ToSystemStopBits()
        )
        {
            DtrEnable = false,
            RtsEnable = false,
            Handshake = Handshake.None,
            ReadTimeout = (int)SerialOptions.ReadTimeout.TotalMilliseconds,
            WriteTimeout = (int)SerialOptions.WriteTimeout.TotalMilliseconds
        };
    }

    /// <summary>
    /// When commands are sent too quickly the device will not respond or respond with an error or gibberish. You can
    /// set a pause between commands to prevent this. Default is 100ms.
    /// </summary>
    public TimeSpan CommandPause { get; init; } = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Sets the load state (ON / OFF).
    /// </summary>
    /// <param name="state">The <see cref="LoadState"/> of the load.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous write operation.</returns>
    public Task SetLoad(LoadState state, CancellationToken cancellationToken = default)
            => SendRequest<CommandResponse>(state == LoadState.On ? _enableload : _disableload, cancellationToken);

    /// <summary>
    /// Sets the current in Amperes for the load.
    /// </summary>
    /// <param name="current">The current in Amperes.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous write operation.</returns>
    public Task SetCurrent(double current, CancellationToken cancellationToken = default)
        => SendRequest<CommandResponse>(new Request(CommandType.SetCurrent, current), cancellationToken);

    /// <summary>
    /// Sets the cutoff voltage in Volts for the load.
    /// </summary>
    /// <param name="cutoffVoltage">The cutoff voltage.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous write operation.</returns>
    public Task SetCutoffVoltage(double cutoffVoltage, CancellationToken cancellationToken = default)
        => SendRequest<CommandResponse>(new Request(CommandType.SetCutoffVoltage, cutoffVoltage), cancellationToken);

    /// <summary>
    /// Sets the timeout for the load.
    /// </summary>
    /// <param name="timeOut">The timeout.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous write operation.</returns>
    public Task SetTimeout(TimeSpan timeOut, CancellationToken cancellationToken = default)
        => SendRequest<CommandResponse>(new Request(CommandType.SetTimeout, timeOut), cancellationToken);

    /// <summary>
    /// Sets the load state (ON / OFF) if the current state is different.
    /// </summary>
    /// <param name="loadState">The <see cref="LoadState"/> of the load.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous write operation.</returns>
    public Task SetLoadIfChanged(LoadState loadState, CancellationToken cancellationToken = default)
        => SetIfChanged(ReadLoad, loadState, SetLoad, cancellationToken);

    /// <summary>
    /// Sets the current in Amperes for the load if the current setting is different.
    /// </summary>
    /// <param name="current">The current in Amperes.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous write operation.</returns>
    public Task SetCurrentIfChanged(double current, CancellationToken cancellationToken = default)
        => SetIfChanged(ReadCurrentSetting, current, SetCurrent, cancellationToken);

    /// <summary>
    /// Sets the cutoff voltage in Volts for the load if the cutoff voltage setting is different.
    /// </summary>
    /// <param name="cutoffVoltage">The cutoff voltage.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous write operation.</returns>
    public Task SetCutoffVoltageIfChanged(double cutoffVoltage, CancellationToken cancellationToken = default)
        => SetIfChanged(ReadCutoffVoltageSetting, cutoffVoltage, SetCutoffVoltage, cancellationToken);

    /// <summary>
    /// Sets the timeout for the load if the timeout setting is different.
    /// </summary>
    /// <param name="timeOut">The timeout.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous write operation.</returns>
    public Task SetTimeoutIfChanged(TimeSpan timeOut, CancellationToken cancellationToken = default)
        => SetIfChanged(ReadTimerSetting, timeOut, SetTimeout, cancellationToken);

    private static async Task SetIfChanged<T>(Func<CancellationToken, Task<T>> readFunc, T compare, Func<T, CancellationToken, Task> setFunc, CancellationToken cancellationToken = default)
        where T : struct
    {
        if (!EqualityComparer<T>.Default.Equals(compare, await readFunc(cancellationToken)))
        {
            await setFunc(compare, cancellationToken);
        }
    }

    /// <summary>
    /// Resets the counters of the load.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous write operation.</returns>
    public Task ResetCounters(CancellationToken cancellationToken = default)
        => SendRequest<CommandResponse>(_resetcounterscommand, cancellationToken);

    /// <summary>
    /// Gets wether the load is enabled or not.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task{bool}"/> whose <see cref="P:System.Threading.Tasks.Task`1.Result"/> property is true if there the load is enabled or false if it is not.</returns>
    public async Task<bool> IsLoadEnabled(CancellationToken cancellationToken = default)
        => await ReadLoad(cancellationToken) == LoadState.On;

    /// <summary>
    /// Gets wether the load is enabled or not.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task{bool}"/> whose <see cref="P:System.Threading.Tasks.Task`1.Result"/> property is a <see cref="LoadState"/> that indicates if the load is on or not.</returns>
    public async Task<LoadState> ReadLoad(CancellationToken cancellationToken = default)
        => (await SendRequest<BoolResponse>(new Request(QueryType.LoadEnabled), cancellationToken)).Value ? LoadState.On : LoadState.Off;

    /// <summary>
    /// Reads the voltage of the load.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task{bool}"/> whose <see cref="P:System.Threading.Tasks.Task`1.Result"/> property is the voltage the load currently reads.</returns>
    public async Task<double> ReadVoltage(CancellationToken cancellationToken = default)
        => (await SendRequest<IntegerResponse>(new Request(QueryType.VoltageReading), cancellationToken)).Value / (double)1000;

    /// <summary>
    /// Reads the current of the load.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task{bool}"/> whose <see cref="P:System.Threading.Tasks.Task`1.Result"/> property is the current the load currently reads.</returns>
    public async Task<double> ReadCurrent(CancellationToken cancellationToken = default)
        => (await SendRequest<IntegerResponse>(new Request(QueryType.CurrentReading), cancellationToken)).Value / (double)1000;

    /// <summary>
    /// Reads the elapsed time the load is ON.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task{bool}"/> whose <see cref="P:System.Threading.Tasks.Task`1.Result"/> property is the elapsed time since the load is on.</returns>
    public async Task<TimeSpan> ReadElapsedTime(CancellationToken cancellationToken = default)
        => (await SendRequest<TimespanResponse>(new Request(QueryType.ElapsedTime), cancellationToken)).Value;

    /// <summary>
    /// Reads the capacity in mAh the load has consumed.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task{bool}"/> whose <see cref="P:System.Threading.Tasks.Task`1.Result"/> property is the capacity the load has consumed in mAh.</returns>
    public async Task<double> ReadCapacityMiliAmpHours(CancellationToken cancellationToken = default)
        => (await SendRequest<IntegerResponse>(new Request(QueryType.CapacityMiliAmpHours), cancellationToken)).Value / (double)1000;

    /// <summary>
    /// Reads the capacity in mWh the load has consumed.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task{bool}"/> whose <see cref="P:System.Threading.Tasks.Task`1.Result"/> property is the capacity the load has consumed in mWh.</returns>
    public async Task<double> ReadCapacityMiliWattHours(CancellationToken cancellationToken = default)
        => (await SendRequest<IntegerResponse>(new Request(QueryType.CapacityMiliWattHours), cancellationToken)).Value / (double)1000;

    /// <summary>
    /// Reads the temperature of the MOSFET.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task{bool}"/> whose <see cref="P:System.Threading.Tasks.Task`1.Result"/> property is the current temperature of the MOSFET.</returns>
    public async Task<int> ReadMosfetTemperature(CancellationToken cancellationToken = default)
        => (await SendRequest<IntegerResponse>(new Request(QueryType.MosfetTemperature), cancellationToken)).Value;

    /// <summary>
    /// Reads the current setting of the load.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task{bool}"/> whose <see cref="P:System.Threading.Tasks.Task`1.Result"/> property is the current current setting of the load.</returns>
    public async Task<double> ReadCurrentSetting(CancellationToken cancellationToken = default)
        => (await SendRequest<IntegerResponse>(new Request(QueryType.CurrentSetting), cancellationToken)).Value / (double)100;

    /// <summary>
    /// Reads the cutoff voltage setting of the load.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task{bool}"/> whose <see cref="P:System.Threading.Tasks.Task`1.Result"/> property is the current voltage setting of the load.</returns>
    public async Task<double> ReadCutoffVoltageSetting(CancellationToken cancellationToken = default)
        => (await SendRequest<IntegerResponse>(new Request(QueryType.CutoffVoltageSetting), cancellationToken)).Value / (double)100;

    /// <summary>
    /// Reads the timer setting of the load.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task{bool}"/> whose <see cref="P:System.Threading.Tasks.Task`1.Result"/> property is the current timer setting of the load.</returns>
    public async Task<TimeSpan> ReadTimerSetting(CancellationToken cancellationToken = default)
        => (await SendRequest<TimespanResponse>(new Request(QueryType.TimerSetting), cancellationToken)).Value;

    /// <summary>
    /// Gets wether the serial port is open or not.
    /// </summary>
    public bool IsOpen => _serialport.IsOpen;

    /// <summary>
    /// Opens the serial port.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    public async Task Open(CancellationToken cancellationToken = default)
    {
        if (!_serialport.IsOpen)
        {
            _serialport.Open();

            // Send a probe 
            await SendRequest<IntegerResponse>(new Request(QueryType.VoltageReading), cancellationToken);
        }
    }

    /// <summary>
    /// Closes the serial port.
    /// </summary>
    public void Close()
    {
        if (_serialport.IsOpen)
        {
            _serialport.Close();
        }
    }

    private int _shuttingdown = 0;
    /// <summary>
    /// Ensures the load is off, by trying to turn it off and reconnecting if needed.
    /// </summary>
    /// <param name="retryTime">How long to keep trying to shut off the load</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous write operation.</returns>
    /// <exception cref="PanicException">Thrown when it could not be confirmed the load is off.</exception>
    public async Task EnsureLoadOff(TimeSpan retryTime)
    {
        if (Interlocked.CompareExchange(ref _shuttingdown, 1, 0) == 0)
        {
            // Try to make sure the load is off for a couple of seconds
            // Do -NOT- use an 'external' cancellation token here, we want to make sure the load is off, regardless of what the caller wants
            using var cts = new CancellationTokenSource(retryTime);
            _logger.LogInformation("Ensuring load is off");

            // Try setting the load to off for some time
            try
            {
                await ShutoffLoad(cts.Token);
            }
            catch { }

            // Try to see if load is still enabled
            var loadenabled = true;
            try
            {
                loadenabled = await IsLoadEnabled(cts.Token);
            }
            catch { }


            // If load is still on, try reconnecting and turning it off
            if (loadenabled)
            {
                _logger.LogWarning("Load is still on, trying to reconnect and turn it off");
                try
                {
                    await Reconnect();
                    await ShutoffLoad(cts.Token);
                }
                catch (Exception ex)
                {
                    throw new PanicException("Failed to turn off load", ex);
                }
            }
        }
    }

    private async Task ShutoffLoad(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && await IsLoadEnabled(cancellationToken))
        {
            await SetLoad(LoadState.Off, cancellationToken);
            await Task.Delay(100, cancellationToken);
        }
    }

    private Task Reconnect()
    {
        Close();
        return Open();
    }

    private async Task<T> SendRequest<T>(Request request, CancellationToken cancellationToken = default)
        where T : Response
    {
        var frame = request.ToFrame();

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("SENDING : {Frame}", frame.ToHex());
        }

        Exception? lastexception = null;
        for (var i = 0; i < SerialOptions.RetryCommandCount; i++)
        {
            try
            {
                await _serialport.BaseStream.WriteAsync(frame, cancellationToken);
                _serialport.DiscardInBuffer();
                await _serialport.BaseStream.FlushAsync(cancellationToken);
                var result = await ReadResponse<T>(request, cancellationToken);
                await Task.Delay(CommandPause, cancellationToken);
                return result;
            }
            catch (InvalidResponseException ex)
            {
                lastexception = ex;
                _logger.LogWarning("Invalid response, retrying");

                _serialport.DiscardInBuffer();
                _serialport.DiscardOutBuffer();
            }
        }
        throw lastexception!;
    }

    private async Task<T> ReadResponse<T>(Request request, CancellationToken cancellationToken = default)
           where T : Response
    {
        var response = new byte[request.ExpectedResponseLength];
        // Wait for data to arrive
        while (!cancellationToken.IsCancellationRequested && _serialport.BytesToRead < request.ExpectedResponseLength)
        {
            await Task.Delay(1, cancellationToken);
        }
        // Read the data
        await _serialport.BaseStream.ReadExactlyAsync(response, cancellationToken);
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("RECEIVED : {Frame}", response.ToHex());
        }


        if (request.Type < 0x10)
        {
            return (T)(Response)ParseCommandResponse(response);
        }
        else if (request.Type < 0x20)
        {
            return (T)(Response)ParseQueryResponse(request.Type, response);
        }
        throw new InvalidRequestTypeException(request.Type);
    }

    private static CommandResponse ParseCommandResponse(byte[] response)
        => response[0] switch
        {
            0x6F => new OkResponse(response),
            _ => new ErrorResponse(response)
        };

    private static QueryResponse ParseQueryResponse(byte type, byte[] response)
    {
        if (response[0] == 0xCA && response[1] == 0xCB && response[^2] == 0xCE && response[^1] == 0xCF)
        {
            var value = response[2..^2];
            return type switch
            {
                (byte)QueryType.LoadEnabled => new BoolResponse(value),
                (byte)QueryType.VoltageReading => new IntegerResponse(value),
                (byte)QueryType.CurrentReading => new IntegerResponse(value),
                (byte)QueryType.ElapsedTime => new TimespanResponse(value),
                (byte)QueryType.CapacityMiliAmpHours => new IntegerResponse(value),
                (byte)QueryType.CapacityMiliWattHours => new IntegerResponse(value),
                (byte)QueryType.MosfetTemperature => new IntegerResponse(value),
                (byte)QueryType.CurrentSetting => new IntegerResponse(value),
                (byte)QueryType.CutoffVoltageSetting => new IntegerResponse(value),
                (byte)QueryType.TimerSetting => new TimespanResponse(value),
                _ => throw new InvalidQueryTypeException(type)
            };
        }
        throw new InvalidResponseException(type, response);
    }
}
