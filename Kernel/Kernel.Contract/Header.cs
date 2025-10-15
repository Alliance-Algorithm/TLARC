namespace Kernel.Contract;


public struct Timestamp
{
    public int  Second;
    public uint Nanosecond;
}

public struct Header
{
    /// <summary>
    /// Transform id
    /// </summary>
    public string       Identifier;    
    public Timestamp    Timestamp;
}