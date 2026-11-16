
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public static class MathParser
{
    public static double EvaluateExpression(string formula, double[] x)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(formula))
                throw new FormatException("Empty expression.");

            if (x == null)
                throw new ArgumentNullException(nameof(x));

            Parser parser = new Parser(formula, x);
            double result = parser.Parse();

            if (double.IsNaN(result) || double.IsInfinity(result))
                return double.PositiveInfinity;

            return result;
        }
        catch (Exception)
        {
            // Fórmula inválida ou operação matemática indefinida.
            return double.PositiveInfinity;
        }
    }
    public static bool ValidateExpression(
    string formula,
    int variableCount,
    out string error)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(formula))
                throw new FormatException("A fórmula está vazia.");

            if (variableCount < 1)
                throw new ArgumentException("Número de variáveis inválido.");


            var matches = Regex.Matches(
                formula,
                @"(?<![a-zA-Z0-9_])x([0-9]+)(?![a-zA-Z0-9_])",
                RegexOptions.IgnoreCase);

            HashSet<int> uniqueIndexes = new HashSet<int>();

            foreach (Match match in matches)
            {
                if (int.TryParse(match.Groups[1].Value, out int index))
                {
                    uniqueIndexes.Add(index);
                }
            }

            if (uniqueIndexes.Count > 0)
            {
                int highestIndex = uniqueIndexes.Max();

                // Verifica se existem lacunas na sequência.
                for (int i = 0; i <= highestIndex; i++)
                {
                    if (!uniqueIndexes.Contains(i))
                    {
                        throw new FormatException(
                            "Os indices das variáveis devem ser sequenciais começando de x0.");
                    }
                }
            }

            // Verifica se a quantidade de variáveis corresponde à dimensão.
            if (uniqueIndexes.Count != variableCount)
            {
                throw new FormatException(
                    "A quantidade de variáveis não condiz com o total informado.");
            }

            // Valores de teste apenas para percorrer a expressão.
            double[] testVariables = new double[variableCount];

            for (int i = 0; i < variableCount; i++)
                testVariables[i] = 1.5;

            Parser parser = new Parser(formula, testVariables, true);
            parser.Parse();

            error = "";
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private class Parser
    {
        private readonly string formula;
        private readonly double[] variables;
        private readonly bool syntaxOnly;
        private int position;

        public Parser(string formula, double[] variables, bool syntaxOnly = false)
        {
            this.formula = formula;
            this.variables = variables;
            this.syntaxOnly = syntaxOnly;
            position = 0;
        }

        public double Parse()
        {
            double result = ParseExpression();

            SkipWhitespace();

            if (position < formula.Length)
                throw new FormatException("Unexpected character.");

            return result;
        }

        // Soma e subtração
        private double ParseExpression()
        {
            double value = ParseTerm();

            while (true)
            {
                SkipWhitespace();

                if (Match('+'))
                    value += ParseTerm();
                else if (Match('-'))
                    value -= ParseTerm();
                else
                    break;
            }

            return value;
        }

        // Multiplicação e divisão
        private double ParseTerm()
        {
            double value = ParseUnary();

            while (true)
            {
                SkipWhitespace();

                if (Match('*'))
                {
                    value *= ParseUnary();
                }
                else if (Match('/'))
                {
                    double divisor = ParseUnary();

                    if (divisor == 0 && !syntaxOnly)
                        throw new DivideByZeroException();

                    value /= divisor;
                }
                else
                {
                    break;
                }
            }

            return value;
        }

        // Sinais positivos e negativos
        private double ParseUnary()
        {
            SkipWhitespace();

            if (Match('+'))
                return ParseUnary();

            if (Match('-'))
                return -ParseUnary();

            return ParsePower();
        }

        // Potenciação (associativa à direita)
        private double ParsePower()
        {
            double value = ParsePrimary();

            SkipWhitespace();

            if (Match('^'))
            {
                double exponent = ParseUnary();
                value = Math.Pow(value, exponent);
            }

            return value;
        }

        // Números, variáveis, constantes, funções e parênteses
        private double ParsePrimary()
        {
            SkipWhitespace();

            if (Match('('))
            {
                double value = ParseExpression();

                SkipWhitespace();

                if (!Match(')'))
                    throw new FormatException("Missing closing parenthesis.");

                return value;
            }

            if (position >= formula.Length)
                throw new FormatException("Unexpected end of expression.");

            char current = formula[position];

            if (char.IsDigit(current) || current == '.')
                return ParseNumber();

            if (char.IsLetter(current) || current == '_')
                return ParseIdentifier();

            throw new FormatException("Invalid character.");
        }

        // Leitura de números
        private double ParseNumber()
        {
            int start = position;

            bool hasExponent = false;

            while (position < formula.Length)
            {
                char c = formula[position];

                if (char.IsDigit(c) || c == '.')
                {
                    position++;
                }
                else if ((c == 'e' || c == 'E') && !hasExponent)
                {
                    hasExponent = true;
                    position++;

                    if (position < formula.Length &&
                        (formula[position] == '+' || formula[position] == '-'))
                    {
                        position++;
                    }
                }
                else
                {
                    break;
                }
            }

            string number = formula.Substring(start, position - start);

            return double.Parse(
                number,
                CultureInfo.InvariantCulture
            );
        }

        // Leitura de variáveis, constantes e funções
        private double ParseIdentifier()
        {
            int start = position;

            while (position < formula.Length &&
                   (char.IsLetterOrDigit(formula[position]) ||
                    formula[position] == '_'))
            {
                position++;
            }

            string name = formula.Substring(start, position - start)
                                 .ToLowerInvariant();

            SkipWhitespace();

            // Verifica se é uma função matemática
            if (Match('('))
            {
                double arg1 = ParseExpression();
                double arg2 = 0;
                bool hasSecondArgument = false;

                SkipWhitespace();

                if (Match(','))
                {
                    arg2 = ParseExpression();
                    hasSecondArgument = true;
                }

                SkipWhitespace();

                if (!Match(')'))
                    throw new FormatException("Missing function parenthesis.");

                return EvaluateFunction(name, arg1, arg2, hasSecondArgument);
            }

            // Constantes matemáticas
            if (name == "pi")
                return Math.PI;

            if (name == "e")
                return Math.E;

            // Variáveis x0, x1, x2...
            if (name.Length > 1 && name[0] == 'x')
            {
                string indexText = name.Substring(1);

                int index;

                if (int.TryParse(indexText, out index))
                {
                    if (index < 0 || index >= variables.Length)
                        throw new IndexOutOfRangeException();

                    return variables[index];
                }
            }

            throw new FormatException("Unknown variable or function: " + name);
        }

        // Execução das funções matemáticas
        private double EvaluateFunction(
            string name,
            double a,
            double b,
            bool hasSecondArgument)
        {
            switch (name)
            {
                case "sin":
                    return Math.Sin(a);

                case "cos":
                    return Math.Cos(a);

                case "tan":
                    return Math.Tan(a);

                case "asin":
                    return Math.Asin(a);

                case "acos":
                    return Math.Acos(a);

                case "atan":
                    return Math.Atan(a);

                case "sqrt":
                    return Math.Sqrt(a);

                case "abs":
                    return Math.Abs(a);

                case "log":
                    return hasSecondArgument
                        ? Math.Log(a, b)
                        : Math.Log(a);

                case "log10":
                    return Math.Log10(a);

                case "exp":
                    return Math.Exp(a);

                case "pow":
                    if (!hasSecondArgument)
                        throw new FormatException("Pow requires two arguments.");

                    return Math.Pow(a, b);

                case "min":
                    if (!hasSecondArgument)
                        throw new FormatException("Min requires two arguments.");

                    return Math.Min(a, b);

                case "max":
                    if (!hasSecondArgument)
                        throw new FormatException("Max requires two arguments.");

                    return Math.Max(a, b);

                case "ln":
                    return Math.Log(a);
                default:
                    throw new FormatException("Unknown function: " + name);
            }
        }

        // Ignora espaços em branco
        private void SkipWhitespace()
        {
            while (position < formula.Length &&
                   char.IsWhiteSpace(formula[position]))
            {
                position++;
            }
        }

        // Verifica e consome um caractere
        private bool Match(char expected)
        {
            if (position < formula.Length &&
                formula[position] == expected)
            {
                position++;
                return true;
            }

            return false;
        }
    }
}