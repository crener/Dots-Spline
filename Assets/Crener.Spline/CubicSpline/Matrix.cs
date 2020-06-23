using Unity.Mathematics;

namespace Crener.Spline.CubicSpline
{
    public class Matrix
    {
        public readonly int maxOrder;
        public bool calcError;
        public float[,] a;
        public float[] y;
        public float[] x;

        public Matrix(int size) : this(size, size) { }
        
        public Matrix(int size, int order)
        {
            maxOrder = order;

            a = new float[size, size];
            y = new float[size];
            x = new float[size];
        }

        void SwitchRows(int n)
        {
            float tempD;
            int i, j;
            for (i = n; i <= maxOrder - 2; i++)
            {
                for (j = 0; j <= maxOrder - 1; j++)
                {
                    tempD = a[i, j];
                    a[i, j] = a[i + 1, j];
                    a[i + 1, j] = tempD;
                }

                tempD = y[i];
                y[i] = y[i + 1];
                y[i + 1] = tempD;
            }
        }

        public bool Eliminate()
        {
            int i, k, l;
            calcError = false;
            for (k = 0; k <= maxOrder - 2; k++)
            {
                for (i = k; i <= maxOrder - 2; i++)
                {
                    if(math.abs(a[i + 1, i]) < 1e-8)
                    {
                        SwitchRows(i + 1);
                    }

                    if(a[i + 1, k] != 0.0)
                    {
                        for (l = k + 1; l <= maxOrder - 1; l++)
                        {
                            if(!calcError)
                            {
                                a[i + 1, l] = a[i + 1, l] * a[k, k] - a[k, l] * a[i + 1, k];
                                if(a[i + 1, l] > 10E260)
                                {
                                    a[i + 1, k] = 0;
                                    calcError = true;
                                }
                            }
                        }

                        y[i + 1] = y[i + 1] * a[k, k] - y[k] * a[i + 1, k];
                        a[i + 1, k] = 0;
                    }
                }
            }

            return !calcError;
        }

        public void Solve()
        {
            int k, l;
            for (k = maxOrder - 1; k >= 0; k--)
            {
                for (l = maxOrder - 1; l >= k; l--)
                {
                    y[k] = y[k] - x[l] * a[k, l];
                }

                if(a[k, k] != 0)
                    x[k] = y[k] / a[k, k];
                else
                    x[k] = 0;
            }
        }
    }
}