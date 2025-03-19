namespace LibAtorch;

internal enum CommandType : byte
{
    ToggleLoad = 0x01,
    SetCurrent = 0x02,
    SetCutoffVoltage = 0x03,
    SetTimeout = 0x04,
    ResetCounters = 0x05
}