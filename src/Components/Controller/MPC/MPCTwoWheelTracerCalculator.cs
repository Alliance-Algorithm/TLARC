using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;


namespace Tlarc.Controller.MPC;

class MPCCalculator(int nx, int nu, int mPCWindow)
{
    private int _nx = nx;
    private int _nu = nu;
    private int _mpcWindow = mPCWindow;
    private DenseMatrix _A = new DenseMatrix(nx + nu, nx + nu);
    private DenseMatrix _B = new DenseMatrix(nx + nu, nu);
    private DenseMatrix _phi = new DenseMatrix(mPCWindow * nx, nu + nx);
    private DenseMatrix _theta = new DenseMatrix(mPCWindow * nx, mPCWindow * nu);
    private DenseVector _delta_u_pre = new DenseVector(nu);
    int[] ct = new int[2 * (mPCWindow * nu + mPCWindow * nu + 1)];
    private readonly DenseMatrix _identity_nu = DenseMatrix.CreateIdentity(nu);
    private readonly DenseMatrix _identity_nx = DenseMatrix.CreateIdentity(nx);
    private readonly DenseMatrix _C = (DenseMatrix)DenseMatrix.CreateIdentity(5).SubMatrix(0, 3, 0, 5);
    public void Init()
    {
        for (int i = 0; i < ct.Length; i++)
            ct[i] = -1;
    }

    public void Reset()
    {
        _delta_u_pre = new DenseVector(_nu);
    }
    public DenseVector Calculate(DenseMatrix a, DenseMatrix b, DenseVector x, DenseVector xRef, DenseMatrix uMin, DenseMatrix uMax, DenseMatrix deltaUMin, DenseMatrix deltaUMax)
    {
        _A.SetSubMatrix(0, _nx, 0, _nx, a);
        _A.SetSubMatrix(0, _nx, _nx, _nu, b);
        _A.SetSubMatrix(_nx, _nu, _nx, _nu, _identity_nu);
        _B.SetSubMatrix(0, _nx, 0, _nu, b);
        _B.SetSubMatrix(_nx, _nu, 0, _nu, _identity_nu);

        var tmp = _C;
        DenseMatrix _tmp_c = new DenseMatrix(_nx, _mpcWindow * _nu);
        for (var i = 1; i < _mpcWindow + 1; i++)
        {
            _phi.SetSubMatrix((i - 1) * _nx, _nx, 0, _nx + _nu, tmp * _A);

            _tmp_c.SetSubMatrix(0, _nx, 0, _nu, tmp * _B);
            if (i > 1)
                _tmp_c.SetSubMatrix(0, _nx, _nu, _nu * (_mpcWindow - 1), _theta.SubMatrix(_nx * (i - 2), _nx, 0, _nu * (_mpcWindow - 1)));

            _theta.SetSubMatrix(_nx * (i - 1), _nx, 0, _nu * _mpcWindow, _tmp_c);

            tmp *= _A;
        }

        var Q = DenseMatrix.CreateIdentity(_nx * _mpcWindow);
        var R = 5.0f * DenseMatrix.CreateIdentity(_nu * _mpcWindow);
        var rho = 10.0f;

        var H = new DenseMatrix(_nu * _mpcWindow + 1, _nu * _mpcWindow + 1);
        H.SetSubMatrix(0, _nu * _mpcWindow, 0, _nu * _mpcWindow, _theta.Transpose() * Q * _theta + R);
        H[_nu * _mpcWindow, _nu * _mpcWindow] = rho;

        var kesi = new DenseMatrix(_nu + _nx, 1);
        var diff_x = x - xRef;
        kesi.SetSubMatrix(0, _nx, 0, 1, diff_x.ToColumnMatrix());
        kesi.SetSubMatrix(_nx, _nu, 0, 1, _delta_u_pre.ToColumnMatrix());

        var g = new DenseMatrix(1, _nu * _mpcWindow + 1);
        g.SetSubMatrix(0, 1, 0, _nu * _mpcWindow, 2 * ((_phi * kesi).Transpose() * Q) * _theta);

        var A_t = new DenseMatrix(_mpcWindow, _mpcWindow);

        for (int i = 0; i < _mpcWindow; i++)
            for (int j = i; j >= 0; j--)
                A_t[i, j] = 1;

        var A_I = A_t.KroneckerProduct(_identity_nu);

        var A_cons = new DenseMatrix(_mpcWindow * _nu + _mpcWindow * _nu + 1, _mpcWindow * _nu + 1);
        A_cons.SetSubMatrix(0, _mpcWindow * _nu, 0, _mpcWindow * _nu, A_I);
        var U_t = DenseMatrix.Create(_mpcWindow, 1, 1).KroneckerProduct(_delta_u_pre.ToColumnMatrix());
        var UMin = DenseMatrix.Create(_mpcWindow, 1, 1).KroneckerProduct(uMin.Transpose());
        var UMax = DenseMatrix.Create(_mpcWindow, 1, 1).KroneckerProduct(uMax.Transpose());
        var LB = new DenseMatrix(1, _mpcWindow * _nu + _mpcWindow * _nu + 1);
        var UB = new DenseMatrix(1, _mpcWindow * _nu + _mpcWindow * _nu + 1);
        LB.SetSubMatrix(0, 1, 0, _mpcWindow * _nu, (UMin - U_t).Transpose());
        UB.SetSubMatrix(0, 1, 0, _mpcWindow * _nu, (UMax - U_t).Transpose());


        var delta_UMin = new DenseMatrix(_mpcWindow * _nu + 1, 1);
        var delta_UMax = new DenseMatrix(_mpcWindow * _nu + 1, 1);
        delta_UMin.SetSubMatrix(0, _nu * _mpcWindow, 0, 1, DenseMatrix.Create(_mpcWindow, 1, 1).KroneckerProduct(deltaUMin.Transpose()));
        delta_UMax.SetSubMatrix(0, _nu * _mpcWindow, 0, 1, DenseMatrix.Create(_mpcWindow, 1, 1).KroneckerProduct(deltaUMax.Transpose()));

        delta_UMin[_mpcWindow * _nu, 0] = 0;
        delta_UMax[_mpcWindow * _nu, 0] = 10;

        var A_1_cons = DenseMatrix.CreateIdentity(_mpcWindow * _nu + 1);
        A_cons.SetSubMatrix(_mpcWindow * _nu, _mpcWindow * _nu + 1, 0, _mpcWindow * _nu + 1, A_1_cons);
        LB.SetSubMatrix(0, 1, _mpcWindow * _nu, _mpcWindow * _nu + 1, delta_UMin.Transpose());
        UB.SetSubMatrix(0, 1, _mpcWindow * _nu, _mpcWindow * _nu + 1, delta_UMax.Transpose());

        alglib.minqpcreate(_mpcWindow * _nu + 1, out var state);
        alglib.minqpsetquadraticterm(state, H.ToArray(), true);
        alglib.minqpsetlinearterm(state, g.ToRowArrays()[0]);
        alglib.minqpsetlc(state, Matrix<double>.Build.DenseOfMatrixArray(new Matrix<double>[,] { { -A_cons, -LB.Transpose() }, { A_cons, UB.Transpose() } }).ToArray(), ct, 2 * (_mpcWindow * _nu + _mpcWindow * _nu + 1));
        alglib.minqpsetscaleautodiag(state);
        alglib.minqpsetalgosparseipm(state, 0.0);
        alglib.minqpoptimize(state);
        alglib.minqpresults(state, out var delta_delta_u, out var rep);
        _delta_u_pre[0] += delta_delta_u[0];
        _delta_u_pre[1] += delta_delta_u[1];
        return delta_delta_u;
    }
}