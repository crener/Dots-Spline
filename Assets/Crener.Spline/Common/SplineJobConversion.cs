using System;
using Crener.Spline._2D.Jobs;
using Crener.Spline._3D.Jobs;
using Crener.Spline.Common.DataStructs;
using Crener.Spline.Common.Interfaces;
using Unity.Jobs;

namespace Crener.Spline.Common
{
    public static class SplineJobConversion
    {
        #region Extract Job
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
                    return new SinglePoint2DPointJob {Spline = spline.SplineEntityData2D.Value};
                case SplineType.Bezier:
                    return new BezierSpline2DPointJob {Spline = spline.SplineEntityData2D.Value};
                case SplineType.CubicLinear:
                    return new LinearCubicSpline2DPointJob {Spline = spline.SplineEntityData2D.Value};
                case SplineType.Cubic:
                //todo
                case SplineType.BSpline:
                //todo
                case SplineType.CatmullRom:
                    return new CatmullRomSpline2DPointJob {Spline = spline.SplineEntityData2D.Value};
                case SplineType.Linear: // falls over to the default by design
                default:
                    return new LinearSpline2DPointJob {Spline = spline.SplineEntityData2D.Value};
            }
        }

        /// <summary>
        /// Create a spline job from this spline
        /// </summary>
        /// <param name="spline">spline to generate a job from</param>
        /// <param name="progress">progress to use when initializing job</param>
        /// <returns>spline point retrieval job</returns>
        public static ISplineJob3D ExtractJob(this ISpline3D spline, float progress)
        {
            return ExtractJob(spline, new SplineProgress(progress));
        }

        /// <summary>
        /// Create a spline job from this spline
        /// </summary>
        /// <param name="spline">spline to generate a job from</param>
        /// <param name="progress">progress to use when initializing job</param>
        /// <returns>spline point retrieval job</returns>
        public static ISplineJob3D ExtractJob(this ISpline3D spline, SplineProgress progress)
        {
            ISplineJob3D splineJob = ExtractJob(spline);
            splineJob.SplineProgress = progress;

            return splineJob;
        }

        /// <summary>
        /// Create a spline job from this spline
        /// </summary>
        /// <param name="spline">spline to generate a job from</param>
        /// <returns>spline point retrieval job</returns>
        public static ISplineJob3D ExtractJob(this ISpline3D spline)
        {
            switch (spline.SplineDataType)
            {
                case SplineType.Empty:
                    return new Empty3DPointJob();
                case SplineType.Single:
                    return new SinglePoint3DPointJob {Spline = spline.SplineEntityData3D.Value};
                case SplineType.Bezier:
                    return new BezierSpline3DPointJob {Spline = spline.SplineEntityData3D.Value};
                case SplineType.CubicLinear:
                    return new LinearCubicSpline3DPointJob {Spline = spline.SplineEntityData3D.Value};
                case SplineType.Cubic:
                //todo
                case SplineType.BSpline:
                //todo
                case SplineType.CatmullRom:
                //todo
                case SplineType.Linear: // falls over to the default by design
                default:
                    return new LinearSpline3DPointJob {Spline = spline.SplineEntityData3D.Value};
            }
        }
        #endregion Extract Job

        #region Schedule Job
        /// <summary>
        /// Create a spline job from this spline
        /// </summary>
        /// <param name="spline">spline to generate a job from</param>
        /// <param name="progress">progress to use when initializing job</param>
        /// <param name="dependsOn">attach <paramref name="dependsOn"/> to the newly scheduled job</param>
        /// <returns>spline point retrieval job</returns>
        public static Tuple<ISplineJob2D, JobHandle> ScheduleJob(this ISpline2D spline, float progress, JobHandle dependsOn = default)
        {
            return ScheduleJob(spline, new SplineProgress(progress), dependsOn);
        }

        /// <summary>
        /// Create a spline job from this spline
        /// </summary>
        /// <param name="spline">spline to generate a job from</param>
        /// <param name="progress">progress to use when initializing job</param>
        /// <param name="dependsOn">attach <paramref name="dependsOn"/> to the newly scheduled job</param>
        /// <returns>spline point retrieval job</returns>
        public static Tuple<ISplineJob2D, JobHandle> ScheduleJob(this ISpline2D spline, SplineProgress progress, JobHandle dependsOn = default)
        {
            switch (spline.SplineDataType)
            {
                case SplineType.Empty:
                {
                    Empty2DPointJob job = new Empty2DPointJob {SplineProgress = progress};
                    return new Tuple<ISplineJob2D, JobHandle>(job, job.Schedule(dependsOn));
                }
                case SplineType.Single:
                {
                    SinglePoint2DPointJob job = new SinglePoint2DPointJob
                    {
                        Spline = spline.SplineEntityData2D.Value,
                        SplineProgress = progress
                    };
                    return new Tuple<ISplineJob2D, JobHandle>(job, job.Schedule(dependsOn));
                }
                case SplineType.Bezier:
                {
                    BezierSpline2DPointJob job = new BezierSpline2DPointJob
                    {
                        Spline = spline.SplineEntityData2D.Value,
                        SplineProgress = progress
                    };
                    return new Tuple<ISplineJob2D, JobHandle>(job, job.Schedule(dependsOn));
                }
                case SplineType.CubicLinear:
                {
                    LinearCubicSpline2DPointJob job = new LinearCubicSpline2DPointJob
                    {
                        Spline = spline.SplineEntityData2D.Value,
                        SplineProgress = progress
                    };
                    return new Tuple<ISplineJob2D, JobHandle>(job, job.Schedule(dependsOn));
                }
                case SplineType.Cubic:
                //todo
                case SplineType.BSpline:
                //todo
                case SplineType.CatmullRom:
                {
                    CatmullRomSpline2DPointJob job = new CatmullRomSpline2DPointJob
                    {
                        Spline = spline.SplineEntityData2D.Value,
                        SplineProgress = progress
                    };
                    return new Tuple<ISplineJob2D, JobHandle>(job, job.Schedule(dependsOn));
                }
                case SplineType.Linear: // falls over to the default by design
                default:
                {
                    LinearSpline2DPointJob job = new LinearSpline2DPointJob
                    {
                        Spline = spline.SplineEntityData2D.Value,
                        SplineProgress = progress
                    };
                    return new Tuple<ISplineJob2D, JobHandle>(job, job.Schedule(dependsOn));
                }
            }
        }

        /// <summary>
        /// Create a spline job from this spline
        /// </summary>
        /// <param name="spline">spline to generate a job from</param>
        /// <param name="progress">progress to use when initializing job</param>
        /// <param name="dependsOn">attach <paramref name="dependsOn"/> to the newly scheduled job</param>
        /// <returns>spline point retrieval job</returns>
        public static Tuple<ISplineJob3D, JobHandle> ScheduleJob(this ISpline3D spline, float progress, JobHandle dependsOn = default)
        {
            return ScheduleJob(spline, new SplineProgress(progress));
        }

        /// <summary>
        /// Create a spline job from this spline
        /// </summary>
        /// <param name="spline">spline to generate a job from</param>
        /// <param name="progress">progress to use when initializing job</param>
        /// <param name="dependsOn">attach <paramref name="dependsOn"/> to the newly scheduled job</param>
        /// <returns>spline point retrieval job</returns>
        public static Tuple<ISplineJob3D, JobHandle> ScheduleJob(this ISpline3D spline, SplineProgress progress, JobHandle dependsOn = default)
        {
            switch (spline.SplineDataType)
            {
                case SplineType.Empty:
                {
                    Empty3DPointJob job = new Empty3DPointJob {SplineProgress = progress};
                    return new Tuple<ISplineJob3D, JobHandle>(job, job.Schedule(dependsOn));
                }
                case SplineType.Single:
                {
                    SinglePoint3DPointJob job = new SinglePoint3DPointJob
                    {
                        Spline = spline.SplineEntityData3D.Value,
                        SplineProgress = progress
                    };
                    return new Tuple<ISplineJob3D, JobHandle>(job, job.Schedule(dependsOn));
                }
                case SplineType.Bezier:
                {
                    BezierSpline3DPointJob job = new BezierSpline3DPointJob
                    {
                        Spline = spline.SplineEntityData3D.Value,
                        SplineProgress = progress
                    };
                    return new Tuple<ISplineJob3D, JobHandle>(job, job.Schedule(dependsOn));
                }
                case SplineType.CubicLinear:
                {
                    LinearCubicSpline3DPointJob job = new LinearCubicSpline3DPointJob
                    {
                        Spline = spline.SplineEntityData3D.Value, SplineProgress = progress
                    };
                    return new Tuple<ISplineJob3D, JobHandle>(job, job.Schedule(dependsOn));
                }
                case SplineType.Cubic:
                //todo
                case SplineType.BSpline:
                //todo
                case SplineType.CatmullRom:
                //todo
                case SplineType.Linear: // falls over to the default by design
                default:
                {
                    LinearSpline3DPointJob job = new LinearSpline3DPointJob
                    {
                        Spline = spline.SplineEntityData3D.Value,
                        SplineProgress = progress
                    };
                    return new Tuple<ISplineJob3D, JobHandle>(job, job.Schedule(dependsOn));
                }
            }
        }
        #endregion Schedule Job
    }
}