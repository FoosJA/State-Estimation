using System;

namespace State_Estimation.Foundation
{
    public class Matrix
    {
        /// <summary>
        /// строка
        /// </summary>
        public int rows;

        /// <summary>
        /// столбец
        /// </summary>
        public int cols;

        /// <summary>
        /// массив
        /// </summary>
        private double[,] mat;

        /// <summary>
        /// Нижняя треугольная матрица 
        /// </summary>
        private Matrix L;

        /// <summary>
        /// Верхняя треугольная матрица
        /// </summary>
        private Matrix U;

        private int[] pi;
        private double detOfP = 1;

        /// <summary>
        /// Конструктор Матриц
        /// </summary>
        /// <param name="iRows">Номер строки</param>
        /// <param name="iCols">Номер столбца</param>
        public Matrix(int iRows, int iCols) // Matrix Class constructor
        {
            rows = iRows;
            cols = iCols;
            mat = new double[rows, cols];
        }

        /// <summary>
        /// Проверка квадратной матрицы
        /// </summary>
        /// <returns>порядок матрицы</returns>
        public Boolean IsSquare()
        {
            return (rows == cols);
        }

        #region не понятно

        /// <summary>
        /// ?
        /// </summary>
        /// <param name="iRow">строка</param>
        /// <param name="iCol">столбец</param>
        /// <returns></returns>
        public double this[int iRow, int iCol]
        {
            get { return mat[iRow, iCol]; }
            set { mat[iRow, iCol] = value; }
        }

        public Matrix GetCol(int k)
        {
            Matrix m = new Matrix(rows, 1);
            for (int i = 0; i < rows; i++) m[i, 0] = mat[i, k];
            return m;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <param name="k"></param>
        public void SetCol(Matrix v, int k)
        {
            for (int i = 0; i < rows; i++) mat[i, k] = v[i, 0];
        }

        #endregion

        /// <summary>
        /// LU-разложение
        /// </summary>
        public void MakeLU()
        {
            if (!IsSquare()) throw new MException("Матрица должна быть квадратичной!");
            L = IdentityMatrix(rows, cols);
            U = Duplicate();

            pi = new int[rows];
            for (int i = 0; i < rows; i++) pi[i] = i;

            double p = 0;
            double pom2;
            int k0 = 0;
            int pom1 = 0;

            for (int k = 0; k < cols - 1; k++)
            {
                p = 0;
                for (int i = k; i < rows; i++) // find the row with the biggest pivot
                {
                    if (Math.Abs(U[i, k]) > p)
                    {
                        p = Math.Abs(U[i, k]);
                        k0 = i;
                    }
                }

                if (p == 0) // samé nuly ve sloupci
                    throw new MException("The matrix is singular!");

                pom1 = pi[k];
                pi[k] = pi[k0];
                pi[k0] = pom1; // switch two rows in permutation matrix

                for (int i = 0; i < k; i++)
                {
                    pom2 = L[k, i];
                    L[k, i] = L[k0, i];
                    L[k0, i] = pom2;
                }

                if (k != k0) detOfP *= -1;

                for (int i = 0; i < cols; i++) // Switch rows in U
                {
                    pom2 = U[k, i];
                    U[k, i] = U[k0, i];
                    U[k0, i] = pom2;
                }

                for (int i = k + 1; i < rows; i++)
                {
                    L[i, k] = U[i, k] / U[k, k];
                    for (int j = k; j < cols; j++)
                        U[i, j] = U[i, j] - L[i, k] * U[k, j];
                }
            }
        }

        /// <summary>
        /// Функция решает Ax = v в соответствии с вектором решения
        /// </summary>
        /// <param name="v">вектор решения</param>
        /// <returns>Найденая матрица</returns>
        public Matrix SolveWith(Matrix v)
        {
            if (rows != cols) throw new MException("The matrix is not square!");
            if (rows != v.rows) throw new MException("Wrong number of results in solution vector!");
            if (L == null) MakeLU();

            Matrix b = new Matrix(rows, 1);
            for (int i = 0; i < rows; i++) b[i, 0] = v[pi[i], 0]; // switch two items in "v" due to permutation matrix

            Matrix z = SubsForth(L, b);
            Matrix x = SubsBack(U, z);

            return x;
        }

        /// <summary>
        /// Функция обратной матрицы
        /// </summary>
        /// <returns>Обратная матрица</returns>
        public Matrix Invert()
        {
            if (L == null) MakeLU();

            Matrix inv = new Matrix(rows, cols);

            for (int i = 0; i < rows; i++)
            {
                Matrix Ei = Matrix.ZeroMatrix(rows, 1);
                Ei[i, 0] = 1;
                Matrix col = SolveWith(Ei);
                inv.SetCol(col, i);
            }

            return inv;
        }

        public static double MaxElement(Matrix mat)
        {
            double max = 0;
            for (int i = 0; i < mat.rows; i++)
            {
                if (Math.Abs(mat[i, 0]) > max)
                {
                    max = Math.Abs(mat[i, 0]);
                }
            }

            return max;
        }

        /// <summary>
        /// Поиск определителя
        /// </summary>
        /// <returns>Определитель</returns>
        public double Det()
        {
            if (L == null) MakeLU();
            double det = detOfP;
            for (int i = 0; i < rows; i++) det *= U[i, i];
            return det;
        }

        /// <summary>
        /// Функция возвращает копию этой матрицы
        /// </summary>
        /// <returns>Копия матрицы</returns>
        public Matrix Duplicate()
        {
            Matrix matrix = new Matrix(rows, cols);
            for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                matrix[i, j] = mat[i, j];
            return matrix;
        }

        /// <summary>
        /// Функция решает Ax = b для A как нижнюю треугольную матрицу
        /// </summary>
        /// <param name="A">Матрица1</param>
        /// <param name="b">Матрица1</param>
        /// <returns>Найденная матрица х</returns>
        public static Matrix SubsForth(Matrix A, Matrix b)
        {
            if (A.L == null) A.MakeLU();
            int n = A.rows;
            Matrix x = new Matrix(n, 1);

            for (int i = 0; i < n; i++)
            {
                x[i, 0] = b[i, 0];
                for (int j = 0; j < i; j++) x[i, 0] -= A[i, j] * x[j, 0];
                x[i, 0] = x[i, 0] / A[i, i];
            }

            return x;
        }

        /// <summary>
        /// Функция решает Ax = b для A в качестве верхней треугольной матрицы
        /// </summary>
        /// <param name="A">Матрица1</param>
        /// <param name="b">Матрица2</param>
        /// <returns>Найденная матрица х</returns>
        public static Matrix SubsBack(Matrix A, Matrix b)
        {
            if (A.L == null) A.MakeLU();
            int n = A.rows;
            Matrix x = new Matrix(n, 1);

            for (int i = n - 1; i > -1; i--)
            {
                x[i, 0] = b[i, 0];
                for (int j = n - 1; j > i; j--) x[i, 0] -= A[i, j] * x[j, 0];
                x[i, 0] = x[i, 0] / A[i, i];
            }

            return x;
        }

        /// <summary>
        /// Создание нулевой матрицы
        /// </summary>
        /// <param name="iRows">строка</param>
        /// <param name="iCols">Столбец</param>
        /// <returns>Нулевая матрица</returns>
        public static Matrix ZeroMatrix(int iRows, int iCols)
        {
            Matrix matrix = new Matrix(iRows, iCols);
            for (int i = 0; i < iRows; i++)
            for (int j = 0; j < iCols; j++)
                matrix[i, j] = 0;
            return matrix;
        }

        /// <summary>
        /// Функция генерирует единичную матрицу
        /// </summary>
        /// <param name="iRows">Строк</param>
        /// <param name="iCols">Столбцов</param>
        /// <returns>Единичная матрица</returns>
        public static Matrix IdentityMatrix(int iRows, int iCols)
        {
            Matrix matrix = ZeroMatrix(iRows, iCols);
            for (int i = 0; i < Math.Min(iRows, iCols); i++)
                matrix[i, i] = 1;
            return matrix;
        }

        /// <summary>
        /// Функция генерирует диагональную матрицу
        /// </summary>
        /// <param name="iRows"></param>
        /// <param name="iCols"></param>
        /// <param name="value">значение по диагонали</param>
        /// <returns></returns>
        public static Matrix DiagonalMatrix(int iRows, int iCols, double value)
        {
            Matrix matrix = ZeroMatrix(iRows, iCols);
            for (int i = 0; i < Math.Min(iRows, iCols); i++)
                matrix[i, i] = value;
            return matrix;
        }

        /// <summary>
        /// Преобразуем матрицу в строку
        /// </summary>
        /// <returns>Строка</returns>
        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++) s += String.Format("{0,5:0.00000}", mat[i, j]) + " ";
                s += "\r\n";
            }

            return s;
        }

        /// <summary>
        /// Транспонирование матрицы, для любой прямоугольной матрицы
        /// </summary>
        /// <param name="m">Исходная матрица</param>
        /// <returns>Транспонированная матрица</returns>
        public static Matrix Transpose(Matrix m)
        {
            Matrix t = new Matrix(m.cols, m.rows);
            for (int i = 0; i < m.rows; i++)
            for (int j = 0; j < m.cols; j++)
                t[j, i] = m[i, j];
            return t;
        }

        #region Умножение матриц по Шнеерсону

        private static void SafeAplusBintoC(Matrix A, int xa, int ya, Matrix B, int xb, int yb, Matrix C, int size)
        {
            for (int i = 0; i < size; i++) // rows
            for (int j = 0; j < size; j++) // cols
            {
                C[i, j] = 0;
                if (xa + j < A.cols && ya + i < A.rows) C[i, j] += A[ya + i, xa + j];
                if (xb + j < B.cols && yb + i < B.rows) C[i, j] += B[yb + i, xb + j];
            }
        }

        private static void SafeAminusBintoC(Matrix A, int xa, int ya, Matrix B, int xb, int yb, Matrix C, int size)
        {
            for (int i = 0; i < size; i++) // rows
            for (int j = 0; j < size; j++) // cols
            {
                C[i, j] = 0;
                if (xa + j < A.cols && ya + i < A.rows) C[i, j] += A[ya + i, xa + j];
                if (xb + j < B.cols && yb + i < B.rows) C[i, j] -= B[yb + i, xb + j];
            }
        }

        private static void SafeACopytoC(Matrix A, int xa, int ya, Matrix C, int size)
        {
            for (int i = 0; i < size; i++) // rows
            for (int j = 0; j < size; j++) // cols
            {
                C[i, j] = 0;
                if (xa + j < A.cols && ya + i < A.rows) C[i, j] += A[ya + i, xa + j];
            }
        }

        private static void AplusBintoC(Matrix A, int xa, int ya, Matrix B, int xb, int yb, Matrix C, int size)
        {
            for (int i = 0; i < size; i++) // rows
            for (int j = 0; j < size; j++)
                C[i, j] = A[ya + i, xa + j] + B[yb + i, xb + j];
        }

        private static void AminusBintoC(Matrix A, int xa, int ya, Matrix B, int xb, int yb, Matrix C, int size)
        {
            for (int i = 0; i < size; i++) // rows
            for (int j = 0; j < size; j++)
                C[i, j] = A[ya + i, xa + j] - B[yb + i, xb + j];
        }

        private static void ACopytoC(Matrix A, int xa, int ya, Matrix C, int size)
        {
            for (int i = 0; i < size; i++) // rows
            for (int j = 0; j < size; j++)
                C[i, j] = A[ya + i, xa + j];
        }

        /// <summary>
        /// Матричное умножение
        /// </summary>
        /// <param name="A">Матрица1</param>
        /// <param name="B">Матрица2</param>
        /// <returns>Результирующая матрица</returns>
        private static Matrix StrassenMultiply(Matrix A, Matrix B)
        {
            if (A.cols != B.rows) throw new MException("Wrong dimension of matrix!");

            Matrix R;

            int msize = Math.Max(Math.Max(A.rows, A.cols), Math.Max(B.rows, B.cols));

            if (msize < 32)
            {
                R = ZeroMatrix(A.rows, B.cols);
                for (int i = 0; i < R.rows; i++)
                for (int j = 0; j < R.cols; j++)
                for (int k = 0; k < A.cols; k++)
                    R[i, j] += A[i, k] * B[k, j];
                return R;
            }

            int size = 1;
            int n = 0;
            while (msize > size)
            {
                size *= 2;
                n++;
            }

            ;
            int h = size / 2;


            Matrix[,] mField = new Matrix[n, 9];

            int z;
            for (int i = 0; i < n - 4; i++) // rows
            {
                z = (int)Math.Pow(2, n - i - 1);
                for (int j = 0; j < 9; j++) mField[i, j] = new Matrix(z, z);
            }

            SafeAplusBintoC(A, 0, 0, A, h, h, mField[0, 0], h);
            SafeAplusBintoC(B, 0, 0, B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 1], 1, mField); // (A11 + A22) * (B11 + B22);

            SafeAplusBintoC(A, 0, h, A, h, h, mField[0, 0], h);
            SafeACopytoC(B, 0, 0, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 2], 1, mField); // (A21 + A22) * B11;

            SafeACopytoC(A, 0, 0, mField[0, 0], h);
            SafeAminusBintoC(B, h, 0, B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 3], 1, mField); //A11 * (B12 - B22);

            SafeACopytoC(A, h, h, mField[0, 0], h);
            SafeAminusBintoC(B, 0, h, B, 0, 0, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 4], 1, mField); //A22 * (B21 - B11);

            SafeAplusBintoC(A, 0, 0, A, h, 0, mField[0, 0], h);
            SafeACopytoC(B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 5], 1, mField); //(A11 + A12) * B22;

            SafeAminusBintoC(A, 0, h, A, 0, 0, mField[0, 0], h);
            SafeAplusBintoC(B, 0, 0, B, h, 0, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 6], 1, mField); //(A21 - A11) * (B11 + B12);

            SafeAminusBintoC(A, h, 0, A, h, h, mField[0, 0], h);
            SafeAplusBintoC(B, 0, h, B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 7], 1, mField); // (A12 - A22) * (B21 + B22);

            R = new Matrix(A.rows, B.cols); // result

            /// C11
            for (int i = 0; i < Math.Min(h, R.rows); i++) // rows
            for (int j = 0; j < Math.Min(h, R.cols); j++) // cols
                R[i, j] = mField[0, 1 + 1][i, j] + mField[0, 1 + 4][i, j] - mField[0, 1 + 5][i, j] +
                          mField[0, 1 + 7][i, j];

            /// C12
            for (int i = 0; i < Math.Min(h, R.rows); i++) // rows
            for (int j = h; j < Math.Min(2 * h, R.cols); j++) // cols
                R[i, j] = mField[0, 1 + 3][i, j - h] + mField[0, 1 + 5][i, j - h];

            /// C21
            for (int i = h; i < Math.Min(2 * h, R.rows); i++) // rows
            for (int j = 0; j < Math.Min(h, R.cols); j++) // cols
                R[i, j] = mField[0, 1 + 2][i - h, j] + mField[0, 1 + 4][i - h, j];

            /// C22
            for (int i = h; i < Math.Min(2 * h, R.rows); i++) // rows
            for (int j = h; j < Math.Min(2 * h, R.cols); j++) // cols
                R[i, j] = mField[0, 1 + 1][i - h, j - h] - mField[0, 1 + 2][i - h, j - h] +
                          mField[0, 1 + 3][i - h, j - h] + mField[0, 1 + 6][i - h, j - h];

            return R;
        }

        // function for square matrix 2^N x 2^N

        private static void
            StrassenMultiplyRun(Matrix A, Matrix B, Matrix C, int l,
                Matrix[,] f) // A * B into C, level of recursion, matrix field
        {
            int size = A.rows;
            int h = size / 2;

            if (size < 32)
            {
                for (int i = 0; i < C.rows; i++)
                for (int j = 0; j < C.cols; j++)
                {
                    C[i, j] = 0;
                    for (int k = 0; k < A.cols; k++) C[i, j] += A[i, k] * B[k, j];
                }

                return;
            }

            AplusBintoC(A, 0, 0, A, h, h, f[l, 0], h);
            AplusBintoC(B, 0, 0, B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 1], l + 1, f); // (A11 + A22) * (B11 + B22);

            AplusBintoC(A, 0, h, A, h, h, f[l, 0], h);
            ACopytoC(B, 0, 0, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 2], l + 1, f); // (A21 + A22) * B11;

            ACopytoC(A, 0, 0, f[l, 0], h);
            AminusBintoC(B, h, 0, B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 3], l + 1, f); //A11 * (B12 - B22);

            ACopytoC(A, h, h, f[l, 0], h);
            AminusBintoC(B, 0, h, B, 0, 0, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 4], l + 1, f); //A22 * (B21 - B11);

            AplusBintoC(A, 0, 0, A, h, 0, f[l, 0], h);
            ACopytoC(B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 5], l + 1, f); //(A11 + A12) * B22;

            AminusBintoC(A, 0, h, A, 0, 0, f[l, 0], h);
            AplusBintoC(B, 0, 0, B, h, 0, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 6], l + 1, f); //(A21 - A11) * (B11 + B12);

            AminusBintoC(A, h, 0, A, h, h, f[l, 0], h);
            AplusBintoC(B, 0, h, B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 7], l + 1, f); // (A12 - A22) * (B21 + B22);

            /// C11
            for (int i = 0; i < h; i++) // rows
            for (int j = 0; j < h; j++) // cols
                C[i, j] = f[l, 1 + 1][i, j] + f[l, 1 + 4][i, j] - f[l, 1 + 5][i, j] + f[l, 1 + 7][i, j];

            /// C12
            for (int i = 0; i < h; i++) // rows
            for (int j = h; j < size; j++) // cols
                C[i, j] = f[l, 1 + 3][i, j - h] + f[l, 1 + 5][i, j - h];

            /// C21
            for (int i = h; i < size; i++) // rows
            for (int j = 0; j < h; j++) // cols
                C[i, j] = f[l, 1 + 2][i - h, j] + f[l, 1 + 4][i - h, j];

            /// C22
            for (int i = h; i < size; i++) // rows
            for (int j = h; j < size; j++) // cols
                C[i, j] = f[l, 1 + 1][i - h, j - h] - f[l, 1 + 2][i - h, j - h] + f[l, 1 + 3][i - h, j - h] +
                          f[l, 1 + 6][i - h, j - h];
        }

        #endregion

        /// <summary>
        /// Простое умножение матриц
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns>Результирующая матрица</returns>
        public static Matrix StupidMultiply(Matrix m1, Matrix m2)
        {
            if (m1.cols != m2.rows) throw new MException("Wrong dimensions of matrix!");

            Matrix result = ZeroMatrix(m1.rows, m2.cols);
            for (int i = 0; i < result.rows; i++)
            for (int j = 0; j < result.cols; j++)
            for (int k = 0; k < m1.cols; k++)
                result[i, j] += m1[i, k] * m2[k, j];
            return result;
        }

        /// <summary>
        /// Умножение матрицы на константу
        /// </summary>
        /// <param name="n">Константа</param>
        /// <param name="m">Матрица</param>
        /// <returns>Результирующая матрица</returns>
        public static Matrix Multiply(double n, Matrix m)
        {
            Matrix r = new Matrix(m.rows, m.cols);
            for (int i = 0; i < m.rows; i++)
            for (int j = 0; j < m.cols; j++)
                r[i, j] = m[i, j] * n;
            return r;
        }

        /// <summary>
        /// Сложение\вычитание матриц
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns></returns>
        private static Matrix Add(Matrix m1, Matrix m2)
        {
            if (m1.rows != m2.rows || m1.cols != m2.cols)
                throw new MException("Матрицы должны иметь одинаковый размер!");
            Matrix r = new Matrix(m1.rows, m1.cols);
            for (int i = 0; i < r.rows; i++)
            for (int j = 0; j < r.cols; j++)
                r[i, j] = m1[i, j] + m2[i, j];
            return r;
        }


        public static Matrix operator -(Matrix m)
        {
            return Matrix.Multiply(-1, m);
        }

        public static Matrix operator +(Matrix m1, Matrix m2)
        {
            return Matrix.Add(m1, m2);
        }

        public static Matrix operator -(Matrix m1, Matrix m2)
        {
            return Matrix.Add(m1, -m2);
        }

        public static Matrix operator *(Matrix m1, Matrix m2)
            //{ return Matrix.StrassenMultiply(m1, m2); }
        {
            return Matrix.StupidMultiply(m1, m2);
        }


        public static Matrix operator *(double n, Matrix m)
        {
            return Matrix.Multiply(n, m);
        }

        public bool Equals(Matrix other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            if (!cols.Equals(other.cols) || !rows.Equals(other.rows))
            {
                return false;
            }

            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    if (!this[j, i].Equals(other[j, i]))
                    {
                        return false;
                    }
                }
                
            }

            return true;
        }
        
    }

    public class MException : Exception
    {
        public MException(string Message)
            : base(Message)
        {
        }
    }
}