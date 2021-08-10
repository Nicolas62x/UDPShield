using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPShield
{
    class MovingAvg
    {
        UInt64 Last = 0;

        double[] data = new double[40];
        byte idx = 0;
        byte count = 0;

        public void Push(UInt64 value, double dt)
        {
            UInt64 delta = value - Last;

            data[idx++] = delta / dt;

            if (count < data.Length)
                count++;

            if (idx >= data.Length)
                idx = 0;

            Last = value;
        }

        public double GetAvg()
        {
            double avg = 0;

            for (int i = 0; i < data.Length; i++)
                avg += data[i] / count;

            return avg;
        }
    }
}
