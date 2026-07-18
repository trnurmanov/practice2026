using System;

namespace task14
{
    public class DefiniteIntegral
    {
        public static double Solve(double lowerLimit, double upperLimit, Func<double, double> integrand, double precision, int threadNumber)
        {
            if (threadNumber < 1)
                throw new ArgumentException("Количество потоков должно быть положительным", nameof(threadNumber));

            double integralResult = 0.0;
            double chunkSize = (upperLimit - lowerLimit) / threadNumber;
            Thread[] computationThreads = new Thread[threadNumber];
            double[] chunkResults = new double[threadNumber];

            using (var synchronizationBarrier = new Barrier(threadNumber + 1))
            {
                for (int i = 0; i < threadNumber; i++)
                {
                    int currentChunk = i;
                    computationThreads[i] = new Thread(() =>
                    {
                        double chunkStart = lowerLimit + currentChunk * chunkSize;
                        double chunkEnd = (currentChunk == threadNumber - 1) ? upperLimit : chunkStart + chunkSize;

                        double chunkIntegral = IntegrateByTrapezoidalRule(chunkStart, chunkEnd, integrand, precision);

                        chunkResults[currentChunk] = chunkIntegral;
                        synchronizationBarrier.SignalAndWait();
                    });

                    computationThreads[i].Start();
                }

                synchronizationBarrier.SignalAndWait();
            }

            foreach (var chunkValue in chunkResults)
                integralResult += chunkValue;

            return integralResult;
        }

        public static double SolveSingleThread(double lowerLimit, double upperLimit, Func<double, double> integrand, double precision)
        {
            double integrationRange = upperLimit - lowerLimit;

            if (integrationRange <= 0)
                return 0.0;

            int partitionPoints = Math.Max(1, (int)Math.Ceiling(integrationRange / precision));
            double refinedStep = integrationRange / partitionPoints;

            double trapezoidalSum = 0.0;
            for (int j = 0; j < partitionPoints; j++)
            {
                double leftNode = lowerLimit + j * refinedStep;
                double rightNode = leftNode + refinedStep;
                double leftFunctionValue = integrand(leftNode);
                double rightFunctionValue = integrand(rightNode);
                trapezoidalSum += (leftFunctionValue + rightFunctionValue) / 2.0 * refinedStep;
            }

            return trapezoidalSum;
        }

        private static double IntegrateByTrapezoidalRule(double intervalStart, double intervalEnd, Func<double, double> integrand, double gridStep)
        {
            double integrationRange = intervalEnd - intervalStart;

            if (integrationRange <= 0)
                return 0.0;

            int partitionPoints = Math.Max(1, (int)Math.Ceiling(integrationRange / gridStep));
            double refinedStep = integrationRange / partitionPoints;

            double trapezoidalSum = 0.0;
            for (int j = 0; j < partitionPoints; j++)
            {
                double leftNode = intervalStart + j * refinedStep;
                double rightNode = leftNode + refinedStep;
                double leftFunctionValue = integrand(leftNode);
                double rightFunctionValue = integrand(rightNode);
                trapezoidalSum += (leftFunctionValue + rightFunctionValue) / 2.0 * refinedStep;
            }

            return trapezoidalSum;
        }
    }
}
