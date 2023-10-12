namespace State_Estimation.Foundation
{
    public class GaussNewton
    {
        /// <summary>
        /// Матрица измерений
        /// </summary>
        public Matrix Measure { get; private set; }

        /// <summary>
        /// Вектор состояния
        /// </summary>
        public Matrix State { get; private set; }

        public GaussNewton(Matrix state, Matrix measure)
        {
            State = state;
            Measure = measure;
        }


        public (Matrix target, double error) Calculate(Matrix calc, Matrix c, Matrix j)
        {
            var F = calc - Measure;
            
            
            var Ft = Matrix.Transpose(F);
            var target = Matrix.Multiply(0.5, Ft) * c * F;
            var H = Matrix.Transpose(j) * c * j;
            var grad = Matrix.Transpose(j) * c * F;
            var deltaU = H.Invert() * (-grad);
            var error = Matrix.MaxElement(deltaU);
            State += Matrix.Multiply(1, deltaU);
            return (target, error);
        }
    }
}