using System;

namespace HllDll
{
    public static class Hll
    {
        public unsafe static float standardDeviationHll(float* sample, int numberOfElements)
        {
            float mean = 0f;
            float sum = 0f;


            // Calculate the mean

            for (int i = 0; i < numberOfElements; i++)
            {
                sum += sample[i];
            }

            mean = sum / numberOfElements;

            // Subtract mean from each element and calculate the square of each difference
            // Place the result in appropriate element of sample

            for (int i = 0; i < numberOfElements; i++)
            {
                sample[i] = (sample[i] - mean) * (sample[i] - mean);
            }

            // Add squares

            sum = 0f;

            for (int i = 0; i < numberOfElements; i++)
            {
                sum += sample[i];
            }

            // Divide the sum by n - number of elements

            sum /= numberOfElements;

            // Calculate the square root

            return (float)Math.Sqrt(sum);
        }
    }
}
