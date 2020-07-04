using Crener.Spline.BezierSpline.Jobs;
using Crener.Spline.Common.DataStructs;
using Crener.Spline.Common.Interfaces;
using Crener.Spline.Linear.Jobs._2D;

namespace Crener.Spline.Common
{
    public static class SplineJobConversion
    {
        /// <summary>
        /// Create a spline job from this spline
        /// </summary>
        /// <param name="spline">spline to generate a job from</param>
        /// <param name="progress">progress to use when initializing job</param>
        /// <returns>spline point retrieval job</returns>
        public static ISplineJob2D ExtractJob(this ISpline2D spline, float progress)
        {
            return ExtractJob(spline, new SplineProgress(progress));
        }

        /// <summary>
        /// Create a spline job from this spline
        /// </summary>
        /// <param name="spline">spline to generate a job from</param>
        /// <param name="progress">progress to use when initializing job</param>
        /// <returns>spline point retrieval job</returns>
        public static ISplineJob2D ExtractJob(this ISpline2D spline, SplineProgress progress)
        {
            ISplineJob2D splineJob = ExtractJob(spline);
            splineJob.SplineProgress = progress;

            return splineJob;
        }

        /// <summary>
        /// Create a spline job from this spline
        /// </summary>
        /// <param name="spline">spline to generate a job from</param>
        /// <returns>spline point retrieval job</returns>
        public static ISplineJob2D ExtractJob(this ISpline2D spline)
        {
            switch (spline.SplineDataType)
            {
                case SplineType.Empty:
                    return new Empty2DPointJob();
                case SplineType.Single:
                    return new SinglePoint2DPointJob {Spline = spline.SplineEntityData.Value};
                case SplineType.Bezier:
                    return new BezierSpline2DPointJob {Spline = spline.SplineEntityData.Value};
                case SplineType.CubicLinear:
                    return new LinearCubicSpline2DPointJob {Spline = spline.SplineEntityData.Value};
                case SplineType.Cubic:
                //todo
                case SplineType.BSpline:
                //todo
                case SplineType.CatmullRom:
                //todo
                case SplineType.Linear: // falls over to the default by design
                default:
                    return new LinearSpline2DPointJob {Spline = spline.SplineEntityData.Value};
            }
        }
    }
}