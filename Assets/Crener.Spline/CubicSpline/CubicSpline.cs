using System;
using Unity.Mathematics;

namespace Crener.Spline.CubicSpline
{
    // Based on the MIT licenced ScottPlot library
    // https://github.com/swharden/ScottPlot/tree/404105e5d7ae8399b2e40e9bd64b246d3b3b80dd/src/ScottPlot/Statistics/Interpolation

    public class CubicSpline
    {
        public float2[] Given;
        public float2[] Interpolated;

        private Matrix m;

        private int m_n;
        protected float[] a, b, c, d, h;

        public CubicSpline(float2[] points, int resolution = 10, int order = 2)
        {
            if(points is null)
                throw new ArgumentException("xs and ys cannot be null");

            if(points.Length < 4)
                throw new ArgumentException("xs and ys must have a length of 4 or greater");

            if(resolution < 1)
                throw new ArgumentException("resolution must be 1 or greater");

            Given = points;
            m_n = points.Length;

            Interpolated = new float2[m_n * resolution];
            m = new Matrix(m_n - 2);

            a = new float[m_n];
            b = new float[m_n];
            c = new float[m_n];
            d = new float[m_n];
            h = new float[m_n - 1];

            CalcParameters();
            Interpolate();
        }

        private void CalcParameters()
        {
            for (int i = 0; i < m_n; i++)
                a[i] = Given[i].y;

            for (int i = 0; i < m_n - 1; i++)
                h[i] = Given[i + 1].x - Given[i].x;

            for (int i = 0; i < m_n - 2; i++)
            {
                for (int k = 0; k < m_n - 2; k++)
                {
                    m.a[i, k] = 0;
                    m.y[i] = 0;
                    m.x[i] = 0;
                }
            }

            for (int i = 0; i < m_n - 2; i++)
            {
                if(i == 0)
                {
                    m.a[i, 0] = 2f * (h[0] + h[1]);
                    m.a[i, 1] = h[1];
                }
                else
                {
                    m.a[i, i - 1] = h[i];
                    m.a[i, i] = 2f * (h[i] + h[i + 1]);
                    if(i < m_n - 3)
                        m.a[i, i + 1] = h[i + 1];
                }

                if((h[i] != 0) && (h[i + 1] != 0))
                    m.y[i] = ((a[i + 2] - a[i + 1]) / h[i + 1] - (a[i + 1] - a[i]) / h[i]) * 3f;
                else
                    m.y[i] = 0f;
            }

            if(m.Eliminate() == false)
                throw new InvalidOperationException("error in matrix calculation");

            m.Solve();

            c[0] = 0f;
            c[m_n - 1] = 0f;

            for (int i = 1; i < m_n - 1; i++)
                c[i] = m.x[i - 1];

            for (int i = 0; i < m_n - 1; i++)
            {
                if(h[i] != 0.0)
                {
                    d[i] = 1f / 3f / h[i] * (c[i + 1] - c[i]);
                    b[i] = 1f / h[i] * (a[i + 1] - a[i]) - h[i] / 3f * (c[i + 1] + 2f * c[i]);
                }
            }
        }

        private void Interpolate()
        {
            int resolution = Interpolated.Length / m_n;
            for (int i = 0; i < h.Length; i++)
            {
                for (int k = 0; k < resolution; k++)
                {
                    float deltaX = (float) k / resolution * h[i];
                    float termA = a[i];
                    float termB = b[i] * deltaX;
                    float termC = c[i] * deltaX * deltaX;
                    float termD = d[i] * deltaX * deltaX * deltaX;
                    
                    int interpolatedIndex = i * resolution + k;
                    Interpolated[interpolatedIndex] = new float2(
                        deltaX + Given[i].x,
                        termA + termB + termC + termD);
                }
            }

            // After interpolation the last several values of the interpolated arrays
            // contain uninitialized data. This section identifies the values which are
            // populated with values and copies just the useful data into new arrays.
            int pointsToKeep = resolution * (m_n - 1) + 1;
            float2[] interpolatedCopy = new float2[pointsToKeep];
            Array.Copy(Interpolated, 0, interpolatedCopy, 0, pointsToKeep - 1);
            Interpolated = interpolatedCopy;
            Interpolated[pointsToKeep - 1] = Given[m_n - 1];
        }
    }
}