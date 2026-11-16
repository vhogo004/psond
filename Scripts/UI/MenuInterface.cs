using System.Linq;
using TMPro; 
using UnityEngine;
using UnityEngine.UI;

public class MenuInterface : MonoBehaviour
{
    [Header("Scripts que receberão a função")]
    
    [SerializeField] private PSO scriptPSO;
    [SerializeField] private FunctionMesh scriptMesh;
    [SerializeField] private CameraController scriptOrbitCamera;
    [SerializeField] private UIMenuManager scriptUIMenuManager;

    [SerializeField] private InputManager scriptInputManager;
    [Header("Componente de Interface")]
    // [SerializeField] private TMP_Dropdown dropdownFuncoes;

    // Dropdown para o tipo de função
    [SerializeField] public TMP_Dropdown dropdownFunctionType;
    // Botão para iniciar o algoritmo
    [SerializeField] private Button startAlgButton;
    // Botão para autopreencher o PSO
    [SerializeField] private Button autofillPSOButton;
    // Botão para autopreencher a função
    [SerializeField] private Button autofillFunctionButton;
    // Botão da visibilidade da UI
    [SerializeField] private Button hideUIButton;

    // Menu de start
    public GameObject startMenuUI;

    public UIMenuManager uiStartMenuScript;

    // Menu Secundário (PSO)
    public GameObject secondMenuUI;

    // Menu de Configurações
    public GameObject configMenuUI;

    public UIMenuManager uiConfigScript;

    // UI enquanto roda geral (Pause e não pause, etc)
    public GameObject psoRunningUI;

    // UI normal enquanto roda
    public GameObject runningNormalUI;

    // Menu de Pause
    public GameObject runningOptionsUI;

    // Overlay do Pause
    public GameObject pauseOverlayUI;
    public GameObject pauseOverlayUI2;

    // Tela de Configuração do menu de Pause
    public GameObject secondConfigUI;

    // Tela para perguntar se o usuário deseja finalizar a simulação
    public GameObject finalizeWindow;

    // Tela final da simulação
    public GameObject finalWindow;

    public GameObject normalUI;

    [SerializeField] private TMP_Text textIteration;
    [SerializeField] private TMP_Text textgBestPos;
    [SerializeField] private TMP_Text textgBestFit;
    [SerializeField] private TMP_Text textpBestPos;
    [SerializeField] private TMP_Text textpBestFit;
    [SerializeField] private TMP_Text textpPos;
    [SerializeField] private TMP_Text textpOrigin;
    [SerializeField] private TMP_Text textpDestiny;

    [SerializeField] private TMP_Text textIterationF;
    [SerializeField] private TMP_Text textPopulationF;
    [SerializeField] private TMP_Text textgBestPosF;
    [SerializeField] private TMP_Text textgBestFitF;
    [SerializeField] private TMP_Text textWeight;
    [SerializeField] private TMP_Text textCoeficient1;
    [SerializeField] private TMP_Text textCoeficient2;
    [SerializeField] private TMP_Text textExecution;
    [SerializeField] private TMP_Text textDimensionF;
    [SerializeField] private TMP_Text textX;
    [SerializeField] private TMP_Text textZ;
    [SerializeField] private TMP_Text textXOriginal;
    [SerializeField] private TMP_Text textZOriginal;
    [SerializeField] private TMP_Text textFitnessRec;
    public bool uiVisible = true;
    private bool closingByMenuButton = false;
    private bool runningConfig = false;

    public Image imagePlay;
    public Image imagePause;

    //[SerializeField] private TMP_InputField speedSettingInput;

    //[SerializeField] private MovementData speedSettings;

    void Start()
    {
        if (dropdownFunctionType != null)
        {
            // Remove ouvintes antigos para evitar duplicidade e adiciona o novo
            //dropdownFuncoes.onValueChanged.RemoveAllListeners();
            dropdownFunctionType.onValueChanged.RemoveAllListeners();
           // dropdownFuncoes.onValueChanged.AddListener(OnFunctionTypeDropdownChanged);
            dropdownFunctionType.onValueChanged.AddListener(OnFunctionTypeDropdownChanged);
            // OnFunctionTypeDropdownChanged(dropdownFuncoes.value);
            OnFunctionTypeDropdownChanged(dropdownFunctionType.value);
        }

        if (startAlgButton != null)
        {
            startAlgButton.onClick.RemoveAllListeners();
            //startAlgButton.onClick.AddListener(OnStartAlgButtonPressed);
        }
        //speedSettingInput.text = 5.ToString();
    }


    public void OnFunctionTypeDropdownChanged(int indice)
    {
        // Converte o índice do dropdown (0 para a primeira opção, 1 para a segunda)
        PSO.FunctionType newFunction = (PSO.FunctionType)indice;

        
        int previewDimensions = 2; // Valor padrão
        previewDimensions = scriptInputManager.ValidateDimension();
        // Envia para o segundo script
        if (scriptPSO != null && previewDimensions > 1)
        {
            // O StartOptimization calcula o novo Redutor para a função selecionada
            
            scriptPSO.StartOptimization(newFunction, previewDimensions, 1);
        }
    }

    // OnBotaoIniciarClicado()
    // OnStartAlgButtonPressed()

    [SerializeField] public TMP_InputField dimensionsInputField;

    public void OnStartAlgButtonPressed()
    {
        int realDimensions = 2;

        if (dimensionsInputField != null && !string.IsNullOrEmpty(dimensionsInputField.text))
        {
            // Converte o texto digitado pelo usuário para um número inteiro estável
            if (int.TryParse(dimensionsInputField.text, out int parsedValue))
            {
                realDimensions = parsedValue;
            }
        }
        else
        {
            // Se o campo estiver vazio, usa o valor que já estava configurado como padrão no PSO
            realDimensions = scriptPSO.dimensions;
        }

        PSO.FunctionType selectedFunction = (PSO.FunctionType)dropdownFunctionType.value;
        scriptPSO.StartOptimization(selectedFunction, realDimensions, 0);

        // Quando o jogador clica no botão, ele "destrava" o Update dos dois scripts
        if (scriptPSO != null) scriptPSO.StartPSO();
        if (scriptMesh != null) scriptMesh.StartMeshFunction();
        psoRunningUI.SetActive(true);
        runningNormalUI.SetActive(true);
        scriptOrbitCamera.ChangeCameraMode(2);
        scriptUIMenuManager.HideMenu();
        //secondMenuUI.SetActive(false);
    }

    // BOTÕES DO MENU PRINCIPAL
    // COMEÇAR
    public void OnStartMenuPress()
    {
        //startMenuUI.SetActive(false); // Desativa o primeiro
        secondMenuUI.SetActive(true); // Ativa o segundo
    }

    public void OnConfigMenuPress()
    {
        //startMenuUI.SetActive(false);
        configMenuUI.SetActive(true);
    }

    public void OnReturnConfigMenuPress()
    {
        //configMenuUI.SetActive(false);
        checkConfigReturn();
        //startMenuUI.SetActive(true);
    }

    // SAIR DO PROGRAMA
    public void OnExitMenuPress()
    {
        Application.Quit();
    }


    // BOTÕES DO MENU DO PSO (SECUNDÁRIO)
    
    // BOTÃO DE VOLTAR
    public void OnReturnSecMenuPress()
    {
        //secondMenuUI.SetActive(false);
        startMenuUI.SetActive(true);
    }

    // PAUSE
    public void OnRunningOptionsPausePress()
    {
        runningOptionsUI.SetActive(true);
        pauseOverlayUI.SetActive(true);
        //startMenuUI.SetActive(true);
    }

    // UNPAUSE
    public void OnRunningOptionsUnpausePress()
    {
        runningOptionsUI.SetActive(false);
        pauseOverlayUI.SetActive(false);
        //startMenuUI.SetActive(true);
    }

    // BOTÃO DE CONFIGURAÇÃO DO MENU DE PAUSE
    public void OnRunningConfigPress()
    {
        runningNormalUI.SetActive(false);
        runningOptionsUI.SetActive(false);
        configMenuUI.SetActive(true);
        runningConfig = true;
        //secondConfigUI.SetActive(true);
    }

    public void checkConfigReturn()
    {
        // Se o botão foi apertado pela tela de pause
        if(runningConfig)
        {
            runningConfig = false;
            OnRunningConfigReturnPress();
        }
        else // Se não
        {
            startMenuUI.SetActive(true);
            //OnReturnConfigMenuPress();
            uiStartMenuScript.ShowMenu();
            uiConfigScript.HideMenu();
            //configMenuUI.gameObject.
            //MostrarMenu(Principal);
            
            //EsconderMenu(configMenuUI);
        }
    }
    // BOTÃO DE VOLTAR DO MENU DE CONFIG DO MENU DE PAUSE
    public void OnRunningConfigReturnPress()
    {
        runningNormalUI.SetActive(true);
        runningOptionsUI.SetActive(true);
        configMenuUI.SetActive(false);
        //secondConfigUI.SetActive(false);
    }

    // BOTÃO DE VOLTAR DENTRO DO MENU DE PAUSE (ENQUANTO RODA)
    public void OnRunningReturnPress()
    {
        runningNormalUI.SetActive(false);
        runningOptionsUI.SetActive(false);
        pauseOverlayUI.SetActive(false);
        psoRunningUI.SetActive(false);
        secondMenuUI.SetActive(true);
    }

    // Botão de autopreencher as configurações do PSO
    //public void OnAutoFillPSOPress()
    //{
        // Preenche os botões na UI
        // Envia para o PSO (ou talvez envie para o PSO quando for preenchido?)
    //}
    // Botão de autopreencher as configurações da função
    //public void OnAutoFillFunctionPress()
    //{
        // Preenche os botões na UI (função)
        
    //}

    public void UpdatePlayUI(int uIteration, Vector3 uGBestPos, float uGBestFit)
        //,Vector3 uPBestPos, float uPBestFit, Vector3 uPPos, Vector3 uPOrigin, Vector3 uPDestiny)
    {


        textIteration.text = uIteration.ToString();
        //textgBestPos.text = uGBestPos.ToString();
        // Pega só o x e z
        textgBestPos.text =
            $"X: {uGBestPos.x:F2} | Z: {uGBestPos.z:F2}";
        //textgBestFit.text = uGBestFit.ToString();
        textgBestFit.text = uGBestFit.ToString("E6");
        if (scriptPSO.selectionExists)
        {
            return;
        }
        else
        {
            textpBestPos.text = "";
            textpBestFit.text = "";
            textpPos.text = "";
            textpOrigin.text = "";
            textpDestiny.text = "";
        }
    }

    public void UpdatePlayUI2(Vector3 uPBestPos, float uPBestFit, Vector3 uPPos, Vector3 uPOrigin, Vector3 uPDestiny)
    {
        if (scriptPSO.selectionExists)
        {
            //textpBestPos.text = uPBestPos.ToString();
            textpBestPos.text =
                $"X: {uPBestPos.x:F4} | Z: {uPBestPos.z:F4}";
            textpBestFit.text = uPBestFit.ToString("E6");
            //textpBestFit.text = uPBestFit.ToString();
            textpPos.text = uPPos.ToString();
            textpOrigin.text = uPOrigin.ToString();
            textpDestiny.text = uPDestiny.ToString();
        }
        else
        {
            textpBestPos.text = "";
            textpBestFit.text = "";
            textpPos.text = "";
            textpOrigin.text = "";
            textpDestiny.text = "";
        }
    }

    public void OpenFinalizeWindow()
    {
        runningOptionsUI.SetActive(false);
        finalizeWindow.SetActive(true);
    }

    public void CloseFinalizeWindow()
    {
        finalizeWindow.SetActive(false);
        pauseOverlayUI.SetActive(false);
    }

    public void OpenFinalWindow()
    {
        runningOptionsUI.SetActive(false);
        runningNormalUI.SetActive(false);
        finalWindow.SetActive(true);
        pauseOverlayUI.SetActive(false);
        pauseOverlayUI2.SetActive(true);
        runningNormalUI.SetActive(false);
    }

    public void CloseFinalWindow()
    {
        finalWindow.SetActive(false);
        pauseOverlayUI2.SetActive(false);
        // Se foi pelo botão do menu, realmente chama a função
        // Se foi chamada pelo botão de esconder próprio, não chama
        // Pra não dar erro de ordem
        if(closingByMenuButton == true)
        {
            hideHideButton();
            UpdatePlayText();
        }
    }

    // Função pra quando voltar para algum menu no final e esconder o botão de esconder menus
    // Mas só funciona se o botão apertado foi o do menu mesmo
    public void hideHideButton()
    {
        runningNormalUI.SetActive(true); // Seta esta tela como true
        psoRunningUI.SetActive(false); // Seta esta tela como false, agora o botão de esconder some também
        closingByMenuButton = false; // Seta o valor dele como false
    }

    // Deixa como true
    public void hideButtonTrue()
    {
        closingByMenuButton = true;
    }

    // Deixa como false;
    public void hideButtonFalse()
    {
        closingByMenuButton = false;
    }

    public void BuildFinalWindow(Vector3 pos, float fit, double[] originalPosition, double reconstructedFitness)
    {
        string reducedX = pos.x.ToString("F6");
        string reducedZ = pos.z.ToString("F6");

        string fitnessT = fit.ToString("E6");
        string reconstructed = reconstructedFitness.ToString("E6");

        string minCoordinate = originalPosition.Min().ToString("F6");
        string maxCoordinate = originalPosition.Max().ToString("F6");

        textX.text = reducedX;
        textZ.text = reducedZ;
        textXOriginal.text = minCoordinate;
        textZOriginal.text = maxCoordinate;
        textFitnessRec.text = reconstructed;
        textgBestFitF.text = fitnessT;
        textIterationF.text = scriptPSO.currentIteration.ToString();
        textPopulationF.text = scriptPSO.populationSize.ToString();
        //textgBestPosF.text = pos.ToString();
        //textgBestFitF.text = fit.ToString();
        textWeight.text = scriptPSO.w.ToString();
        textCoeficient1.text = scriptPSO.c1.ToString();
        textCoeficient2.text = scriptPSO.c2.ToString();
        textDimensionF.text = scriptInputManager.returnDimension(); // Coleta a dimensão de acordo com o campo
    }

    public void ChangeUI()
    {
        if (uiVisible == true)
        {
            uiVisible = false;
            HideAllUI();
        }
        else
        {
            uiVisible = true;
            ShowAllUI();
        }
    }

    public void HideAllUI()
    {
        if(scriptPSO.startedAlg)
        {
            runningNormalUI.SetActive(false);
        }
        else
        {
            closingByMenuButton = false; // Pra ter certeza
            CloseFinalWindow();
            scriptOrbitCamera.ChangeCameraMode(2);
        }
    }

    public void ShowAllUI()
    {
        if (scriptPSO.startedAlg)
        {
            runningNormalUI.SetActive(true);
        }
        else
        {
            OpenFinalWindow();
            scriptOrbitCamera.ChangeCameraMode(3);
        }
    }

    public void ShowSecondMenu()
    {
        psoRunningUI.SetActive(false);
        secondMenuUI.SetActive(true);
    }

    public void ShowMainMenu()
    {
        psoRunningUI.SetActive(false);
        startMenuUI.SetActive(true);
    }

    public void OnStartMenuButtonPress()
    {
     
        if (scriptPSO != null)
        {
            scriptPSO.canUpdateMeshVisuals = true;
        }

        if (scriptMesh != null)
        {
            // Salva o estado do botão
            bool state = (PlayerPrefs.GetInt("Config_PremadeFunction", 1) == 1);
            scriptPSO.usingPremadeFunction = true; // Marca como true
            scriptMesh.generateMesh();
            scriptPSO.usingPremadeFunction = state; // Devolve ao estado
        }
    }

    public void UpdatePlayText()
    {
        textExecution.text = "Em execução";
        imagePlay.color = Color.green;
        imagePause.color = Color.white;
    }

    public void UpdatePauseText()
    {
        textExecution.text = "Pausado";
        imagePause.color = Color.red;
        imagePlay.color = Color.white;
    }
}