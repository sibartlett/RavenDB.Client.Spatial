using System;

namespace Raven.Client.Spatial
{
    [Flags]
    internal enum Dimensions
    {
        XY = 0,
        Z = 1,
        M = 2,
    }
}