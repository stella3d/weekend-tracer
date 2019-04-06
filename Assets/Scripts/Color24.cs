using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct Color24
{
    public byte r;
    public byte g;
    public byte b;

    public Color24(byte r, byte g, byte b)
    {
        this.r = r;
        this.g = g;
        this.b = b;
    }
}
