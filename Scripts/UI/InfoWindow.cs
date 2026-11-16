using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class InfoWindow : MonoBehaviour
{
    [Header("Info Window")]
    public GameObject infoWindow;
    [SerializeField] private Button closeButton;

    private bool clickDetection = false;

    [Header("Info Buttons")]
    [SerializeField] private Button buttonIteration;
    [SerializeField] private Button buttonPopulation;
    [SerializeField] private Button buttonWeight;
    [SerializeField] private Button buttonCoeficient;
    [SerializeField] private Button buttonSearchSpace;
    [SerializeField] private Button buttonResolution;
    [SerializeField] private Button buttonSpacing;
    [SerializeField] private Button buttonMathScale;
    [SerializeField] private Button buttonHeightY;


    [Header("Text Fields")]
    [SerializeField] private TMP_Text textIteration;

    [SerializeField] private TMP_Text textPopulation;
    [SerializeField] private TMP_Text textWeight;
    [SerializeField] private TMP_Text textCoeficient;
    [SerializeField] private TMP_Text textSearchSpace;
    [SerializeField] private TMP_Text textResolution;
    [SerializeField] private TMP_Text textSpacing;
    [SerializeField] private TMP_Text textMathScale;
    [SerializeField] private TMP_Text textHeightY;
    [SerializeField] private TMP_Text textFunction;
    [SerializeField] private TMP_Text textDimension;

    [SerializeField] private TMP_Text lastActiveText;

    public void showInfoWindow()
    {
        infoWindow.SetActive(true);
        clickDetection = true;
    }

    public void closeInfoWindow()
    {
        infoWindow.SetActive(false);
        lastActiveText.enabled = false;
        clickDetection = false;
    }


    void Update()
    {
        if(clickDetection && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if(!IsMouseOverTarget(infoWindow))
            {
                closeInfoWindow();
            }
        }

    }

    // Função para checar se o mouse está sobre a janela
    private bool IsMouseOverTarget(GameObject infoWindow)
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // Evento de clique sobre a posição do mouse
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = mousePosition
        };

        // Raycast que traz os objetos atingidos
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        // Se a janela ou outro botão dentro foi atingido
        foreach (RaycastResult result in results)
        {
            // Se tocou em um filho dela ou nela, o clique foi dado dentro
            if (result.gameObject == infoWindow || result.gameObject.transform.IsChildOf(infoWindow.transform))
            {
                return true;
            }
        }

        // Se não tocou em nada dentro dela ou nela, está tocando fora
        return false;
    }

    // Funções para mostrar o texto específico do botão selecionado
    public void iterationInfo()
    {
        textIteration.enabled = !textIteration.enabled;
        lastActiveText = textIteration;
        showInfoWindow();
        infoWindow.GetComponent<RectTransform>().anchoredPosition = new Vector3(480, -442, 0);
        //infoWindow.transform.position = new Vector3(480, -442, 0);
    }

    public void populationInfo()
    {
        textPopulation.enabled = !textPopulation.enabled;
        lastActiveText = textPopulation;
        showInfoWindow();
        infoWindow.GetComponent<RectTransform>().anchoredPosition = new Vector3(480, -483, 0);
        //infoWindow.transform.position = new Vector3(480, -483, 0);
    }

    public void weightInfo()
    {
        textWeight.enabled = !textWeight.enabled;
        lastActiveText = textWeight;
        //infoWindow.transform.position = new Vector3(480, -557, 0);
        infoWindow.GetComponent<RectTransform>().anchoredPosition = new Vector3(480, -557, 0);
        showInfoWindow();
    }

    public void coeficientInfo()
    {
        textCoeficient.enabled = !textCoeficient.enabled;
        lastActiveText = textCoeficient;
        //infoWindow.transform.position = new Vector3(480, -557, 0);
        infoWindow.GetComponent<RectTransform>().anchoredPosition = new Vector3(480, -557, 0);
        showInfoWindow();
    }

    public void searchSpaceInfo()
    {
        textSearchSpace.enabled = !textSearchSpace.enabled;
        lastActiveText = textSearchSpace;
        showInfoWindow();
        //infoWindow.transform.position = new Vector3(211, -442, 0);
        infoWindow.GetComponent<RectTransform>().anchoredPosition = new Vector3(211, -442, 0);
    }

    public void resolutionInfo()
    {
        textResolution.enabled = !textResolution.enabled;
        lastActiveText = textResolution;
        showInfoWindow();
        //infoWindow.transform.position = new Vector3(211, -545, 0);
        infoWindow.GetComponent<RectTransform>().anchoredPosition = new Vector3(211, -545, 0);
    }

    public void spacingInfo()
    {
        textSpacing.enabled = !textSpacing.enabled;
        lastActiveText = textSpacing;
        showInfoWindow();
        //infoWindow.transform.position = new Vector3(211, -605, 0);
        infoWindow.GetComponent<RectTransform>().anchoredPosition = new Vector3(211, -605, 0);
    }


    public void mathScaleInfo()
    {
        textMathScale.enabled = !textMathScale.enabled;
        lastActiveText = textMathScale;
        showInfoWindow();
        //infoWindow.transform.position = new Vector3(211, -605, 0);
        infoWindow.GetComponent<RectTransform>().anchoredPosition = new Vector3(211, -605, 0);
    }

    public void heightYInfo()
    {
        textHeightY.enabled = !textHeightY.enabled;
        lastActiveText = textHeightY;
        showInfoWindow();
        //infoWindow.transform.position = new Vector3(211, -605, 0);
        infoWindow.GetComponent<RectTransform>().anchoredPosition = new Vector3(211, -605, 0);
    }

    public void functionInfo()
    {
        textFunction.enabled = !textFunction.enabled;
        lastActiveText = textFunction;
        showInfoWindow();
        infoWindow.GetComponent<RectTransform>().anchoredPosition = new Vector3(464, -225, 0);
    }

    public void dimensionInfo()
    {
        textDimension.enabled = !textDimension.enabled;
        lastActiveText = textDimension;
        showInfoWindow();
        infoWindow.GetComponent<RectTransform>().anchoredPosition = new Vector3(464, -298, 0);
    }
}
