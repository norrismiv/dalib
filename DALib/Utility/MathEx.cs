using System.Numerics;

namespace DALib.Utility;

public static class MathEx
{
    public static T2 ScaleRange<T1, T2>(
        T1 num,
        T1 min,
        T1 max,
        T2 newMin,
        T2 newMax
    ) where T1: INumber<T1>
      where T2: INumber<T2> =>
        T2.CreateTruncating(
            ScaleRange(
                double.CreateTruncating(num),
                double.CreateTruncating(min),
                double.CreateTruncating(max),
                double.CreateTruncating(newMin),
                double.CreateTruncating(newMax)));
}