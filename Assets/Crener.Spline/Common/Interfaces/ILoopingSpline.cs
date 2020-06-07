using System;

namespace Crener.Spline.Common.Interfaces
{
    public interface ILoopingSpline : ISpline
    {
        /// <summary>
        /// Is the spline looped
        /// </summary>
        bool Looped { get; }
    }
}