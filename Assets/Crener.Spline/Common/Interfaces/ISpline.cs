namespace Crener.Spline.Common.Interfaces
{
    /// <summary>
    /// basic spline behaviour
    /// </summary>
    public interface ISpline
    {
        /// <summary>
        /// Amount of control points in the spline
        /// </summary>
        int ControlPointCount { get; }

        /// <summary>
        /// Length of the spline
        /// </summary>
        float Length();
        
        /// <summary>
        /// Cleans up the spline data by disposing any disposable types 
        /// </summary>
        void ClearData();
    }
}