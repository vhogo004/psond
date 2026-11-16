using System;
using System.Linq;
using System.Xml.Linq;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

using System.Diagnostics;
using UnityEngine.Profiling;

// Script para o PSO em si
public class PSO : MonoBehaviour
{
    // Pega o Script da malha
    public FunctionMesh scriptMesh;

    // Pega o Script do menu
    public MenuInterface scriptMenu;

    // Pega o script da câmera
    public CameraController scriptCamera;

    public InputManager scriptInput;
    public CameraController cameraController;

    private int focusedIndex = -1;
    // InputField para uma função customizada
    [SerializeField] public TMPro.TMP_InputField customFormulaInputField;

    // parâmetros para configurar o PSO
    public int dimensions, numberIterations, populationSize, currentIteration;

    private Vector3 gBestPosition;
    private float gBestFitness = float.MaxValue;

    // public float randomPositionRange = 10; // Range para movimentação das partículas
    public float positionRangeX1 = 10;
    public float positionRangeX2 = 10;
    public float positionRangeZ1 = 10;
    public float positionRangeZ2 = 10;
    private bool userDefinedReducedBounds = false;
    public GameObject particlePrefab; // Prefab da partícula
    public Transform particleContainer; // Contâiner para organizar as particulas; Serve como pai

    public float w, c1, c2; // W Inércia; C1 Componente Cognitivo; C2 Componente Social;

    // Vetor de partículas
    private Particle[] swarm;

    private Stopwatch cronometro = new Stopwatch();
    private List<float> deltaTimes = new List<float>();
    private long picoMemoriaBytes = 0;
    // Tempo de movimento (velocidade) em segundos
    [SerializeField] private MovementData speedSettings;

    // Variáveis para checagem
    public bool startedAlg = false;
    public int check = 0;
    private bool startedByButton = false;

    public bool pause = false;

    public bool useAnimation = true; // Animar

    public bool isFormulaValid = false; // Para ver se a última formula inserida na função de customizar é válida, sem ter que chamar a função

    [HideInInspector] public bool canUpdateMeshVisuals = false;
    private SelectableObj currentSelectedObject;

    public bool selectionExists => currentSelectedObject != null;

    // Para as funções de teste. Cada uma precisa ser colocada aqui, colocada no dropdown e usada no switch em ordem
    public enum FunctionType { Sphere, Rastrigin, Ackley, Rosenbrock, Griewank, Himmelblau, Booth, Beale }

    [Header("Configurações Dinâmicas")]
    [SerializeField] private FunctionType chosenFunction;

    // Redutor de dimensionalidade
    public UniversalPSOReducer Reducer { get; private set; }
    public System.Func<double[], double> CalculateFunctionND { get; private set; }

    //public bool usingCustomFunction = false;
    public bool usingPremadeFunction = true;

    // Para a função selecionada    
    private Functions.TestFunctionND calculateFunction;

    //private Func<double[], double> _currentFunction;

    [Header("Configuração Experimental")]
    public bool experimentalMode = false;
    public int randomSeed = 12345;
    public int experimentRun = 1;

    private List<float> gBestHistory = new List<float>();
    public int executionCount = 0;

    [Header("Botões da Automação")]
    public Button finalWindowScreenButton;
    public Button parameterScreenButton;
    void Awake()
    {
        //AtualizarFuncaoAtiva();
        // UpdateActiveFunction();
    }

    // Executa quando algo é alterado no inspetor (play mode tbm)
    private void OnValidate()
    {
        // Garante que só atualiza em tempo de execução para avoid erros no editor
        if (Application.isPlaying)
        {
            //UpdateActiveFunction();
            // Debug.Log($"<color=green>Função alterada em tempo de execução para: {calculateFunction}</color>");
        }

    }

    // Essa função é chamada quando o botão de start é apertado
    public void StartPSO()
    {
        
        if (CalculateFunctionND != null || usingPremadeFunction == false) // || usingCustomFunction == true)
        {
            startedByButton = true;
            //UpdateActiveFunction(); // Atualiza a função selecionada
            InitializeParticleSwarm(); // Inicializa o enxame
        }
    }

    public void CallStartOptimization(int d, int u)
    {
        // Debug.Log($"Aqui; CallStartOptimization {d}");
        StartOptimization(chosenFunction, d, u);
    }

    public void StartOptimization(FunctionType newFunction, int targetDimensions, int updateCheck)
    {
        chosenFunction = newFunction;
        dimensions = targetDimensions;
        if (usingPremadeFunction)
        {
            
            CalculateFunctionND = chosenFunction switch
            {
                FunctionType.Sphere => Functions.SphereFunction,
                FunctionType.Rastrigin => Functions.RastriginFunction,
                FunctionType.Ackley => Functions.AckleyFunction,
                FunctionType.Rosenbrock => Functions.RosenbrockFunction,
                FunctionType.Griewank => Functions.GriewankFunction,
                FunctionType.Himmelblau => Functions.HimmelblauFunction,
                FunctionType.Booth => Functions.BoothFunction,
                FunctionType.Beale => Functions.BealeFunction,
                _ => Functions.SphereFunction
            };
        }
        else
        {
            // NOVA LÓGICA: Se for função customizada, captura o texto da UI
            string userFormula = customFormulaInputField != null ? customFormulaInputField.text : "x0*x0 + x1*x1";

            string validationError;
			// Debug.Log($"Feita a validação da expressão!");
			if (!MathParser.ValidateExpression(
                userFormula, targetDimensions, out validationError))
            {
                // Debug.LogWarning(
                // "Invalid formula: " + validationError);
                if (validationError == "Index was outside the bounds of the array.")
                {
                    // Debug.Log($"Estava fora!!!");
                    scriptInput.ShowDimensionWarning(); // Aviso de dimensão
                }
                //else if(validationError == "Os indices das variáveis devem ser sequenciais começando de x0!")
                //{
                //    isFormulaValid = false;
                //    scriptInput.ShowFunctionWarningInvalid();
                //}
                else if (validationError == "Os indices das variáveis devem ser sequenciais começando de x0.")
                {
                    isFormulaValid = false;
                    scriptInput.ShowFunctionWarningInvalid();
                }
                else if (validationError == "A quantidade de variáveis não condiz com o total informado.")
                {
                    isFormulaValid = false;
                    //scriptInput.ShowFunctionWarningInvalid();
                    scriptInput.ShowDimensionWarning();
                    scriptInput.HideFunctionWarningInvalid();
                }
                else
                {
					isFormulaValid = false;
					//Debug.Log($"Não estava fora!"); // do array
				}
                
                if (updateCheck == 0)
                {
                    // Debug.Log($"Deu return!");
                    return;
                }
                updateCheck = 3;
            }
            else
            {
                updateCheck = 2;
                // Debug.Log($"Não deu erro na validação");
                scriptInput.HideDimensionWarningEmpty();
				scriptInput.HideDimensionWarning();
				scriptInput.HideDimensionWarningLess();// Remove o aviso
				isFormulaValid = true; // Marca a fórmula como válida
            }
            // Passa o Interpretador dinâmico como se fosse uma função do sistema!
            // O Redutor Universal aceita isso perfeitamente sem saber a diferença.
            CalculateFunctionND = (x) => MathParser.EvaluateExpression(userFormula, x);
            if(updateCheck == 3)
            {
                targetDimensions = 2;
                CalculateFunctionND = (x) => MathParser.EvaluateExpression("x0+x1 - (x0 + x1)", x); // Linha
				// Debug.Log($"Entrou aqui, atribui outra função");
               
				//CalculateFunctionND = Functions.SphereFunction; // Força a fun~ção de esfera
			}
        }

       var (min, max) = GetDefaultBounds(chosenFunction);
  //      if (!usingPremadeFunction) { min = -5.0; max = 5.0; } // Define um limite padrão para a função do usuário
    
  //      var originalBounds = new (double min, double max)[targetDimensions];
  //      for (int i = 0; i < targetDimensions; i++) originalBounds[i] = (min, max);

		//Debug.Log($"Aqui; Dimensão atual: {dimensions}");
		//Debug.Log($"Aqui; targetDimensão atual: {targetDimensions}");


        if (!usingPremadeFunction)
        {
            min = -5.0;
            max = 5.0;
        }

        var originalBounds = new (double min, double max)[targetDimensions];

        // Primeiro, aplica os limites padrão.
        for (int i = 0; i < targetDimensions; i++)
        {
            originalBounds[i] = (min, max);
        }

        // Depois, substitui pelos limites definidos pelo usuário.
        if (scriptMesh != null && scriptMesh.hasUserDefinedRanges)
        {
            if (targetDimensions < 2)
            {
                // Debug.LogError(
                // "São necessárias pelo menos duas dimensões para utilizar os ranges personalizados."
                // );
                return;
            }

            originalBounds[0] = (
                scriptMesh.userMinX,
                scriptMesh.userMaxX
            );

            originalBounds[1] = (
                scriptMesh.userMinZ,
                scriptMesh.userMaxZ
            );

            //Debug.Log(
            //    $"Ranges personalizados aplicados: " +
            //    $"X0 [{originalBounds[0].min}, {originalBounds[0].max}], " +
            //    $"X1 [{originalBounds[1].min}, {originalBounds[1].max}]"
            //);
        }

        var reducedBounds = new (double min, double max)[2];

        if (targetDimensions == 2)
        {
            // Não utiliza PCA
            Reducer = null;

            // Mantém os limites originais
            reducedBounds[0] = originalBounds[0];
            reducedBounds[1] = originalBounds[1];

            // Debug.Log("Função 2D: PCA desabilitado.");
        }
        else
        {
            Reducer = new UniversalPSOReducer(targetDim: 2);
            reducedBounds = Reducer.AdaptToFunction(
                CalculateFunctionND,
                originalBounds,
                sampleSize: 400,
                rangeMargin: 0.15
            );
            // Debug.Log("Função multidimensional: PCA habilitado.");
        }

        
        if (scriptMesh != null)
        {
            if(updateCheck == 1)
            {
                scriptMesh.ChangeFunctionDinamically(chosenFunction, reducedBounds, updateCheck);
            }
            
            if(updateCheck == 3)
            {
                scriptMesh.ChangeFunctionDinamically(chosenFunction, reducedBounds, updateCheck);
            }else
            {
                // Debug.Log($"Updatecheck provavelmente 0: {updateCheck}");
                scriptMesh.ChangeFunctionDinamically(chosenFunction, reducedBounds, updateCheck);
            }
            
            //scriptMesh.ChangeFunctionDinamically(chosenFunction, reducedBounds);
        }
    }

    private (double min, double max) GetDefaultBounds(FunctionType type)
    {
        
        return type switch
        {
            FunctionType.Sphere => (-5.12, 5.12),
            FunctionType.Rastrigin => (-5.12, 5.12),
            FunctionType.Ackley => (-5.0, 5.0),
            FunctionType.Rosenbrock => (-2.048, 2.048),
            FunctionType.Griewank => (-600.0, 600.0),
            FunctionType.Himmelblau => (-5.0, 5.0),
            FunctionType.Booth => (-10.0, 10.0),
            FunctionType.Beale => (-4.5, 4.5),
            _ => (-5.0, 5.0)
        };
    }

    
    // Funções para pausar, tocar, termianr e finalizar a execução do algoritmo
    public void Pause()
    {
        pause = true;
        scriptMenu.UpdatePauseText();
    }
    public void Play()
    {
        pause = false;
        scriptMenu.UpdatePlayText();
    }
    // Para quando o botão de ir pro menu é apertado
    public void EndSimulation()
    {
        startedAlg = false;
        scriptMesh.hasUserDefinedRanges = false;
        ClearCurrentSelection();
        CleanSwarm();
    }

    // Para quando o algoritmo é finalizado pelo botão de finalizar ou termina por si só
    public void FinalizeSimulation()
    {
        double[] reducedPosition = { gBestPosition.x, gBestPosition.z };
        double[] originalPosition = (Reducer != null)
            ? Reducer.ConvertToOriginalSpace(reducedPosition)
            : new double[] { reducedPosition[0], reducedPosition[1] };

        double reconstructedFitness = CalculateFunctionND(originalPosition);

        // 2. Coleta as métricas do PCA (se Dimensions > 2) e da Malha
        double evr2D = Reducer != null ? Reducer.ExplainedVarianceRatio2D : 100.0;
        double meanReconErr = Reducer != null ? Reducer.MeanReconstructionError : 0.0;
        double maxReconErr = Reducer != null ? Reducer.MaxReconstructionError : 0.0;
        double pcaTimeMs = Reducer != null ? Reducer.PCAComputationTimeMs : 0.0;
        float meshTimeMs = scriptMesh != null ? scriptMesh.LastMeshGenerationTimeMs : 0f;

        if (experimentalMode)
        {
            cronometro.Stop();
            double tempoTotalMs = cronometro.Elapsed.TotalMilliseconds;
            double picoMemoriaMB = picoMemoriaBytes / (1024.0 * 1024.0);

            float avgFPS = 0f;
            float onePercentLowFPS = 0f;

            if (deltaTimes.Count > 0)
            {
                // FPS Médio = 1 / média(deltaTime)
                float somaDt = 0f;
                foreach (float dt in deltaTimes) somaDt += dt;
                avgFPS = deltaTimes.Count / somaDt;

                // 1% Low FPS = 1 / percentil 99 dos piores (maiores) deltaTimes
                deltaTimes.Sort();
                int indiceP99 = Mathf.Clamp((int)(deltaTimes.Count * 0.99f), 0, deltaTimes.Count - 1);
                onePercentLowFPS = 1f / deltaTimes[indiceP99];
            }

            PSOExperimentLogger.SaveRun(
                chosenFunction.ToString(),
                experimentRun,
                randomSeed,
                dimensions,
                populationSize,
                numberIterations,
                w,
                c1,
                c2,
                gBestFitness,
                gBestHistory,
                (float)tempoTotalMs,
                avgFPS,
                onePercentLowFPS,
                (float)picoMemoriaMB,
                reconstructedFitness,
                evr2D,
                meanReconErr,
                maxReconErr,
                pcaTimeMs,
                meshTimeMs
            );
            randomSeed += 1;
        }
        scriptMesh.hasUserDefinedRanges = false;
        

        scriptCamera.ChangeCameraMode(3);
        startedAlg = false;
        scriptMenu.OpenFinalWindow();
        scriptMenu.BuildFinalWindow(gBestPosition, gBestFitness, originalPosition, reconstructedFitness);
        ClearCurrentSelection();
        CleanSwarm();
        scriptMenu.uiVisible = true;

        experimentRun = experimentRun + 1; // Aumenta o run em 1 também
        executionCount = executionCount + 1; // Incrementa em 1
        if(executionCount == 30)
        {
            experimentRun = 1; // Reseta Run
            executionCount = 0; // Execution count reseta também
            randomSeed = 12345; // Seed também
            //
        }
        else
        // Para o modo experimental; Vai apertar o botão de voltar a tela de parâmetros, e após, vai apertar o botão de iniciar a simulação!
        if(experimentalMode)
        {
            StartCoroutine(ClickSequence());
        }
        // Deve mostrar a tela final da simulação
    }

    // Atualiza a função selecionada
    private IEnumerator ClickSequence()
    {
        if (finalWindowScreenButton != null && finalWindowScreenButton.interactable)
        {
            finalWindowScreenButton.onClick.Invoke();
        }
        yield return null;

        if (parameterScreenButton != null && parameterScreenButton.gameObject.activeInHierarchy && finalWindowScreenButton.interactable)
        {
            parameterScreenButton.onClick.Invoke();
        }
        else
        {
            UnityEngine.Debug.LogWarning("O segundo botão não ficou ativo a tempo!");
        }
    }

    void Start()
    {
        executionCount = 0; // Reseta o execution
        //InitializeParticleSwarm();
    }

    void Update()
    {
        // Se o enxame foi inicializado
        if (!startedAlg || !startedByButton) return;
        //CheckReadyParticles();

        // Checa pra ver se já chegamos ao número máximo de iterações
        if (currentIteration >= numberIterations)
        {
            //startedAlg = false;
            FinalizeSimulation();
            return;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ClickDetection();
        }

        if (Keyboard.current != null &&
            Keyboard.current.fKey.wasPressedThisFrame &&
            cameraController.currentMode == CameraController.CameraMode.Free)
        {
            if (cameraController.IsFocusMode())
            {
                cameraController.DisableFocus();
                focusedIndex = -1;
            }
            else
            {
                int index = currentSelectedObject != null
                    ? currentSelectedObject.indexInArray
                    : 0;

                FocusObjectAtIndex(index);
            }
        }

        if (cameraController.IsFocusMode() && Keyboard.current != null)
        {
            if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                FocusNextObject();
            }

            if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                FocusPreviousObject();
            }
        }

        // Movimenta a particula
        MoveParticle();
        scriptMenu.UpdatePlayUI(currentIteration, gBestPosition, gBestFitness);

        if (currentSelectedObject != null)
        {
            scriptMenu.UpdatePlayUI2(swarm[currentSelectedObject.indexInArray].bestPosition,
            swarm[currentSelectedObject.indexInArray].bestFitness,
            swarm[currentSelectedObject.indexInArray].position,
            swarm[currentSelectedObject.indexInArray].starterStep,
            swarm[currentSelectedObject.indexInArray].nextPos);
        }
        // Se todas as partículas já estão prontas, vai pra próxima iteração
        if (AllParticlesReady())
        {
            NextIteration();
            // Função para atualizar as informações da UI
        }
    }


    [System.Serializable]
    public class Particle
    {
        public GameObject instance;
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 bestPosition;
        public float bestFitness;
        //public float fitness;
        public bool readyForNextIteration;
        public Vector3 starterStep;
        public Vector3 nextPos;

        public float timeElapsed;
    }

    // Função para inicializar um enxame de partículas
    void InitializeParticleSwarm()
    {
        gBestHistory.Clear();
        if (experimentalMode)
        {
            UnityEngine.Random.InitState(randomSeed);
            deltaTimes.Clear();
            picoMemoriaBytes = 0;
            cronometro.Restart();
        }

        currentIteration = 0;
        check = 0;
        // Cria uma nova população
        swarm = new Particle[populationSize];
        gBestFitness = float.MaxValue;
        gBestPosition = Vector3.zero;

        //float halfSize = ((scriptMesh.resolution - 1) * scriptMesh.spacing) / 2f;
        //float halfMeshSize = ((scriptMesh.resolution - 1) * scriptMesh.spacing) / 2f;


        //positionRangeX1 = scriptMesh.currentMinRangeX;
        //positionRangeX2 = scriptMesh.currentMaxRangeX;
        //positionRangeZ1 = scriptMesh.currentMinRangeZ;
        //positionRangeZ2 = scriptMesh.currentMaxRangeZ;

        if (!userDefinedReducedBounds)
        {
            positionRangeX1 = scriptMesh.currentMinRangeX;
            positionRangeX2 = scriptMesh.currentMaxRangeX;
            positionRangeZ1 = scriptMesh.currentMinRangeZ;
            positionRangeZ2 = scriptMesh.currentMaxRangeZ;
        }

        for (int i = 0; i < populationSize; i++)
        {
            // Posição aleatória na geração
            Vector3 randomPosition = GetRandomPos(positionRangeX1, positionRangeX2, positionRangeZ1, positionRangeZ2);

 

            randomPosition.y = EvaluateFitness(randomPosition);

            Vector3 visualPosition = scriptMesh.ReducedToVisualPosition(
                randomPosition.x,
                randomPosition.z,
                randomPosition.y
            );

            GameObject newObject = Instantiate(
                particlePrefab,
                visualPosition,
                Quaternion.identity,
                particleContainer
            );
            //GameObject newObject = Instantiate(particlePrefab, randomPosition, Quaternion.identity, particleContainer);
            newObject.GetComponent<SelectableObj>().indexInArray = i;
            //Vector3 randomVelocity = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f));
            Vector3 randomVelocity = new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                0f,
                UnityEngine.Random.Range(-1f, 1f)
            );
            swarm[i] = new Particle
            {
                instance = newObject,
                position = randomPosition,
                velocity = randomVelocity,
                bestPosition = randomPosition,
                bestFitness = randomPosition.y, // O fitness inicial é o próprio Y
                readyForNextIteration = false,  // Começa FALSO para elas se moverem para o primeiro destino
                starterStep = randomPosition,
                timeElapsed = 0f
            };

            // Atualiza o melhor global inicial
            if (swarm[i].bestFitness < gBestFitness)
            {
                gBestFitness = swarm[i].bestFitness;
                gBestPosition = swarm[i].bestPosition;
            }
        }

        gBestHistory.Add(gBestFitness);

        for (int i = 0; i < populationSize; i++)
        {
            float r1 = UnityEngine.Random.value;
            float r2 = UnityEngine.Random.value;

            swarm[i].velocity = CalculateVelocity(swarm[i], r1, r2);

            swarm[i].starterStep = swarm[i].position;

            swarm[i].nextPos = swarm[i].position + swarm[i].velocity;

            swarm[i].nextPos = ClampParticlePosition(swarm[i].nextPos);

            swarm[i].velocity = ReflectVelocity(
                swarm[i].nextPos,
                swarm[i].velocity
            );
        }

        startedAlg = true;
        pause = false;
    }

    private Vector3 CalculateVelocity(Particle particle, float r1, float r2)
    {
        Vector3 cognitive = c1 * r1 *
            (particle.bestPosition - particle.position);

        Vector3 social = c2 * r2 *
            (gBestPosition - particle.position);

        Vector3 velocity = (w * particle.velocity) + cognitive + social;

        // O movimento acontece somente em X e Z
        velocity.y = 0f;

        return velocity;
    }
    // Checa se todas as partículas estão prontas para a próxima iteração. É pra evitar erros de processamento
    bool AllParticlesReady()
    {
        for (int i = 0; i < populationSize; i++)
        {
            if (!swarm[i].readyForNextIteration) return false;
        }
        return true;
    }

    // Função para a próxima iteração

    void NextIteration()
    {
        currentIteration++;

        // Atualiza o fitness e os melhores valores
        for (int i = 0; i < populationSize; i++)
        {
            float fitness = EvaluateFitness(swarm[i].position);

            if (float.IsNaN(fitness) || float.IsInfinity(fitness))
                continue;

            swarm[i].position.y = fitness;

            // Melhor posição individual
            if (fitness < swarm[i].bestFitness)
            {
                swarm[i].bestFitness = fitness;
                swarm[i].bestPosition = swarm[i].position;
            }

            // Melhor posição global
            if (fitness < gBestFitness)
            {
                gBestFitness = fitness;
                gBestPosition = swarm[i].bestPosition;
            }
        }

        gBestHistory.Add(gBestFitness);

        if (currentIteration >= numberIterations)
            return;

        // Atualiza velocidade e destino das partículas
        // Atualiza velocidade e destino das partículas

        for (int i = 0; i < populationSize; i++)
        {
            float r1 = UnityEngine.Random.value;
            float r2 = UnityEngine.Random.value;

            swarm[i].velocity = CalculateVelocity(swarm[i], r1, r2);

            swarm[i].starterStep = swarm[i].position;

            swarm[i].nextPos = swarm[i].position + swarm[i].velocity;

            swarm[i].nextPos = ClampParticlePosition(swarm[i].nextPos);

            swarm[i].velocity = ReflectVelocity(
                swarm[i].nextPos,
                swarm[i].velocity
            );

            swarm[i].timeElapsed = 0f;
            swarm[i].readyForNextIteration = false;
        }

    }


    // Função para gerar uma posição aleatória e retornar em formato de Vector3
    // Usa os valores passados como limitadores pro range;
    Vector3 GetRandomPos(float minValueX, float maxValueX, float minValueZ, float maxValueZ)
    {
        float posX = UnityEngine.Random.Range(minValueX, maxValueX);
        //float posY = Random.Range(minValue, maxValue);
        float posZ = UnityEngine.Random.Range(minValueZ, maxValueZ);

        Vector3 randomVector3 = new Vector3(posX, 0, posZ);
        return randomVector3;

    }

    // Função para fazer a movimentação da partícula
    // Anima ela se movendo pelo espaço em vez de apenas teleportar-se
    // No entanto, ela não vai guardar as informações das coordenadas que passou por cima para chegar ao local
    // (Seu fitness não é atualizado pelo caminho, apenas na origem e destino)


    void MoveParticle()
    {
        if (!useAnimation)
        {
            if (Time.unscaledDeltaTime > 0f)
            {
                deltaTimes.Add(Time.unscaledDeltaTime);
            }

            // Monitora o pico de memória alocada pelo processo/Unity
            long memAtual = Profiler.GetTotalAllocatedMemoryLong();
            if (memAtual > picoMemoriaBytes)
            {
                picoMemoriaBytes = memAtual;
            }
            MoveParticlesInstantly();
            return;
        }

        if (speedSettings == null ||
            speedSettings.movementTime <= 0f ||
            pause)
            return;

        for (int i = 0; i < populationSize; i++)
        {
            if (swarm[i].readyForNextIteration)
                continue;

            if (swarm[i].timeElapsed < speedSettings.movementTime)
            {
                swarm[i].timeElapsed += Time.deltaTime;

                float percentageTime = Mathf.Clamp01(
                    swarm[i].timeElapsed / speedSettings.movementTime
                );

                // Interpola no espaço reduzido do PCA
                Vector3 currentPosition = Vector3.Lerp(
                    swarm[i].starterStep,
                    swarm[i].nextPos,
                    percentageTime
                );

                // Calcula o fitness na posição atual
                currentPosition.y = EvaluateFitness(currentPosition);

                // Atualiza a posição matemática
                swarm[i].position = currentPosition;

                // Converte para o espaço visual da mesh
                swarm[i].instance.transform.position =
                    scriptMesh.ReducedToVisualPosition(
                        currentPosition.x,
                        currentPosition.z,
                        currentPosition.y
                    );
            }
            else
            {
                check += 1;

                // Garante a posição matemática exata do destino
                swarm[i].position = swarm[i].nextPos;

                // Calcula o fitness no destino
                swarm[i].position.y = EvaluateFitness(
                    swarm[i].position
                );

                // Atualiza a posição visual utilizando a mesma conversão
                swarm[i].instance.transform.position =
                    scriptMesh.ReducedToVisualPosition(
                        swarm[i].position.x,
                        swarm[i].position.z,
                        swarm[i].position.y
                    );

                swarm[i].readyForNextIteration = true;
            }
        }
    }

    private void MoveParticlesInstantly()
    {
        if (pause)
            return;

        for (int i = 0; i < populationSize; i++)
        {
            if (swarm[i].readyForNextIteration)
                continue;

            swarm[i].position = swarm[i].nextPos;

            swarm[i].position.y =
                EvaluateFitness(swarm[i].position);

            swarm[i].instance.transform.position =
                scriptMesh.ReducedToVisualPosition(
                    swarm[i].position.x,
                    swarm[i].position.z,
                    swarm[i].position.y
                );

            swarm[i].readyForNextIteration = true;
        }
    }

    public void turnAnimationOn()
    {
        useAnimation = true;
    }
    public void turnAnimationOff()
    {
        useAnimation = false;
    }

    // Limpa o vetor de partículas
    public void CleanSwarm()
    {
        for (int i = 0; i < swarm.Length; i++)
        {
            if (swarm[i].instance != null)
            {
                Destroy(swarm[i].instance);
            }
            swarm[i] = default(Particle);
        }
    }
    void ClickDetection()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        // Raio a partir da camera
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        // Se acertar algo
        if (Physics.Raycast(ray, out hit))
        {
            SelectableObj clicked = hit.collider.GetComponent<SelectableObj>();
            if (clicked != null)
            {
                SetNewSelected(clicked);
                return;
            }
        }
        ClearCurrentSelection();
    }

    void SetNewSelected(SelectableObj newObject)
    {
        ClearCurrentSelection();
        currentSelectedObject = newObject;
        currentSelectedObject.DefineSelection(true);
    }

    public void ClearCurrentSelection()
    {
        if (currentSelectedObject != null)
        {
            currentSelectedObject.DefineSelection(false);
            currentSelectedObject = null;
        }

        if (cameraController != null && cameraController.IsFocusMode())
        {
            cameraController.DisableFocus();
            focusedIndex = -1;
        }
    }
    void FocusObjectAtIndex(int index)
    {
        if (swarm == null || swarm.Length == 0) return;

        if (index < 0 || index >= swarm.Length) return;

        if (swarm[index] == null || swarm[index].instance == null) return;

        focusedIndex = index;

        if (currentSelectedObject != null)
        {
            currentSelectedObject.DefineSelection(false);
        }

        currentSelectedObject =
        swarm[index].instance.GetComponent<SelectableObj>();

        if (currentSelectedObject != null)
        {
            currentSelectedObject.DefineSelection(true);
        }

        cameraController.FocusOn(
            swarm[index].instance.transform
        );
    }

    public void FocusNextObject()
    {
        if (!cameraController.IsFocusMode()) return;
        if (swarm == null || swarm.Length == 0) return;

        focusedIndex++;

        if (focusedIndex >= swarm.Length)
            focusedIndex = 0;

        FocusObjectAtIndex(focusedIndex);
    }
    public void FocusPreviousObject()
    {
        if (!cameraController.IsFocusMode()) return;
        if (swarm == null || swarm.Length == 0) return;

        focusedIndex--;

        if (focusedIndex < 0)
            focusedIndex = swarm.Length - 1;

        FocusObjectAtIndex(focusedIndex);
    }

    private float EvaluateFitness(Vector3 position)
    {
        if (CalculateFunctionND == null)
        {
            // Debug.LogError("PSO: Objective function is not initialized.");
            return float.NaN;
        }

        double[] originalInput;

        if (Reducer != null)
        {
            double[] reducedInput =
            {
                position.x,
                position.z
             };

            originalInput = Reducer.ConvertToOriginalSpace(
                reducedInput
            );
        }
        else
        {
            originalInput = new double[]
            {
                position.x,
                position.z
            };
        }
        //    double[] reducedInput =
        //    {
        //    position.x,
        //    position.z
        //};


        //double result = Reducer.ConvertAndEvaluate(
        //    reducedInput,
        //    CalculateFunctionND
        //);

        double result = CalculateFunctionND(originalInput);
        if (double.IsNaN(result) || double.IsInfinity(result))
        {
            // Debug.LogWarning("PSO: Invalid fitness value.");
            return float.NaN;
        }

        return (float)result;
    }

    private Vector3 ClampParticlePosition(Vector3 position)
    {
        position.x = Mathf.Clamp(
            position.x,
            positionRangeX1,
            positionRangeX2
        );

        position.z = Mathf.Clamp(
            position.z,
            positionRangeZ1,
            positionRangeZ2
        );

        return position;
    }

    private Vector3 ReflectVelocity(Vector3 position, Vector3 velocity)
    {
        if (position.x <= positionRangeX1 ||
            position.x >= positionRangeX2)
        {
            velocity.x *= -1f;
        }

        if (position.z <= positionRangeZ1 ||
            position.z >= positionRangeZ2)
        {
            velocity.z *= -1f;
        }

        return velocity;
    }

    //public void SetReducedBounds(
    //float x1,
    //float x2,
    //float z1,
    //float z2)
    //{
    //    positionRangeX1 = x1;
    //    positionRangeX2 = x2;
    //    positionRangeZ1 = z1;
    //    positionRangeZ2 = z2;
    //}

    public void SetReducedBounds(
    float x1,
    float x2,
    float z1,
    float z2)
    {
        if (x1 >= x2 || z1 >= z2)
        {
            // Debug.LogError("Invalid reduced search space.");
            return;
        }

        positionRangeX1 = x1;
        positionRangeX2 = x2;
        positionRangeZ1 = z1;
        positionRangeZ2 = z2;

        userDefinedReducedBounds = true;
    }

}