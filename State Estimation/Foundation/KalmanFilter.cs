namespace State_Estimation.Foundation
{
    /// <summary>
    /// Фильтр Калмана
    /// </summary>
    public class KalmanFilter
    {
        /// <summary>
        /// Предсказание вектора сосояния
        /// </summary>
        public Matrix X0 { get; private set; }

        /// <summary>
        /// Предсказание ковариации
        /// </summary>
        public Matrix P0 { get; private set; }

        /// <summary>
        /// Матрица перехода
        /// </summary>
        public Matrix F { get; private set; }

        /// <summary>
        /// Управляющее воздействие
        /// </summary>
        public Matrix B { get; private set; }



        /// <summary>
        /// Ковариация шума процесса
        /// </summary>
        public Matrix C { get; private set; }

        /// <summary>
        /// Вектор состояния
        /// </summary>
        public Matrix State { get; private set; }

        /// <summary>
        /// Коварицаия
        /// </summary>
        public Matrix Covariance { get; private set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="b">Управляющее воздействие</param>
        /// <param name="q">Шум измерений</param>
        /// <param name="f">Матрица перехода</param>
        /// <param name="j">Матрица Якоби</param>
        /// <param name="c">Ковариация шума процесса</param>
        /// <param name="state"></param>
        /// <param name="covariance"></param>
        public KalmanFilter(Matrix b, Matrix c, Matrix f,  Matrix state, Matrix covariance)
        {
            B = b;
            C =c;
            F = f;
            State = state;
            Covariance = covariance;
        }

        /// <summary>
        /// Фильтрация
        /// </summary>
        /// <param name="measureMatrix"></param>
        /// <param name="j"></param>
        /// <param name="c"></param>
        public void Correct(Matrix measureMatrix, Matrix j, Matrix q)
        {
            //measurement update - correction
            var S = j * Covariance * Matrix.Transpose(j) + q;
            var K = Covariance * Matrix.Transpose(j) * S.Invert();
            X0 = State + K * (measureMatrix - j * State);
            P0 = (Matrix.IdentityMatrix(F.rows, F.cols) - K * j) * Covariance;

            //time update - prediction
            State = F * X0 + B;
            Covariance = F * P0 * Matrix.Transpose(F) + C;
        }
    }
}