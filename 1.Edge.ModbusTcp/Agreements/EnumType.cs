namespace Edge.ModbusTcp.Agreements
{
    internal enum FunctionCode
    {
        Undefined = 0,
        ReadCoilStatus = 1,
        ReadInputStatus = 2,
        ReadHolding = 3,
        ReadInputRegisters = 4,
        ForceSingleCoil = 5,
        PresetSingleRegister = 6
    }
}
