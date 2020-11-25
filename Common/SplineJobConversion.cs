using Crener.Spline._2D.Jobs;
using Crener.Spline._3D.Jobs;
using Crener.Spline.Common.DataStructs;
using Crener.Spline.Common.Interfaces;
using Unity.Collections;

namespace Crener.Spline.Common
{
    public static class SplineJobConversion
    {
        #region Extract Job
        #region 2D
        /// <summary>
        /// Create a spline job from this spline
        /// </summary>
        /// <param name="spline">spline to generate a job from</param>
        /// <param name="progress">progress to use when initializing job</param>
        /// <param name="allocator">allocator to use for result values</param>
        /// <returns>spline point retrieval job</returns>
        public static ISplineJob2D ExtractJob(this ISpline2D spline, float progress, Allocator allocator = Allocator.None)
        {
            return ExtractJob(spline, new SplineProgress(progress), allocator);
        }

        /// <summary>
        /// Create a spline job from this spline
        /// </summary>
        /// <param name="spline">spline to generate a job from</param>
        /// <param name="progress">progress to use when initializing job</param>
        /// <param name="allocator">allocator to use for result values</param>
        /// <returns>spline point retrieval job</returns>
        public static ISplineJob2D ExtractJob(this ISpline2D spline, SplineProgress progress = default, Allocator allocator = Allocator.None)
        {
            switch (spline.SplineDataType)
            {
                case SplineType.Empty:
                    return new Empty2DPointJob(allocator);
                case SplineType.Single:
                    return new SinglePoint2DPointJob(spline, allocator);
                case SplineType.Bezier:
                    return new BezierSpline2DPointJob(spline, progress, allocator);
                case SplineType.CubicLinear:
                    return new LinearCubicSpline2DPointJob(spline, progress, allocator);
                case SplineType.Cubic:
                //todo
                case SplineType.BSpline:
                //todo
                case SplineType.CatmullRom:
                    return new CatmullRomSpline2DPointJob(spline, progress, allocator);
                case SplineType.Linear: // falls over to the default by design
                default:
                    return new LinearSpline2DPointJob(spline, progress, allocator);
            }
        }

        #endregion 2D
        #region 3D

        /// <summary>
        /// Create a spline job from this spline
        /// </summary>
        /// <param name="spline">spline to generate a job from</param>
        /// <param name="progress">progress to use when initializing job</param>
        /// <param name="allocator">allocator to use for result values</param>
        /// <returns>spline point retrieval job</returns>
        public static ISplineJob3D ExtractJob(this ISpline3D spline, float progress, Allocator allocator = Allocator.None)
        {
            return ExtractJob(spline, new SplineProgress(progress), allocator);
        }

        /// <summary>
        /// Create a spline job from this spline
        /// </summary>
        /// <param name="spline">spline to generate a job from</param>
        /// <param name="progress">progress to use when initializing job</param>
        /// <param name="allocator">allocator to use for result values</param>
        /// <returns>spline point retrieval job</returns>
        public static ISplineJob3D ExtractJob(this ISpline3D spline, SplineProgress progress = default, Allocator allocator = Allocator.None)
        {
            switch (spline.SplineDataType)
            {
                case SplineType.Empty:
                    return new Empty3DPointJob(spline, allocator);
                case SplineType.Single:
                    return new SinglePoint3DPointJob(spline, allocator);
                case SplineType.Bezier:
                    return new BezierSpline3DPointJob(spline, progress, allocator);
                case SplineType.CubicLinear:
                    return new LinearCubicSpline3DPointJob(spline, progress, allocator);
                case SplineType.Cubic:
                //todo
                case SplineType.BSpline:
                //todo
                case SplineType.CatmullRom:
                //todo
                case SplineType.Linear: // falls over to the default by design
                default:
                    return new LinearSpline3DPointJob(spline, progress, allocator);
            }
        }
        #endregion 3D
        #endregion Extract Job
    }
}