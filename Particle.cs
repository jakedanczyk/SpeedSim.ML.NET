using System;
using System.Collections.Generic;
using System.Text;

namespace SpeedSimML
{
    abstract class Particle
    {
        static Random rand = new Random(); // do not want seed here, want each particle to have different random order

        public double[] position;
        public double[] velocity;

        public double error;

        public double[] bestPartPosition;
        public double bestPartError;

        public Particle(int dim)
        {
            position = new double[dim];
            velocity = new double[dim];
            bestPartPosition = new double[dim];
        }

    }
}
