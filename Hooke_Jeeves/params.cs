namespace Hooke_Jeeves
{
    public class parameters
    {
        public double a, b, e, mapsize, animspeed;
        public double[] h = new double[2];
        public double[] point0 = new double[2];
        public string function = "";
        public int maxiter;


        public string a_string()
        {
            return a.ToString();
        }
        public string b_string()
        {
            return b.ToString();
        }
        public string e_string()
        {
            return e.ToString();
        }
        public string hx_string()
        {
            return h[0].ToString();
        }
        public string hy_string()
        {
            return h[1].ToString();
        }
        public string x0_string()
        {
            return point0[0].ToString();
        }
        public string y0_string()
        {
            return point0[1].ToString();
        }
        public string max_iter_string()
        {
            return maxiter.ToString();
        }
    }
}