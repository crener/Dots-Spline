using Crener.Spline.BaseSpline;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline.Linear.Common
{
    public abstract class Linear2DBase : BaseSpline2D, ILoopingSpline
    {
        [SerializeField]
        protected bool looped = false;

        public bool Looped
        {
            get => looped;
            set
            {
                looped = value;
                RecalculateLengthBias();
            }
        }

        public override SplineType SplineDataType
        {
            get
            {
                if(ControlPointCount == 0) return SplineType.Empty;
                if(ControlPointCount == 1) return SplineType.Single;
                if(ControlPointCount == 2) return SplineType.Linear;
                return SplineType.CubicLinear;
            }
        }
        public override int SegmentPointCount => Looped ? ControlPointCount + 1 : ControlPointCount - 1;


        protected override void RecalculateLengthBias()
        {
            ClearData();
            SegmentLength.Clear();

            if(ControlPointCount <= 1)
            {
                LengthCache = 0f;
                SegmentLength.Add(1f);
                return;
            }

            if(ControlPointCount == 2)
            {
                LengthCache = math.distance(Points[0], Points[1]);
                SegmentLength.Add(1f);
                return;
            }

            // fallback to known good code
            base.RecalculateLengthBias();
        }
    }
}