
using System;
using UnityEngine;

public class FunctionMesh : MonoBehaviour
{
    [Header("Configurações da Malha")]
    public int resolution = 80;
    public float spacing = 0.25f;

    [Header("Ajustes Gerais (Tamanho Natural)")]
    public float mathScale = 0.2f;
    public float heightScaleY = 0.2f;

    [Header("Cores do Gradiente")]
    public Color baseColor = Color.blue;
    public Color mediumColor = Color.green;
    public Color topColor = Color.red;

    private PSO.FunctionType chosenFunction;

    [Header("Scripts")]
    [SerializeField] private InputManager scriptInputManager;
    [SerializeField] private MenuInterface scriptMenu;

    private bool startedByButton = false;

    [SerializeField] private PSO scriptPSO;

    private UniversalPSOReducer _reducer;
    private Func<double[], double> _currentFunction;


    [Header("Ranges originais do usuário")]
    public float originalMinRangeX;
    public float originalMaxRangeX;
    public float originalMinRangeZ;
    public float originalMaxRangeZ;

    [Header("Ranges reduzidos")]
    public float currentMinRangeX;
    public float currentMaxRangeX;
    public float currentMinRangeZ;
    public float currentMaxRangeZ;

    [Header("Ranges definidos pelo usuário")]
    public float userMinX;
    public float userMaxX;
    public float userMinZ;
    public float userMaxZ;

    public bool hasUserDefinedRanges = false;

    public float LastMeshGenerationTimeMs { get; private set; }
    // Limites independentes dos dois eixos reduzidos
    //public float currentMinRangeX, currentMaxRangeX;
    //public float currentMinRangeZ, currentMaxRangeZ;


    // Inicialização da mesh
    public void StartMeshFunction()
    {
        if (scriptPSO.CalculateFunctionND != null ||
            scriptPSO.usingPremadeFunction == false)
        {
            startedByButton = true;
            generateMesh();
        }
    }


    // Alteração dinâmica da função
    public void ChangeFunctionDinamically(

        PSO.FunctionType newFunction,
        (double min, double max)[] reducedBounds,
        int upC)
    {
        // Debug.Log(
        // $"RANGES RECEBIDOS PELA MESH: " +
        // $"{reducedBounds[0].min} até {reducedBounds[0].max} | " +
        // $"{reducedBounds[1].min} até {reducedBounds[1].max}"
        // );
        chosenFunction = newFunction;

        if (reducedBounds == null || reducedBounds.Length < 2)
        {
            // Debug.LogError("Os limites reduzidos precisam possuir duas dimensões.");
            return;
        }

        currentMinRangeX = (float)reducedBounds[0].min;
        currentMaxRangeX = (float)reducedBounds[0].max;

        currentMinRangeZ = (float)reducedBounds[1].min;
        currentMaxRangeZ = (float)reducedBounds[1].max;

        // Atualiza somente os campos de ranges da interface - NÃO USAR
        //scriptInputManager.UpdateRangeFields(
        //    currentMinRangeX,
        //    currentMaxRangeX,
        //    currentMinRangeZ,
        //    currentMaxRangeZ
        //);

        //scriptInputManager.UpdateRangeFields(
        //    originalMinRangeX,
        //    originalMaxRangeX,
        //    originalMinRangeZ,
        //    originalMaxRangeZ
        //);

        if (upC == 2) // Se for 2 (função customizada, campo de input não vazio, valido)
        {
            generateMesh();
            return;
        }
        if(upC == 3) // Caso não tenha nenhuma função, o upC vai ser 3; Vai desenhar apenas a superfície em 0
        {
            scriptPSO.usingPremadeFunction = true; // Linha
            generateMesh(); // Linha
            scriptPSO.usingPremadeFunction = false; // Linha
            return;
        }
        if (!scriptPSO.usingPremadeFunction || upC == 0) // Se não estiver usando função pré-definida ou não precisar desenhar novamente (0)
        {
            // Debug.Log($"Entrou aqui; {upC}");
            return;
        }
        switch (chosenFunction)
        {
            case PSO.FunctionType.Sphere:
                changeFunctionMeshConfig(80, 2.0f, 0.065f, 0.40f);
                break;

            case PSO.FunctionType.Rastrigin:
                changeFunctionMeshConfig(80, 0.12962f, 1.0f, 0.25f);
                break;

            case PSO.FunctionType.Ackley:
                changeFunctionMeshConfig(80, 0.12658f, 1.0f, 1.0f);
                break;

            case PSO.FunctionType.Rosenbrock:
                changeFunctionMeshConfig(80, 2, 1.0f, 0.05f);
                break;

            case PSO.FunctionType.Griewank:
                changeFunctionMeshConfig(80, 1.01265f, 1.0f, 0.20f);
                break;

            case PSO.FunctionType.Himmelblau:
                changeFunctionMeshConfig(80, 0.12658f, 1.0f, 5f);
                break;

            case PSO.FunctionType.Booth:
                changeFunctionMeshConfig(80, 1, 1.0f, 0.05f);
                break;

            case PSO.FunctionType.Beale:
                changeFunctionMeshConfig(80, 1f, 1.0f, 0.005f);
                break;
        }

        // Debug.Log($"Valor da resolução: {resolution}");

        scriptInputManager.AutoFillFunction(
            resolution,
            spacing,
            mathScale,
            heightScaleY,
            currentMinRangeX,
            currentMaxRangeX,
            currentMinRangeZ,
            currentMaxRangeZ
        );

        // Debug.Log($"Segundo script atualizado para: {chosenFunction}");
    }


    // Botão de preenchimento automático
    public void AutoFillFunctionPress()
    {
        if (scriptPSO.usingPremadeFunction)
        {
            int currentDimensions = 2;

            if (scriptMenu != null &&
                scriptMenu.dimensionsInputField != null &&
                !string.IsNullOrEmpty(scriptMenu.dimensionsInputField.text))
            {
                int.TryParse(
                    scriptMenu.dimensionsInputField.text,
                    out currentDimensions
                );
            }
            else
            {
                currentDimensions = scriptPSO.dimensions;
            }

            if (scriptPSO.Reducer != null &&
                scriptPSO.dimensions == currentDimensions)
            {
                var temporaryBounds = new (double min, double max)[]
                {
                    (currentMinRangeX, currentMaxRangeX),
                    (currentMinRangeZ, currentMaxRangeZ)
                };

                ChangeFunctionDinamically(
                    chosenFunction,
                    temporaryBounds,
                    1
                );

                scriptPSO.StartOptimization(
                    chosenFunction,
                    currentDimensions,
                    1
                );

                // Debug.Log($"Aqui1. bool: {scriptPSO.usingPremadeFunction}");
            }
            else
            {
                scriptPSO.StartOptimization(
                    chosenFunction,
                    currentDimensions,
                    1
                );

                // Debug.Log("Aqui2");
            }
            // Debug.Log("Vai ser desenhada a mesh");
            generateMesh();
        }
        else
        {
            if (!string.IsNullOrEmpty(scriptPSO.customFormulaInputField.text))
            {
                scriptPSO.isFormulaValid = true;
                scriptInputManager.HideFunctionWarningEmpty();
                scriptInputManager.HideFunctionWarningInvalid();

                int currentDimensions = 2;

                if (scriptMenu != null &&
                    scriptMenu.dimensionsInputField != null &&
                    !string.IsNullOrEmpty(scriptMenu.dimensionsInputField.text))
                {
                    int.TryParse(
                        scriptMenu.dimensionsInputField.text,
                        out currentDimensions
                    );
                }
                else
                {
                    currentDimensions = scriptPSO.dimensions;
                    scriptInputManager.ShowDimensionWarningEmpty();
                    return; // Se o campo de dimensão estiver vazio, da return
                }

                // Debug.Log($"Dimensão coletada: {currentDimensions}");

                scriptPSO.StartOptimization(
                    chosenFunction,
                    currentDimensions,
                    1
                );
            }
            else
            {
                scriptPSO.StartOptimization(
                    chosenFunction,
                    2,
                    3
                );
            }

            scriptPSO.positionRangeX2 = 5.12f; // LINHA

            CalculateRecommendedSettings();

            // Debug.Log($"Pulou aqui: {resolution}");

            scriptInputManager.AutoFillFunction(
                resolution,
                spacing,
                mathScale,
                heightScaleY,
                currentMinRangeX,
                currentMaxRangeX,
                currentMinRangeZ,
                currentMaxRangeZ
            );

            generateMesh();
        }
    }


    void Start()
    {
        bool state = (PlayerPrefs.GetInt("Config_PremadeFunction", 1) == 1);

        scriptPSO.usingPremadeFunction = state;

        scriptPSO.usingPremadeFunction = true;

        scriptPSO.canUpdateMeshVisuals = true;
        //hasUserDefinedRanges = false;
        scriptPSO.StartOptimization(
            PSO.FunctionType.Rastrigin,
            2,
            1
        );

        scriptPSO.canUpdateMeshVisuals = false;

        scriptPSO.StartOptimization(
            PSO.FunctionType.Sphere,
            2,
            1
        );
        //hasUserDefinedRanges = true;
        scriptPSO.usingPremadeFunction = state;
        scriptInputManager.HideDimensionWarningEmpty();
    }


    void Update()
    {
        if (startedByButton == true)
        {
            //generateMesh();
        }
    }


    // Converte coordenadas reduzidas em coordenadas visuais
    public Vector3 ReducedToVisualPosition(
        float u,
        float v,
        float fitness)
    {
        float halfSize = ((resolution - 1) * spacing) / 2f;

        float pctX = Mathf.Abs(currentMaxRangeX - currentMinRangeX) > 0.000001f
            ? Mathf.Clamp01(Mathf.InverseLerp(
                currentMinRangeX,
                currentMaxRangeX,
                u))
            : 0.5f;

        float pctZ = Mathf.Abs(currentMaxRangeZ - currentMinRangeZ) > 0.000001f
            ? Mathf.Clamp01(Mathf.InverseLerp(
                currentMinRangeZ,
                currentMaxRangeZ,
                v))
            : 0.5f;

        float xVisual = Mathf.Lerp(-halfSize, halfSize, pctX);
        float zVisual = Mathf.Lerp(-halfSize, halfSize, pctZ);

        float yVisual = fitness * heightScaleY;

        return new Vector3(xVisual, yVisual, zVisual);
    }


    // Geração da mesh
    public void generateMesh()
    {
        if (!startedByButton &&
            scriptPSO != null &&
            !scriptPSO.canUpdateMeshVisuals)
        {
            return;
        }

        if (string.IsNullOrEmpty(scriptPSO.customFormulaInputField.text) &&
            !scriptPSO.usingPremadeFunction)
        {
            // Debug.Log("Fórmula customizada vazia.");
            scriptInputManager.ShowFunctionWarningEmpty();
            return;
        }
        else
        {
            scriptInputManager.HideFunctionWarningEmpty();
        }

        var swMesh = System.Diagnostics.Stopwatch.StartNew();

        // Debug.Log($"Mesh alterada, função {chosenFunction}");

        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();

        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();

        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();

        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

        meshRenderer.material = new Material(Shader.Find("Sprites/Default"));

        Mesh mesh = new Mesh
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };

        int totalVertices = resolution * resolution;

        Vector3[] vertices = new Vector3[totalVertices];
        Color[] colors = new Color[totalVertices];

        int[] triangles = new int[
            (resolution - 1) * (resolution - 1) * 6
        ];

        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        // Calcular os vértices
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float pctX = resolution > 1
                    ? (float)x / (resolution - 1)
                    : 0.5f;

                float pctZ = resolution > 1
                    ? (float)z / (resolution - 1)
                    : 0.5f;

                float u = Mathf.Lerp(
                    currentMinRangeX,
                    currentMaxRangeX,
                    pctX
                );

                float v = Mathf.Lerp(
                    currentMinRangeZ,
                    currentMaxRangeZ,
                    pctZ
                );

                float mathRes = 0f;

                //if (scriptPSO != null &&
                //    scriptPSO.Reducer != null &&
                //    scriptPSO.CalculateFunctionND != null)
                //{
                //    double[] reducedInput = new double[] { u, v };

                //    mathRes = (float)scriptPSO.Reducer.ConvertAndEvaluate(
                //        reducedInput,
                //        scriptPSO.CalculateFunctionND
                //    );
                //}
                if (scriptPSO != null &&
                    scriptPSO.CalculateFunctionND != null)
                {
                    double[] input = { u, v };

                    if (scriptPSO.Reducer != null)
                    {
                        mathRes = (float)scriptPSO.Reducer.ConvertAndEvaluate(
                            input,
                            scriptPSO.CalculateFunctionND
                        );
                    }
                    else
                    {
                        mathRes = (float)scriptPSO.CalculateFunctionND(input);
                    }
                }
                
                Vector3 visualPosition = ReducedToVisualPosition(
                    u,
                    v,
                    mathRes
                );

                int index = z * resolution + x;

                vertices[index] = visualPosition;

                if (visualPosition.y > maxHeight)
                    maxHeight = visualPosition.y;

                if (visualPosition.y < minHeight)
                    minHeight = visualPosition.y;
            }
        }

        // Cores
        float heightRange = (maxHeight - minHeight) < 0.001f
            ? 1f
            : maxHeight - minHeight;

        for (int i = 0; i < totalVertices; i++)
        {
            float normalizedHeight =
                (vertices[i].y - minHeight) / heightRange;

            colors[i] = normalizedHeight < 0.5f
                ? Color.Lerp(
                    baseColor,
                    mediumColor,
                    normalizedHeight * 2f
                )
                : Color.Lerp(
                    mediumColor,
                    topColor,
                    (normalizedHeight - 0.5f) * 2f
                );
        }

        // Triângulos
        int t = 0;

        for (int z = 0; z < resolution - 1; z++)
        {
            for (int x = 0; x < resolution - 1; x++)
            {
                int root = z * resolution + x;

                triangles[t++] = root;
                triangles[t++] = root + resolution;
                triangles[t++] = root + 1;

                triangles[t++] = root + 1;
                triangles[t++] = root + resolution;
                triangles[t++] = root + resolution + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

        swMesh.Stop();
        LastMeshGenerationTimeMs = (float)swMesh.Elapsed.TotalMilliseconds;
    }


    // Configurações da mesh
    private void changeFunctionMeshConfig(
        int res,
        float space,
        float mScale,
        float hScale)
    {
        resolution = res;
        spacing = space;
        mathScale = mScale;
        heightScaleY = hScale;
    }


    // Calcula configurações recomendadas para funções customizadas
    public void CalculateRecommendedSettings()
    {
        float defaultChosenHeight = 4f;

        resolution = 80;
        mathScale = 1f;

        float mathWidth = Mathf.Max(
            Mathf.Abs(scriptPSO.positionRangeX1),
            Mathf.Abs(scriptPSO.positionRangeX2),
            Mathf.Abs(scriptPSO.positionRangeZ1),
            Mathf.Abs(scriptPSO.positionRangeZ2)
        );
        // Talvez usar o de baixo -- LINHA
        //float mathWidth = Mathf.Max(
        //    Mathf.Abs(maxRangeX),
        //    Mathf.Abs(minRangeX),
        //    Mathf.Abs(maxRangeZ),
        //    Mathf.Abs(minRangeZ)
        //);
        spacing = resolution > 1
            ? (mathWidth * 2f) / (resolution - 1)
            : 0.1f;

        float maxFoundMathRes = 0.001f;

       
        float minRangeX = hasUserDefinedRanges
            ? userMinX
            : scriptPSO.positionRangeX1;

        float maxRangeX = hasUserDefinedRanges
            ? userMaxX
            : scriptPSO.positionRangeX2;
        float minRangeZ = hasUserDefinedRanges
            ? userMinZ
            : scriptPSO.positionRangeZ1;

        float maxRangeZ = hasUserDefinedRanges
            ? userMaxZ
            : scriptPSO.positionRangeZ2;

  

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float pctX = resolution > 1
                    ? (float)x / (resolution - 1)
                    : 0.5f;

                float pctZ = resolution > 1
                    ? (float)z / (resolution - 1)
                    : 0.5f;

                float u = Mathf.Lerp(minRangeX, maxRangeX, pctX);
                float v = Mathf.Lerp(minRangeZ, maxRangeZ, pctZ);

                float mathRes = 0f;

  

                if (scriptPSO != null &&
                    scriptPSO.CalculateFunctionND != null)
                {
                    double[] input = { u, v };

                    if (scriptPSO.Reducer != null)
                    {
                        mathRes = (float)scriptPSO.Reducer.ConvertAndEvaluate(
                            input,
                            scriptPSO.CalculateFunctionND
                        );
                    }
                    else
                    {
                        mathRes = (float)scriptPSO.CalculateFunctionND(input);
                    }
                }
                if (Mathf.Abs(mathRes) > maxFoundMathRes)
                {
                    maxFoundMathRes = Mathf.Abs(mathRes);
                }
            }
        }

        if (maxFoundMathRes < 0.001f)
        {
            heightScaleY = 1f;
        }
        else
        {
            heightScaleY = defaultChosenHeight / maxFoundMathRes;
        }
    }

    public bool UpdateOriginalRanges(
    float x1,
    float x2,
    float z1,
    float z2)
    {
        if (x1 >= x2 || z1 >= z2)
        {
            // Debug.LogWarning("Intervalos inválidos.");
            return false;
        }

        // Limites definidos pelo usuário
        userMinX = x1;
        userMaxX = x2;
        userMinZ = z1;
        userMaxZ = z2;

        // Limites originais
        originalMinRangeX = x1;
        originalMaxRangeX = x2;

        originalMinRangeZ = z1;
        originalMaxRangeZ = z2;

        hasUserDefinedRanges = true;

        return true;
    }

    public void TurnoffUserrange()
    {
        hasUserDefinedRanges = false;
    }
    public void ApplyUserDefinedRanges()
    {
        if (!hasUserDefinedRanges)
            return;

        currentMinRangeX = userMinX;
        currentMaxRangeX = userMaxX;

        currentMinRangeZ = userMinZ;
        currentMaxRangeZ = userMaxZ;

        generateMesh();
    }

}
