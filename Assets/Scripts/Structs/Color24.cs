using System.Runtime.InteropServices;
using UnityEngine;

namespace RayTracingWeekend
{
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

        public static implicit operator Color24(Color32 c)
        {
            return new Color24(c.r, c.g, c.b);
        }
        
        public static implicit operator Color32(Color24 c)
        {
            return new Color32(c.r, c.g, c.b, 255);
        }
    }
}