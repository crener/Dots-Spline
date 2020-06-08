namespace Crener.Spline.Common.Interfaces
{
    /// <summary>
    /// Spline which is able to take the direct spline input and convert it into a Point to Point spline 
    /// </summary>
    public interface IArkableSpline : ISpline
    {
        bool ArkParameterization { get; set; }
        
        float ArkLength { get; set; }
    }
}