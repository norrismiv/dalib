using System.Runtime.InteropServices;

namespace DALib.Utility;

internal static class Helpers
{
    internal static void FreeHandle(nint address, object context)
    {
        var handle = (GCHandle)context;
        handle.Free();
    }
}