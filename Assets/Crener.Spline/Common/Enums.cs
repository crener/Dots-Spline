namespace Crener.Spline.Common
{
    /// <summary>
    /// Type of bezier point that is represented 
    /// </summary>
    public enum SplinePoint
    {
        Point = 0,
        Post = 1,
        Pre = 2,
    }

    /// <summary>
    /// Type of bezier point that is represented in a variance spline
    /// </summary>
    public enum SplinePointVariance
    {
        Point = SplinePoint.Point,
        Post = SplinePoint.Post,
        Pre = SplinePoint.Pre,

        PostLeft,
        PointLeft,
        PreLeft,

        PostRight,
        PointRight,
        PreRight,
    }

    /// <summary>
    /// Way in which control points should be modified
    /// </summary>
    public enum SplineEditMode
    {
        Standard,
        Mirror,
        Free
    }
    
    /// <summary>
    /// Used by variance splines to denote array offsets for spline sides 
    /// </summary>
    internal enum SplineSide
    {
        Center = 0,
        Left = 1,
        Right = 2
    }

    public enum SplineType
    {
        Bezier,
        PointToPoint,
        BSpline
    }

    public static class SplineEnumExtensions
    {
        /// <summary>
        /// Swaps the left point variance with right and vice versa
        /// </summary>
        /// <param name="original">point to find the opposite for</param>
        /// <returns>opposite point</returns>
        public static SplinePointVariance OppositeLR(this SplinePointVariance original)
        {
            if(original == SplinePointVariance.PointLeft) return SplinePointVariance.PointRight;
            if(original == SplinePointVariance.PreLeft) return SplinePointVariance.PreRight;
            if(original == SplinePointVariance.PostLeft) return SplinePointVariance.PostRight;

            if(original == SplinePointVariance.PointRight) return SplinePointVariance.PointLeft;
            if(original == SplinePointVariance.PreRight) return SplinePointVariance.PreLeft;
            if(original == SplinePointVariance.PostRight) return SplinePointVariance.PostLeft;

            return original;
        }

        /// <summary>
        /// Swaps the pre point variance with post and vice versa
        /// </summary>
        /// <param name="original">point to find the opposite for</param>
        /// <returns>opposite point</returns>
        public static SplinePointVariance OppositePrePost(this SplinePointVariance original)
        {
            if(original == SplinePointVariance.Pre) return SplinePointVariance.Post;
            if(original == SplinePointVariance.Post) return SplinePointVariance.Pre;

            if(original == SplinePointVariance.PreLeft) return SplinePointVariance.PostLeft;
            if(original == SplinePointVariance.PostLeft) return SplinePointVariance.PreLeft;

            if(original == SplinePointVariance.PreRight) return SplinePointVariance.PostRight;
            if(original == SplinePointVariance.PostRight) return SplinePointVariance.PreRight;

            return original;
        }

        /// <summary>
        /// Convert a point to the pre variance, returns same point if it cannot be converted
        /// </summary>
        /// <param name="from">point to convert</param>
        /// <returns>pre variance version</returns>
        public static SplinePointVariance ToPre(this SplinePointVariance from)
        {
            if(from == SplinePointVariance.PointLeft || from == SplinePointVariance.PostLeft) return SplinePointVariance.PreLeft;
            if(from == SplinePointVariance.PointRight || from == SplinePointVariance.PostRight) return SplinePointVariance.PreRight;
            if(from == SplinePointVariance.Point || from == SplinePointVariance.Post) return SplinePointVariance.Pre;

            return from;
        }

        /// <summary>
        /// Convert a point to the post variance, returns same point if it cannot be converted
        /// </summary>
        /// <param name="from">point to convert</param>
        /// <returns>post variance version</returns>
        public static SplinePointVariance ToPost(this SplinePointVariance from)
        {
            if(from == SplinePointVariance.PointLeft || from == SplinePointVariance.PreLeft) return SplinePointVariance.PostLeft;
            if(from == SplinePointVariance.PointRight || from == SplinePointVariance.PreRight) return SplinePointVariance.PostRight;
            if(from == SplinePointVariance.Point || from == SplinePointVariance.Pre) return SplinePointVariance.Post;

            return from;
        }

        /// <summary>
        /// Is a point a post point
        /// </summary>
        /// <param name="from">point to check</param>
        /// <returns>true if the point is a post variant</returns>
        public static bool isPost(this SplinePointVariance from)
        {
            return from == SplinePointVariance.Post ||
                   from == SplinePointVariance.PostLeft ||
                   from == SplinePointVariance.PostRight;
        }

        /// <summary>
        /// Is a point a pre point
        /// </summary>
        /// <param name="from">point to check</param>
        /// <returns>true if the point is a pre variant</returns>
        public static bool isPre(this SplinePointVariance from)
        {
            return from == SplinePointVariance.Pre ||
                   from == SplinePointVariance.PreLeft ||
                   from == SplinePointVariance.PreRight;
        }
    }
}