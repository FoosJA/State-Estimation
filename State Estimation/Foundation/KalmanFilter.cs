namespace State_Estimation.Foundation
{
    /// <summary>
    /// ������ �������
    /// </summary>
    public class KalmanFilter
    {
        /// <summary>
        /// ������������ ������� ��������
        /// </summary>
        public Matrix X0 { get; private set; }

        /// <summary>
        /// ������������ ����������
        /// </summary>
        public Matrix P0 { get; private set; }

        /// <summary>
        /// ������� ��������
        /// </summary>
        public Matrix F { get; private set; }

        /// <summary>
        /// ����������� �����������
        /// </summary>
        public Matrix B { get; private set; }



        /// <summary>
        /// ���������� ���� ��������
        /// </summary>
        public Matrix C { get; private set; }

        /// <summary>
        /// ������ ���������
        /// </summary>
        public Matrix State { get; private set; }

        /// <summary>
        /// ����������
        /// </summary>
        public Matrix Covariance { get; private set; }

        /// <summary>
        /// �����������
        /// </summary>
        /// <param name="b">����������� �����������</param>
        /// <param name="q">��� ���������</param>
        /// <param name="f">������� ��������</param>
        /// <param name="j">������� �����</param>
        /// <param name="c">���������� ���� ��������</param>
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
        /// ����������
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