using System;
using System.Numerics;

namespace DALib.Utility;

/// <summary>
///     An extension of the Math class, providing additional math-related methods
/// </summary>
public static class MathEx
{
    /// <summary>
    ///     Scales a number from one range to another range.
    /// </summary>
    /// <param name="num">
    ///     The input number to be scaled.
    /// </param>
    /// <param name="min">
    ///     The lower bound of the original range.
    /// </param>
    /// <param name="max">
    ///     The upper bound of the original range.
    /// </param>
    /// <param name="newMin">
    ///     The lower bound of the new range.
    /// </param>
    /// <param name="newMax">
    ///     The upper bound of the new range.
    /// </param>
    /// <returns>
    ///     The scaled number in the new range.
    /// </returns>
    /// <remarks>
    ///     This method assumes that the input number is within the original range. No clamping or checking is performed.
    /// </remarks>
    public static T2 ScaleRange<T1, T2>(
        T1 num,
        T1 min,
        T1 max,
        T2 newMin,
        T2 newMax) where T1: INumber<T1>
                   where T2: INumber<T2>
    {
        var ret = ScaleRange(
            double.CreateTruncating(num),
            double.CreateTruncating(min),
            double.CreateTruncating(max),
            double.CreateTruncating(newMin),
            double.CreateTruncating(newMax));

        //get a rounded value and a truncated value
        var roundedValue = Math.Round(ret, MidpointRounding.AwayFromZero);
        var truncatedValue = T2.CreateTruncating(ret);

        //take whichever value is closer to the true value
        var roundedDiff = Math.Abs(roundedValue - ret);
        var truncatedDiff = Math.Abs(double.CreateTruncating(truncatedValue) - ret);

        if (roundedDiff < truncatedDiff)
            return T2.CreateTruncating(roundedValue);

        return truncatedValue;
    }

    /// <summary>
    ///     Scales a number from one range to another range.
    /// </summary>
    /// <param name="num">
    ///     The input number to be scaled.
    /// </param>
    /// <param name="min">
    ///     The lower bound of the original range.
    /// </param>
    /// <param name="max">
    ///     The upper bound of the original range.
    /// </param>
    /// <param name="newMin">
    ///     The lower bound of the new range.
    /// </param>
    /// <param name="newMax">
    ///     The upper bound of the new range.
    /// </param>
    /// <returns>
    ///     The scaled number in the new range.
    /// </returns>
    /// <remarks>
    ///     This method assumes that the input number is within the original range. No clamping or checking is performed.
    /// </remarks>
    public static double ScaleRange(
        double num,
        double min,
        double max,
        double newMin,
        double newMax)
    {
        if (min.Equals(max))
            throw new ArgumentOutOfRangeException(nameof(min), "Min and max cannot be the same value");

        return (newMax - newMin) * (num - min) / (max - min) + newMin;
    }
}