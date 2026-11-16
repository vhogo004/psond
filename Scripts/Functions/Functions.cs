using UnityEngine;
using System;

public static class Functions
{
    
    public delegate double TestFunctionND(double[] x);

    // Função de Esfera
    public static double SphereFunction(double[] x)
    {
        double sum = 0;
        for (int i = 0; i < x.Length; i++) sum += x[i] * x[i];
        return sum;
    }

    // Função de Rastrigin
    public static double RastriginFunction(double[] x)
    {
        double A = 10.0;
        double sum = A * x.Length;
        for (int i = 0; i < x.Length; i++) sum += (x[i] * x[i]) - A * Math.Cos(2.0 * Math.PI * x[i]);
        return sum;
    }

    // Função de Ackley
    public static double AckleyFunction(double[] x)
    {
        double sum1 = 0;
        double sum2 = 0;
        double n = x.Length;

        for (int i = 0; i < x.Length; i++)
        {
            sum1 += x[i] * x[i];
            sum2 += Math.Cos(2.0 * Math.PI * x[i]);
        }

        double term1 = -20.0 * Math.Exp(-0.2 * Math.Sqrt(sum1 / n));
        double term2 = -Math.Exp(sum2 / n);

        return term1 + term2 + 20.0 + Math.E;
    }

    // Função de Rosenbrock
    public static double RosenbrockFunction(double[] x)
    {
        double sum = 0;
        for (int i = 0; i < x.Length - 1; i++)
        {
            sum += 100.0 * Math.Pow(x[i + 1] - x[i] * x[i], 2) + Math.Pow(1.0 - x[i], 2);
        }
        return sum;
    }

    // Função de Griewank
    public static double GriewankFunction(double[] x)
    {
        double sum = 0;
        double prod = 1.0;

        for (int i = 0; i < x.Length; i++)
        {
            sum += (x[i] * x[i]) / 4000.0;
            prod *= Math.Cos(x[i] / Math.Sqrt(i + 1));
        }

        return sum - prod + 1.0;
    }

    // Função de Himmelblau
    public static double HimmelblauFunction(double[] x)
    {
        
        double xPos = x[0];
        double yPos = x.Length > 1 ? x[1] : 0.0;

        double term1 = Math.Pow(xPos * xPos + yPos - 11.0, 2);
        double term2 = Math.Pow(xPos + yPos * yPos - 7.0, 2);

        return (term1 + term2) * 0.01;
    }

    // Função de Booth
    public static double BoothFunction(double[] x)
    {
        double xPos = x[0];
        double yPos = x.Length > 1 ? x[1] : 0.0;

        double term1 = Math.Pow(xPos + 2.0 * yPos - 7.0, 2);
        double term2 = Math.Pow(2.0 * xPos + yPos - 5.0, 2);

        return (term1 + term2) * 0.05; 
    }

    // Função de Beale
    public static double BealeFunction(double[] x)
    {
        double xPos = x[0];
        double yPos = x.Length > 1 ? x[1] : 0.0;

        
        xPos = Math.Max(-4.5, Math.Min(4.5, xPos));
        yPos = Math.Max(-4.5, Math.Min(4.5, yPos));

        double t1 = Math.Pow(1.5 - xPos + xPos * yPos, 2);
        double t2 = Math.Pow(2.25 - xPos + xPos * yPos * yPos, 2);
        double t3 = Math.Pow(2.625 - xPos + xPos * Math.Pow(yPos, 3), 2);

        return (t1 + t2 + t3) * 0.1; 
    }
}
