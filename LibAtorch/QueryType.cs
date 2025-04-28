namespace LibAtorch;

internal enum QueryType : byte
{
    LoadEnabled = 0x10,
    VoltageReading = 0x11,
    CurrentReading = 0x12,
    ElapsedTime = 0x13,
    CapacityMilliAmpHours = 0x14,
    CapacityMilliWattHours = 0x15,
    MosfetTemperature = 0x16,
    CurrentSetting = 0x17,
    CutoffVoltageSetting = 0x18,
    TimerSetting = 0x19
}