using TMPro;
using UnityEngine;
using System.Globalization;

public class InputManager : MonoBehaviour
{

    [Header("Fields PSO")]
    //[SerializeField] private string inputText;
    [SerializeField] private TMP_InputField iterationInput;
    [SerializeField] private TMP_InputField populationInput;
    [SerializeField] private TMP_InputField weightPSOInput;
    [SerializeField] private TMP_InputField c1Input;
    [SerializeField] private TMP_InputField c2Input;

    [Header("Fields Functions")]

    [SerializeField] private TMP_InputField x1Input;
    [SerializeField] private TMP_InputField x2Input;
    [SerializeField] private TMP_InputField z1Input;
    [SerializeField] private TMP_InputField z2Input;
    [SerializeField] private TMP_InputField resolutionInput;
    [SerializeField] private TMP_InputField spacingInput;
    [SerializeField] private TMP_InputField mathScaleInput;
    [SerializeField] private TMP_InputField heightScaleYInput;
    [SerializeField] private TMP_InputField speedSettingInput;
    [SerializeField] private TMP_InputField functionFieldInput;
    [SerializeField] private TMP_InputField dimensionFieldInput;
    [Header("Scripts")]

    [SerializeField] private MenuInterface scriptMenuInterface;
    [SerializeField] private PSO scriptPSO;
    [SerializeField] private FunctionMesh scriptFunctionMesh;

    [Header("Warning Text")]
    [SerializeField] private TMP_Text textIteration;
    [SerializeField] private TMP_Text textPopulation;
    [SerializeField] private TMP_Text textWeight;
    [SerializeField] private TMP_Text textCoeficient;
    [SerializeField] private TMP_Text textSearchSpace;
    [SerializeField] private TMP_Text textSearchSpace2;
    [SerializeField] private TMP_Text textResolution;
    [SerializeField] private TMP_Text textSpacing;
    [SerializeField] private TMP_Text textMathScale;
    [SerializeField] private TMP_Text textHeightY;
    [SerializeField] private TMP_Text textFunction;
    [SerializeField] private TMP_Text textFunction2;
    [SerializeField] private TMP_Text textDimension;
    [SerializeField] private TMP_Text textDimension2;
    [SerializeField] private TMP_Text textDimension3;

    public bool dimensionFailed = false;

    public string returnDimension()
    {
        return dimensionFieldInput.text;
    }

    // Preenche os campos de input com os valores do PSO
    public void AutoFillPSO()
    {
        iterationInput.text = 100.ToString();
        populationInput.text = 30.ToString();
        weightPSOInput.text = 0.4.ToString();//0.4f.ToString(); //
        c1Input.text = 1.5f.ToString();
        c2Input.text = 1.5f.ToString();
        //scriptMenuInterface.OnAutoFillPSOPress();
    }

    // Preenche os campos de input com os valores da função
    public void AutoFillFunction(int resolutionA, float spacingA, float mathscaleA, float heightScaleYA, float rangeX1, float rangeX2, float rangeZ1, float rangeZ2)
    {
        resolutionInput.text = resolutionA.ToString();
        spacingInput.text = spacingA.ToString();
        mathScaleInput.text = mathscaleA.ToString(); ;
        heightScaleYInput.text = heightScaleYA.ToString();
        //scriptMenuInterface.OnAutoFillFunctionPress();
        UpdateRangeFields(
            rangeX1,
            rangeX2,
            rangeZ1,
            rangeZ2
        );
        //x1Input.SetTextWithoutNotify(rangeX1.ToString());
        //x2Input.SetTextWithoutNotify(rangeX2.ToString());
        //z1Input.SetTextWithoutNotify(rangeZ1.ToString());
        //z2Input.SetTextWithoutNotify(rangeZ2.ToString());
    }
    
    // Função para mandar as configurações preenchidas para o PSO
    // Também é checado se todos os campos estão corretos; Se não, mostra a mensagem de erro
    public void SubmitSimulationForm()
    {
        string iterationString = iterationInput.text;
        string populationString = populationInput.text;
        string weightString = weightPSOInput.text;
        string c1String = c1Input.text;
        string c2String = c2Input.text;

        string x1String = x1Input.text;
        string x2String = x2Input.text;
        string z1String = z1Input.text;
        string z2String = z2Input.text;

        string resolutionString = resolutionInput.text;
        string spacingString = spacingInput.text;
        string mathScaleString = mathScaleInput.text;
        string heightYString = heightScaleYInput.text;

        string functionString = functionFieldInput.text;
        string dimensionString = dimensionFieldInput.text;

        int count = 0;
        int coeficientCount = 0;
        int searchSpaceCount = 0;

        // Lida com cada entrada. Cada uma é diferente.
        // O valor é passado para o script do PSO. Caso tudo esteja preenchido corretamente, inicia.
        if (int.TryParse(iterationString, out int iteration))
        {
            count++;
            scriptPSO.numberIterations = iteration;
            textIteration.enabled = false;
        }
        else
        {
            // Erro!]
            textIteration.enabled = true;
            count = 0;
        }

        if (int.TryParse(populationString, out int population))
        {
            count++;
            scriptPSO.populationSize = population;
            textPopulation.enabled = false;
        }
        else
        {
            textPopulation.enabled = true;
            count = 0;
            // Debug.LogError($"População deu erro: {population}");
        }

        if (float.TryParse(weightString, out float weight))
        {
            count++;
            scriptPSO.w = weight;
            textWeight.enabled = false;
        }
        else
        {
            textWeight.enabled = true;
            count = 0;
        }

        if (float.TryParse(c1String, out float c1))
        {
            count++;
            coeficientCount++;
            scriptPSO.c1 = c1;
            textCoeficient.enabled = false;
        }
        else
        {
            textCoeficient.enabled = true;
            count = 0;
        }

        if (float.TryParse(c2String, out float c2))
        {
            count++;
            coeficientCount++;
            textCoeficient.enabled = false;
            scriptPSO.c2 = c2;
        }
        else
        {
            textCoeficient.enabled = true;
            count = 0;
        }

        if (coeficientCount == 2)
        {
            textCoeficient.enabled = false;
        }
        else
        {
            textCoeficient.enabled = true;
            coeficientCount = 0;
        }

        if (float.TryParse(x1String, out float x1))
        {
            count++;
            searchSpaceCount++;
        }
        else
        {
            count = 0;
        }

        if (float.TryParse(x2String, out float x2))
        {
            count++;
            searchSpaceCount++;
        }
        else
        {
            count = 0;
        }

        if (float.TryParse(z1String, out float z1))
        {
            count++;
            searchSpaceCount++;
        }
        else
        {
            count = 0;
        }

        if (float.TryParse(z2String, out float z2))
        {
            count++;
            searchSpaceCount++;
        }
        else
        {
            count = 0;
        }

        if (searchSpaceCount == 4)
        {

            if (x1 >= x2 || z1 >= z2)
            {
                textSearchSpace2.enabled = true;
                textSearchSpace.enabled = false;
                count = 0;
            }
            else
            {
                textSearchSpace2.enabled = false;
                textSearchSpace.enabled = false;
            }
        }
        else
        {
            textSearchSpace.enabled = true;
            textSearchSpace2.enabled = false;
            searchSpaceCount = 0;
        }

        if (int.TryParse(resolutionString, out int resol))
        {
            if(resol > 0)
            {
                count++;
                textResolution.enabled = false;
            }
            else
            {
                textResolution.enabled = true;
                count = 0;
            }
        }
        else
        {
            textResolution.enabled = true;
            count = 0;
        }

        if (float.TryParse(spacingString, out float spa))
        {
            if(spa > 0)
            {
                count++;
                textSpacing.enabled = false;
            }
            else
            {
                textSpacing.enabled = true;
                count = 0;
            }
        }
        else
        {
            textSpacing.enabled = true;
            count = 0;
        }

        if (float.TryParse(mathScaleString, out float math))
        {
            if (math > 0)
            {
                count++;
                textMathScale.enabled = false;
            }
            else
            {
                textSpacing.enabled = true;
            }
        }
        else
        {
            textMathScale.enabled = true;
            count = 0;
        }

        if (float.TryParse(heightYString, out float height))
        {
            if(height > 0 )
            {
                count++;
                textHeightY.enabled = false;

            }
            else
            {
                textHeightY.enabled = true;
                count = 0;
            }
        }
        else
        {
            textHeightY.enabled = true;
            count = 0;
        }

        // Se está usando uma função customizada
        if(!scriptPSO.usingPremadeFunction)
        {
            // Se a fórmula é válida (de acordo com a ultima chamada do StartOptimization)
            if (scriptPSO.isFormulaValid)
            {
                count++;
                textFunction2.enabled = false;
            }
            else
            {
                if (!textFunction.enabled)
                { 
                    //textFunction2.enabled = true;
                }
                count = 0;
            }
        }
        else // Caso não esteja usando uma função customizada
        {
            count++;
        }

        if (int.TryParse(dimensionString, out int dimen))
        {
            if(!dimensionFailed)
            {
                count++;
                textDimension.enabled = false;
            }
        }
        else
        {
            // Debug.Log($"Campo de dimensão não deu pra dar parse;");
            textDimension2.enabled = false; // Desliga os outros textos de aviso que podem estarem ligados
            textDimension3.enabled = false;
            textDimension.enabled = true;
            dimensionFailed = false;
            count = 0;
        }

        // Caso todos os campos estejam preenchidos, inicia
        if (count == 15)
        {
            // OK!!
            scriptMenuInterface.OnStartAlgButtonPressed();
            count = 0;
            coeficientCount = 0;
            searchSpaceCount = 0;
        }
        else
        {
            // Debug.Log($"Falhou!");
        }
    }

    // Funções que são chamadas quando o campo de Input é alterado
    // Atualizam os valores dinamicamente, tiram a mensagem de erro
    public void OnEndUse(string typedText)
    {
        string texto = typedText;
        // Debug.Log($"[Imediato] Campo finalizado. Conteúdo: {typedText}");
    }

    public void OnEndIteration(string inputText)
    {
        textIteration.enabled = false;

        if (int.TryParse(inputText, out int text))
        {
            if (text > 1000)
            {
                iterationInput.text = "1000";
            }
            else if (text < -1000)
            {
                iterationInput.text = "-1000";
            }
            else if (text == 0)
            {
                iterationInput.text = "1";
            }
        }
    }

    public void OnEndPopulation(string inputText)
    {
        textPopulation.enabled = false;

        if (int.TryParse(inputText, out int text))
        {
            if (text > 1000)
            {
                populationInput.text = "1000";
            }
            else if (text < -1000)
            {
                populationInput.text = "-1000";
            }
            else if (text == 0)
            {
                populationInput.text = "1";
            }
        }
    }

    public void OnEndWeight(string inputText)
    {
        textWeight.enabled = false;

        if (float.TryParse(inputText, out float text))
        {
            if (text > 1000)
            {
                weightPSOInput.text = "1000";
            }
            else if (text < -1000)
            {
                weightPSOInput.text = "-1000";
            }
        }
        //ValidateDecimalField(weightPSOInput);
    }

    public void OnEndCoeficient1(string inputText)
    {
        textCoeficient.enabled = false;

        if (float.TryParse(inputText, out float c1tex))
        {
            if (c1tex > 1000)
            {
                c1tex = 1000;
                c1Input.text = "1000";
            }
            else if (c1tex < -1000)
            {
                c1tex = -1000;
                c1Input.text = "-1000";
            }
        }
        //ValidateDecimalField(c1Input);
    }

    public void OnEndCoeficient2(string inputText)
    {
        textCoeficient.enabled = false;

        if (float.TryParse(inputText, out float c2tex))
        {
            if (c2tex > 1000)
            {
                c2tex = 1000;
                c2Input.text = "1000";
            }
            else if (c2tex < -1000)
            {
                c2tex = -1000;
                c2Input.text = "-1000";
            }
        }

        //ValidateDecimalField(c2Input);
    }

    public void OnEndFunctionResolution(string inputText)
    {
        textResolution.enabled = false;

        if (int.TryParse(inputText, out int resolution))
        {
            if (resolution > 800)
            {
                resolution = 800;
                resolutionInput.text = "800";
                scriptFunctionMesh.resolution = resolution;
                //Debug.Log($"<color=green>X1 alterada em tempo de execução para: {resolution}</color>");
                scriptFunctionMesh.generateMesh();
            }
            else if (resolution == 0)
            {
                //resolution = 1;
                //resolutionInput.text = "1";
                scriptFunctionMesh.resolution = resolution;
                //scriptFunctionMesh.generateMesh();
            }
            else
            {
                scriptFunctionMesh.resolution = resolution;
                //Debug.Log($"<color=green>X1 alterada em tempo de execução para: {resolution}</color>");
                scriptFunctionMesh.generateMesh();
            }
        }
        else
        {

        }
        //scriptFunctionMesh.generateMesh();
    }

    public void OnEndFunctionSpacing(string inputText)
    {
        ValidateDecimalField(spacingInput);

        textSpacing.enabled = false;

        if (float.TryParse(inputText, out float spacing))
        {
            if (spacing > 100)
            {
                spacing = 100;
                spacingInput.text = "100";
                scriptFunctionMesh.spacing = spacing;
                scriptFunctionMesh.generateMesh();
            }
            else if (spacing == 0)
            {
            //    spacing = 1;
            //    spacingInput.text = "1";
                scriptFunctionMesh.spacing = spacing;
            //    scriptFunctionMesh.generateMesh();
            }
            else
            {
                scriptFunctionMesh.spacing = spacing;
                scriptFunctionMesh.generateMesh();
            }
        }
        else
        {

        }
        //scriptFunctionMesh.generateMesh();
    }

    public void OnEndFunctionMathScale(string inputText)
    {
        ValidateDecimalField(mathScaleInput);

        textMathScale.enabled = false;

        if (float.TryParse(inputText, out float mathScale))
        {
            if (mathScale > 100)
            {
                mathScale = 100;
                mathScaleInput.text = "100";
                scriptFunctionMesh.mathScale = mathScale;
                scriptFunctionMesh.generateMesh();
            }
            else if (mathScale == 0)
            {
                mathScale = 1;
                mathScaleInput.text = "1";
                scriptFunctionMesh.mathScale = mathScale;
                scriptFunctionMesh.generateMesh();
            }
            else
            {
                scriptFunctionMesh.mathScale = mathScale;
                scriptFunctionMesh.generateMesh();
            }
        }
        else
        {

        }
        //scriptFunctionMesh.generateMesh();
    }

    public void OnEndFunctionHeightScaleY(string inputText)
    {

        ValidateDecimalField(heightScaleYInput);

        textHeightY.enabled = false;

        if (float.TryParse(inputText, out float heightScaleY))
        {
            if (heightScaleY > 100)
            {
                heightScaleY = 100;
                heightScaleYInput.text = "100";
                scriptFunctionMesh.heightScaleY = heightScaleY;
                scriptFunctionMesh.generateMesh();
            }
            else if (heightScaleY == 0)
            {
            //    heightScaleY = 1;
            //    heightScaleYInput.text = "1";
                scriptFunctionMesh.heightScaleY = heightScaleY;
            //    scriptFunctionMesh.generateMesh();
            }
            else
            { 
                scriptFunctionMesh.heightScaleY = heightScaleY;
                scriptFunctionMesh.generateMesh();
            }
        }
        else
        {

        }
        //scriptFunctionMesh.generateMesh();
    }


    public void OnEndFunctionX1(string inputText)
    {
        if (!float.TryParse(inputText, out float value))
            return;

        if(value > 1000)
        {
            value = 1000;
            x1Input.text = "1000";
        }
        else if (value < -1000)
        {
            value = -1000;
            x1Input.text = "-1000";
        }

        scriptPSO.positionRangeX1 = Mathf.Clamp(value, -1000f, 1000f);

        int d = ValidateDimension();

        if (UpdateSearchSpace())
            scriptPSO.CallStartOptimization(d, 1);
        scriptFunctionMesh.generateMesh();
    }

    public void OnEndFunctionX2(string inputText)
    {
        UpdateSearchSpace();

        if (!float.TryParse(inputText, out float value))
            return;

        if (value > 1000)
        {
            value = 1000;
            x2Input.text = "1000";
        }
        else if (value < -1000)
        {
            value = -1000;
            x2Input.text = "-1000";
        }
        scriptPSO.positionRangeX1 = Mathf.Clamp(value, -1000f, 1000f);

        int d = ValidateDimension();

        if (UpdateSearchSpace())
            scriptPSO.CallStartOptimization(d, 1);
        scriptFunctionMesh.generateMesh();
    }

    public void OnEndFunctionZ1(string inputText)
    {
        UpdateSearchSpace();

        if (!float.TryParse(inputText, out float value))
            return;

        scriptPSO.positionRangeZ1 = Mathf.Clamp(value, -1000f, 1000f);
        if (value > 1000)
        {
            value = 1000;
            z1Input.text = "1000";
        }
        else if (value < -1000)
        {
            value = -1000;
            z1Input.text = "-1000";
        }
        
        int d = ValidateDimension();

        if (UpdateSearchSpace())
            scriptPSO.CallStartOptimization(d, 1);
    }

    public void OnEndFunctionZ2(string inputText)
    {
        UpdateSearchSpace();

        if (!float.TryParse(inputText, out float value))
            return;
        if (value > 1000)
        {
            value = 1000;
            z2Input.text = "1000";
        }
        else if (value < -1000)
        {
            value = -1000;
            z2Input.text = "-1000";
        }
        scriptPSO.positionRangeZ2 = Mathf.Clamp(value, -1000f, 1000f);

        int d = ValidateDimension();

        if (UpdateSearchSpace())
            scriptPSO.CallStartOptimization(d, 1);
        scriptFunctionMesh.generateMesh();
    }


    //public void OnEndFunctionX1(string inputText)
    //{
    //    //ValidateDecimalField(x1Input);

    //    textSearchSpace2.enabled = false;
    //    textSearchSpace.enabled = false;

    //    ValidateSearchSpace();

    //    if (float.TryParse(inputText, out float x1))
    //    {
    //        if (x1 > 1000)
    //        {
    //            x1 = 1000;
    //            x1Input.text = "1000";
    //            scriptPSO.positionRangeX1 = x1;
    //            scriptFunctionMesh.generateMesh();
    //        }
    //        else if (x1 < -1000)
    //        {
    //            x1 = -1000;
    //            x1Input.text = "-1000";
    //            scriptPSO.positionRangeX1 = x1;
    //            scriptFunctionMesh.generateMesh();
    //        }
    //        else
    //        {
    //            scriptPSO.positionRangeX1 = x1;
    //            scriptFunctionMesh.generateMesh();
    //        }
    //        //Debug.Log($"<color=green>X1 alterada em tempo de execução para: {x1}</color>");
    //    }
    //    else
    //    {

    //    }
    //}

    //public void OnEndFunctionX2(string inputText)
    //{
    //    //ValidateDecimalField(x2Input);
    //    textSearchSpace2.enabled = false;
    //    textSearchSpace.enabled = false;

    //    ValidateSearchSpace();

    //    if (float.TryParse(inputText, out float x2))
    //    {
    //        if (x2 > 1000)
    //        {
    //            x2 = 1000;
    //            x2Input.text = "1000";
    //            scriptPSO.positionRangeX2 = x2;
    //            scriptFunctionMesh.generateMesh();
    //        }
    //        else if (x2 < -1000)
    //        {
    //            x2 = -1000;
    //            x2Input.text = "-1000";
    //            scriptPSO.positionRangeX2 = x2;
    //            scriptFunctionMesh.generateMesh();
    //        }
    //        else
    //        {
    //            scriptPSO.positionRangeX2 = x2;
    //            scriptFunctionMesh.generateMesh();
    //        }
    //    }
    //    else
    //    {

    //    }
    //}

    //public void OnEndFunctionZ1(string inputText)
    //{
    //    //ValidateDecimalField(z1Input);
    //    textSearchSpace2.enabled = false;
    //    textSearchSpace.enabled = false;

    //    ValidateSearchSpace();

    //    if (float.TryParse(inputText, out float z1))
    //    {
    //        if (z1 > 1000)
    //        {
    //            z1 = 1000;
    //            z1Input.text = "1000";
    //            scriptPSO.positionRangeZ1 = z1;
    //            scriptFunctionMesh.generateMesh();
    //        }
    //        else if (z1 < -1000)
    //        {
    //            z1 = -1000;
    //            z1Input.text = "-1000";
    //            scriptPSO.positionRangeZ1 = z1;
    //            scriptFunctionMesh.generateMesh();
    //        }
    //        else
    //        {
    //            scriptPSO.positionRangeZ1 = z1;
    //            scriptFunctionMesh.generateMesh();
    //        }
    //    }
    //    else
    //    {

    //    }
    //}

    //public void OnEndFunctionZ2(string inputText)
    //{
    //    //ValidateDecimalField(z2Input);
    //    textSearchSpace2.enabled = false;
    //    textSearchSpace.enabled = false;

    //    ValidateSearchSpace();

    //    if (float.TryParse(inputText, out float z2))
    //    {
    //        if (z2 > 1000)
    //        {
    //            z2 = 1000;
    //            z2Input.text = "1000";
    //            scriptPSO.positionRangeZ2 = z2;
    //            scriptFunctionMesh.generateMesh();
    //        }
    //        else if (z2 < -1000)
    //        {
    //            z2 = -1000;
    //            z2Input.text = "-1000";
    //            scriptPSO.positionRangeZ2 = z2;
    //            scriptFunctionMesh.generateMesh();
    //        }
    //        else
    //        {
    //            scriptPSO.positionRangeZ2 = z2;
    //            scriptFunctionMesh.generateMesh();
    //        }
    //    }
    //    else
    //    {

    //    }
    //}

    private bool UpdateSearchSpace()
    {
        // Tenta converter os quatro campos
        if (!float.TryParse(x1Input.text, out float x1) ||
            !float.TryParse(x2Input.text, out float x2) ||
            !float.TryParse(z1Input.text, out float z1) ||
            !float.TryParse(z2Input.text, out float z2))
        {
            textSearchSpace.enabled = true;
            textSearchSpace2.enabled = false;
            return false;
        }

        // Verifica se os intervalos são válidos
        bool validX = x1 < x2;
        bool validZ = z1 < z2;

        if (!validX || !validZ)
        {
            textSearchSpace.enabled = false;
            textSearchSpace2.enabled = true;
            return false;
        }

        // Atualiza os limites oficiais do PSO
        scriptPSO.positionRangeX1 = x1;
        scriptPSO.positionRangeX2 = x2;
        scriptPSO.positionRangeZ1 = z1;
        scriptPSO.positionRangeZ2 = z2;

        // Atualiza a representação visual
        scriptFunctionMesh.UpdateOriginalRanges(
            x1,
            x2,
            z1,
            z2
        );
        //scriptFunctionMesh.currentMinRangeX = x1;
        //scriptFunctionMesh.currentMaxRangeX = x2;

        //scriptFunctionMesh.currentMinRangeZ = z1;
        //scriptFunctionMesh.currentMaxRangeZ = z2;

        textSearchSpace.enabled = false;
        textSearchSpace2.enabled = false;

        return true;
    }

    private bool ValidateSearchSpace()
    {
        // Tenta converter os quatro campos
        if (!float.TryParse(x1Input.text, out float x1) ||
            !float.TryParse(x2Input.text, out float x2) ||
            !float.TryParse(z1Input.text, out float z1) ||
            !float.TryParse(z2Input.text, out float z2))
        {
            textSearchSpace.enabled = true;
            textSearchSpace2.enabled = false;
            return false;
        }
        const float minDifference = 0.1f;

        bool validX = x1 < x2;
        bool validZ = z1 < z2;

        if (!validX)
        {
            //x1 = Mathf.Min(x1, x2 - 0.1f);
            x1 = x2 - minDifference;
            x1Input.text = x1.ToString();
        }

        if (!validZ)
        {
            //z1 = Mathf.Min(z1, z2 - 0.1f);
            z1 = z2 - minDifference;
            z1Input.text = z1.ToString();
        }

        scriptPSO.positionRangeX1 = x1;
        scriptPSO.positionRangeX2 = x2;
        scriptPSO.positionRangeZ1 = z1;
        scriptPSO.positionRangeZ2 = z2;

        // Verifica se os intervalos possuem amplitude positiva
        //if (x1 >= x2 || z1 >= z2)
        //{
        //    textSearchSpace2.enabled = true;
        //    textSearchSpace.enabled = false;
        //    return false;
        //}

        bool valid = validX && validZ;

        textSearchSpace2.enabled = !valid;
        textSearchSpace.enabled = false;
        //textSearchSpace2.enabled = false;
        return valid;
    }
    public void OnEndFunction()
    {
        textFunction.enabled = false;
        textFunction2.enabled = false;
        if(!scriptPSO.usingPremadeFunction)
        {
            int dimensionI = ValidateDimension();
            scriptPSO.CallStartOptimization(dimensionI, 1);
            scriptFunctionMesh.generateMesh();
            //scriptMenuInterface.OnAutoFillPSOPress();
        }
    }

    // Para checar o campo de dimensão e desativar os avisos
    public void OnEndDimension() //string inputText)
    {
        //ValidateDecimalField(dimensionFieldInput);
        HideDimensionWarning();
        HideDimensionWarningEmpty();
        HideDimensionWarningLess();
        textDimension.enabled = false;
        textDimension2.enabled = false;
        textDimension3.enabled = false;

        int dimensionI = ValidateDimension();
        // Debug.Log($"If em frente;");
        if (dimensionI > 1)
        {
            // Debug.Log($"If 1;");
            if (scriptPSO.usingPremadeFunction)
            {
                // Debug.Log($"If 2;");
                scriptPSO.CallStartOptimization(dimensionI, 1);
                // Linha Posso adicioanr aqui e embaixo pra ajustar os valores sem ter que apertar no botão automaticamente, mas pode atrapalhar o usuario
                // scriptFunctionMesh.AutoFillFunctionPress();
            }
            else
            {
                // Debug.Log($"Else 1;");
                scriptPSO.CallStartOptimization(dimensionI, 0);
                // scriptFunctionMesh.AutoFillFunctionPress(); // Se usar, Teria que ver se o 0 não vai atrapalhar
            }
            scriptFunctionMesh.generateMesh();
        }
        //if (int.TryParse(inputText, out int dimensionI))
        //{
        //    //scriptPSO.dimensions = dimensionI;
        //    //if(dimensionI)
        //    scriptPSO.CallStartOptimization(dimensionI, 0);
        //}
        //else
        //{

        //}
        //scriptPSO.StartOptimization(,dimensionI,0)
        //scriptFunctionMesh.generateMesh();
    }

    // Para checar se o input de dimensão é valido
    // Caso o valor não seja valido, seta como 2
    // Essa função é chamada por outro script
    public int ValidateDimension()
    {
        string dimensionString = dimensionFieldInput.text;

        // Parse
        if (int.TryParse(dimensionString, out int dimensionI))
        {
            // Se a dimensão for menor que 2 (1 ou 0, já que o input apenas aceita inteiros reais)
            if(dimensionI < 2)
            {
                ShowDimensionWarningLess(); // Aviso
                // Já que o valor do campo de input é alterado, pode permitir a variavel
                dimensionFailed = false; // Linha
                if(dimensionI == 1)
                {
                    dimensionFieldInput.text = "2"; // Seta como o valor padrão de duas dimensões no minimo
                    return 2;
                }
                else
                {
                    dimensionFieldInput.text = "2";
                    return 2;
                }
            }
            else // Caso não tenha erros, retorna o valor real
            {
                return dimensionI;
            }
        }
        else // Caso o parse não tenha dado certo, é provável que o usuário não colocou nada (Nulo)
        {
            ShowDimensionWarningEmpty();
            dimensionFieldInput.text = "2"; // Seta como o valor padrão também
            return 2;
        }
    }

    // Para o botão de usar função pré disponível
    public void turnPremadeFunction()
    {
        if (scriptPSO.usingPremadeFunction)
        {
            scriptPSO.usingPremadeFunction = false;
        }
        else
        {
            scriptPSO.usingPremadeFunction = true;
            scriptFunctionMesh.AutoFillFunctionPress();
        }
    }

    public void turnPremadeFunctionOn()
    {
        scriptPSO.isFormulaValid = true;
        HideFunctionWarningEmpty();
        HideFunctionWarningInvalid();
        
        scriptPSO.usingPremadeFunction = true;
        scriptFunctionMesh.AutoFillFunctionPress();
        //scriptFunctionMesh.generateMesh();
        scriptFunctionMesh.TurnoffUserrange();
    }

    public void turnPremadeFunctionOff()
    {
        scriptPSO.isFormulaValid = false;
        scriptPSO.usingPremadeFunction = false;
        //scriptMenuInterface.OnFunctionTypeDropdownChanged(scriptMenuInterface.dropdownFunctionType.value); // Linha
        int previewDimensions = ValidateDimension();
        if(previewDimensions > 1)
        {
        scriptFunctionMesh.AutoFillFunctionPress();
        }


        //PSO.FunctionType newFunction = (PSO.FunctionType)1; // Altera
        //scriptPSO.StartOptimization(newFunction, 2, 1);
        //scriptFunctionMesh.AutoFillFunctionPress(); // Desenha a função vazia
        //scriptFunctionMesh.generateMesh();  // DESCOMENTAR
    }

    [SerializeField] private MovementData speedSettings;

    // Quando a configuração de velocidade é alterada, atualiza ela
    public void OnEndSpeedSetting(string inputText)
    {
        ValidateDecimalField(speedSettingInput);

        if (float.TryParse(inputText, out float speed))
        {
            if(speed != 0)
            {
                speedSettings.movementTime = speed;
            }
            else
            {
                speedSettings.movementTime = 1;
                speedSettingInput.text = "1";
            }
            // Debug.Log($"Speed settings atualizou para {speed}");
        }
        else
        {
            // Debug.Log($"Speed settings não atualizou");
        }
    }

    // Checa se o valor decimal é válido (Permite números reais apenas)
    // Recebe um campo como parâmetro
    public void ValidateDecimalField(TMP_InputField fieldToValidate)
    {
        if (string.IsNullOrEmpty(fieldToValidate.text)) return;

        string typedText = fieldToValidate.text;

        if (float.TryParse(typedText, NumberStyles.Any, CultureInfo.InvariantCulture, out float result))
        {
            if (result < 0)
            {
                FixFieldText(fieldToValidate);
            }
        }
        // Caso o usuário tenha colocado alguma outra coisa de alguma forma
        else
        {
            fieldToValidate.text = "";
        }
    }

    // Remove o sinal de "-" quando inserido em campos que não permitem
    private void FixFieldText(TMP_InputField field)
    {
        if (field.text.Contains("-"))
        {
            field.text = field.text.Replace("-", "");
        }
    }

    // Atualiza na inicialização do programa
    void Start()
    {
        speedSettingInput.text = speedSettings.movementTime.ToString();

        foreach (ToggleSwitch toggle in FindObjectsByType<ToggleSwitch>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            toggle.InitializeSetting();
        }
    }

    public void ShowFunctionWarningEmpty()
    {
        textFunction.enabled = true;
    }

    public void HideFunctionWarningEmpty()
    {
        textFunction.enabled = false;
    }

    public void ShowFunctionWarningInvalid()
    {
        textFunction2.enabled = true;
    }

    public void HideFunctionWarningInvalid()
    {
        textFunction2.enabled = false;
    }

    public void ShowDimensionWarningEmpty()
    {
        textDimension.enabled = true;
        dimensionFailed = true;
    }

    public void HideDimensionWarningEmpty()
    {
        textDimension.enabled = false;
        dimensionFailed = false;
    }

    public void ShowDimensionWarning()
    {
        textDimension2.enabled = true;
        dimensionFailed = true;
    }

    public void HideDimensionWarning()
    {
        textDimension2.enabled = false;
        dimensionFailed = false;
    }

    public void ShowDimensionWarningLess()
    {
        textDimension3.enabled = true;
        dimensionFailed = true;
    }

    public void HideDimensionWarningLess()
    {
        textDimension3.enabled = false;
        dimensionFailed = false;
    }

    public void UpdateRangeFields(
    float x1,
    float x2,
    float z1,
    float z2)
    {
        // Não sobrescreve nenhum campo enquanto o usuário digita.
        if (x1Input.isFocused ||
            x2Input.isFocused ||
            z1Input.isFocused ||
            z2Input.isFocused)
        {
            return;
        }

        x1Input.SetTextWithoutNotify(x1.ToString());
        x2Input.SetTextWithoutNotify(x2.ToString());
        z1Input.SetTextWithoutNotify(z1.ToString());
        z2Input.SetTextWithoutNotify(z2.ToString());
    }

    private bool IsEditingRangeField()
    {
        return x1Input.isFocused ||
               x2Input.isFocused ||
               z1Input.isFocused ||
               z2Input.isFocused;
    }
}
