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
        /// ������� �����
        /// </summary>
        public Matrix J { get; private set; }

        /// <summary>
        /// ��� ���������
        /// </summary>
        public Matrix Q { get; private set; }

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
        public KalmanFilter(Matrix b, Matrix q, Matrix f, Matrix j, Matrix c)
        {
            B = b;
            Q = q;
            F = f;
            J = j;
            C = c;
        }

        /// <summary>
        /// ������ ������� ���������
        /// </summary>
        /// <param name="state"></param>
        /// <param name="covariance"></param>
        public void SetState(Matrix state, Matrix covariance)
        {
            State = state;
            Covariance = covariance;
        }

        /// <summary>
        /// ����������
        /// </summary>
        /// <param name="measureMatrix"></param>
        public void Correct(Matrix measureMatrix)
        {
            //measurement update - correction
            var S = J * Covariance * Matrix.Transpose(J) + Q;
            var K = Covariance * Matrix.Transpose(J) * S.Invert();
            X0 = State + K * (measureMatrix - J * State);
            P0 = (Matrix.IdentityMatrix(F.rows, F.cols) - K * J) * Covariance;

            //time update - prediction
            State = F * X0 + B;
            Covariance = F * P0 * Matrix.Transpose(F) + C;
        }
    }
}