using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class UniversalPSOReducer
{
    public double ExplainedVarianceRatio2D { get; private set; } // % da variância retida em PC1 + PC2
    public double MeanReconstructionError { get; private set; }  // Erro médio de projeção/reconstrução
    public double MaxReconstructionError { get; private set; }   // Erro máximo de projeção/reconstrução
    public double PCAComputationTimeMs { get; private set; }     // Tempo gasto no AdaptToFunction (ms)

    private readonly int _targetDim;

    public double[,] _principalComponents;
    public double[] _originalMean;
    private double[][] _boundsN;

    public int OriginalDimensions => _originalMean?.Length ?? 0;

    public int TargetDimensions => _targetDim;

    public UniversalPSOReducer(int targetDim = 2)
    {
        if (targetDim <= 0)
            throw new ArgumentException("Target dimension must be positive.");

        _targetDim = targetDim;
    }

    public (double min, double max)[] AdaptToFunction(
        Func<double[], double> objectiveFunction,
        (double min, double max)[] originalBounds,
        int sampleSize = 400,
        double rangeMargin = 0.15)
    {
        Stopwatch swPCA = Stopwatch.StartNew();

        if (objectiveFunction == null)
            throw new ArgumentNullException(nameof(objectiveFunction));

        if (originalBounds == null || originalBounds.Length == 0)
            throw new ArgumentException("Original bounds cannot be empty.");

        if (originalBounds.Length < _targetDim)
            throw new ArgumentException(
                "Original dimensions cannot be smaller than target dimensions.");

        if (sampleSize < 2)
            throw new ArgumentException("Sample size must be at least 2.");

        if (rangeMargin < 0)
            throw new ArgumentException("Range margin cannot be negative.");

        int originalDimCount = originalBounds.Length;

        _boundsN = new double[originalDimCount][];

        for (int d = 0; d < originalDimCount; d++)
        {
            double min = originalBounds[d].min;
            double max = originalBounds[d].max;

            if (double.IsNaN(min) || double.IsInfinity(min) ||
                double.IsNaN(max) || double.IsInfinity(max) ||
                min >= max)
            {
                throw new ArgumentException(
                    $"Invalid range for dimension x{d}.");
            }

            _boundsN[d] = new double[] { min, max };
        }

        System.Random rand = new System.Random(42);

        double[][] samples = new double[sampleSize][];

        // Gera amostras usando os bounds individuais de cada dimensão
        for (int i = 0; i < sampleSize; i++)
        {
            samples[i] = new double[originalDimCount];

            for (int d = 0; d < originalDimCount; d++)
            {
                double min = originalBounds[d].min;
                double max = originalBounds[d].max;

                samples[i][d] =
                    min + rand.NextDouble() * (max - min);
            }

            double fitness = objectiveFunction(samples[i]);

            if (double.IsNaN(fitness) || double.IsInfinity(fitness))
                throw new ArgumentException(
                    "The objective function returned an invalid value.");
        }

        // Calcula a média de cada dimensão
        _originalMean = new double[originalDimCount];

        for (int d = 0; d < originalDimCount; d++)
        {
            _originalMean[d] = samples.Average(s => s[d]);
        }

        // Centraliza a matriz de amostras
        double[,] centeredMatrix =
            new double[sampleSize, originalDimCount];

        for (int i = 0; i < sampleSize; i++)
        {
            for (int d = 0; d < originalDimCount; d++)
            {
                centeredMatrix[i, d] =
                    samples[i][d] - _originalMean[d];
            }
        }

        // Calcula as componentes principais
        _principalComponents = ComputePrincipalComponents(
            centeredMatrix,
            sampleSize,
            originalDimCount,
            _targetDim
        );

        double totalError = 0;
        double maxError = 0;

        for (int i = 0; i < sampleSize; i++)
        {
            double[] reducedPoint = new double[_targetDim];

            for (int t = 0; t < _targetDim; t++)
            {
                for (int d = 0; d < originalDimCount; d++)
                {
                    reducedPoint[t] +=
                        centeredMatrix[i, d] *
                        _principalComponents[t, d];
                }
            }

            double[] reconstructed =
                ConvertToOriginalSpace(reducedPoint);

            double error = 0;

            for (int d = 0; d < originalDimCount; d++)
            {
                double difference =
                    samples[i][d] - reconstructed[d];

                error += difference * difference;
            }

            error = Math.Sqrt(error);

            totalError += error;
            maxError = Math.Max(maxError, error);
        }

        // Salva métricas de erro de reconstrução
        MeanReconstructionError = totalError / sampleSize;
        MaxReconstructionError = maxError;

        // Projeta as amostras no espaço reduzido
        double[][] reducedData = ProjectToReducedSpace(
            centeredMatrix,
            sampleSize,
            originalDimCount
        );

        // Calcula os bounds reduzidos
        var reducedBounds =
            new (double min, double max)[_targetDim];

        for (int t = 0; t < _targetDim; t++)
        {
            double min = reducedData.Min(d => d[t]);
            double max = reducedData.Max(d => d[t]);

            double margin = (max - min) * rangeMargin;

            if (max - min < 1e-10)
            {
                min -= 1.0;
                max += 1.0;
            }
            else
            {
                min -= margin;
                max += margin;
            }

            reducedBounds[t] = (min, max);
        }

        swPCA.Stop();
        PCAComputationTimeMs = swPCA.Elapsed.TotalMilliseconds;

        return reducedBounds;
    }

    // Converte as coordenadas reduzidas de volta a dimensão original
    public double[] ConvertToOriginalSpace(double[] reducedPosition)
    {
        if (_originalMean == null || _principalComponents == null)
            throw new InvalidOperationException(
                "Reducer has not been initialized.");

        if (reducedPosition == null ||
            reducedPosition.Length != _targetDim)
        {
            throw new ArgumentException(
                "Invalid reduced position dimensions.");
        }

        int originalDimCount = _originalMean.Length;

        double[] pointND = new double[originalDimCount];

        for (int d = 0; d < originalDimCount; d++)
        {
            double sum = 0;

            for (int t = 0; t < _targetDim; t++)
            {
                sum += reducedPosition[t] *
                       _principalComponents[t, d];
            }

            pointND[d] = sum + _originalMean[d];
        }

        return pointND;
    }

    // Avalia a função original usando coordenadas reduzidas
    public double ConvertAndEvaluate(
        double[] reducedPsoPosition,
        Func<double[], double> objectiveFunction)
    {
        if (objectiveFunction == null)
            throw new ArgumentNullException(nameof(objectiveFunction));

        double[] pointND = ConvertToOriginalSpace(reducedPsoPosition);

        return objectiveFunction(pointND);
    }

   
    // Calcula as compontentes da ortogonal principal com diagonalização de Jacobi
    private double[,] ComputePrincipalComponents(
        double[,] data,
        int rows,
        int cols,
        int desiredComponents)
    {
        // Calcula a matriz de covariância
        double[,] covariance = new double[cols, cols];

        for (int i = 0; i < cols; i++)
        {
            for (int j = i; j < cols; j++)
            {
                double sum = 0;

                for (int r = 0; r < rows; r++)
                    sum += data[r, i] * data[r, j];

                covariance[i, j] = sum / (rows - 1);
                covariance[j, i] = covariance[i, j];
            }
        }

        // Inicializa a matriz de autovetores
        double[,] eigenvectors = new double[cols, cols];

        for (int i = 0; i < cols; i++)
            eigenvectors[i, i] = 1.0;

        // Diagonalização de Jacobi
        int maxIterations = 100 * cols * cols;
        double tolerance = 1e-12;

        for (int iteration = 0;
             iteration < maxIterations;
             iteration++)
        {
            int p = 0;
            int q = 1;
            double maxValue = 0;

            // Procura o maior elemento fora da diagonal
            for (int i = 0; i < cols; i++)
            {
                for (int j = i + 1; j < cols; j++)
                {
                    double value = Math.Abs(covariance[i, j]);

                    if (value > maxValue)
                    {
                        maxValue = value;
                        p = i;
                        q = j;
                    }
                }
            }

            if (maxValue < tolerance)
                break;

            // Calcula o ângulo da rotação
            double app = covariance[p, p];
            double aqq = covariance[q, q];
            double apq = covariance[p, q];

            double tau = (aqq - app) / (2.0 * apq);

            double t = Math.Sign(tau) /
                (Math.Abs(tau) + Math.Sqrt(1.0 + tau * tau));

            if (tau == 0)
                t = 1.0;

            double c = 1.0 / Math.Sqrt(1.0 + t * t);
            double s = t * c;

            // Atualiza a matriz de covariância
            for (int k = 0; k < cols; k++)
            {
                if (k == p || k == q)
                    continue;

                double akp = covariance[k, p];
                double akq = covariance[k, q];

                covariance[k, p] = c * akp - s * akq;
                covariance[p, k] = covariance[k, p];

                covariance[k, q] = s * akp + c * akq;
                covariance[q, k] = covariance[k, q];
            }

            covariance[p, p] = app - t * apq;
            covariance[q, q] = aqq + t * apq;

            covariance[p, q] = 0;
            covariance[q, p] = 0;

            // Atualiza os autovetores
            for (int k = 0; k < cols; k++)
            {
                double vkp = eigenvectors[k, p];
                double vkq = eigenvectors[k, q];

                eigenvectors[k, p] = c * vkp - s * vkq;
                eigenvectors[k, q] = s * vkp + c * vkq;
            }
        }

        // Ordena os autovalores em ordem decrescente
        int[] indices = Enumerable.Range(0, cols)
            .OrderByDescending(i => covariance[i, i])
            .ToArray();

        double totalVariance = 0.0;
        for (int i = 0; i < cols; i++)
            totalVariance += Math.Max(0.0, covariance[i, i]);

        double retainedVariance = 0.0;
        for (int component = 0; component < desiredComponents; component++)
            retainedVariance += Math.Max(0.0, covariance[indices[component], indices[component]]);

        ExplainedVarianceRatio2D = totalVariance > 0.0
            ? (retainedVariance / totalVariance) * 100.0
            : 100.0;

        double totalEigenvalues = Enumerable.Range(0, cols).Sum(i => covariance[i, i]);
        double top2Eigenvalues = covariance[indices[0], indices[0]] + covariance[indices[1], indices[1]];
        ExplainedVarianceRatio2D = totalEigenvalues > 0 ? (top2Eigenvalues / totalEigenvalues) * 100.0 : 100.0;

        // Extrai os componentes principais
        double[,] components =
            new double[desiredComponents, cols];

        for (int component = 0;
             component < desiredComponents;
             component++)
        {
            int eigenIndex = indices[component];

            for (int d = 0; d < cols; d++)
            {
                components[component, d] =
                    eigenvectors[d, eigenIndex];
            }
        }

        return components;
    }

    private double Normalize(double[] vector)
    {
        double norm = Math.Sqrt(
            vector.Sum(v => v * v));

        if (norm < 1e-12)
            return 0;

        for (int i = 0; i < vector.Length; i++)
            vector[i] /= norm;

        return norm;
    }

    private double[][] ProjectToReducedSpace(
        double[,] centeredMatrix,
        int rows,
        int cols)
    {
        double[][] reduced = new double[rows][];

        for (int i = 0; i < rows; i++)
        {
            reduced[i] = new double[_targetDim];

            for (int t = 0; t < _targetDim; t++)
            {
                double sum = 0;

                for (int d = 0; d < cols; d++)
                {
                    sum += centeredMatrix[i, d] *
                           _principalComponents[t, d];
                }

                reduced[i][t] = sum;
            }
        }

        return reduced;
    }
}
