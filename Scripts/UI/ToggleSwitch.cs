using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Script para o botão de Toggle

public class ToggleSwitch : MonoBehaviour, IPointerClickHandler
{
    // Cria uma chave para salvar as configurações
    [Header("Save Settings")]
    [Tooltip("Chave única para salvar esta configuração no PlayerPrefs")]
    [SerializeField] private string saveKey = "Toggle_Setting_Key";
    [SerializeField] private bool saveAutomatically = true;

    // Valor do Slider entre 0 e 1
    [Header("Slider setup")]
    [SerializeField, Range(0, 1f)] private float sliderValue;
    public bool CurrentValue { get; private set; }

    private Slider _slider;

    // Tempo de animação
    [Header("Animation")]
    [SerializeField, Range(0, 1f)] private float animationDuration = 0.5f; // Tempo aqui
    [SerializeField] private AnimationCurve slideEase = AnimationCurve.EaseInOut(0, 0, 1, 1); // Curva da animação

    private Coroutine _animateSliderCoroutine;

    // Eventos de ligar e desligar o botão
    [Header("Events")]
    [SerializeField] private UnityEvent onToggleOn;
    [SerializeField] private UnityEvent onToggleOff;

    private ToggleSwitchGroupManager _toggleSwitchGroupManager;

    // Pra atualizar quando algo mudar no inspetor
    protected void OnValidate()
    {
        SetupToggleComponents();
        _slider.value = sliderValue;
    }

    // Caso o slider não tenha sido apontado
    private void SetupToggleComponents()
    {
        if (_slider != null) return;
        SetupSliderComponent();
    }

    // Coleta o componente do slider
    private void SetupSliderComponent()
    {
        _slider = GetComponent<Slider>();
        if (_slider == null)
        {
            Debug.Log("No Slider found!", this);
            return;
        }

        _slider.interactable = false; // Impede o usuário de deslizar
        var sliderColors = _slider.colors;
        sliderColors.disabledColor = Color.white;
        _slider.colors = sliderColors;
        _slider.transition = Selectable.Transition.None;
    }

    // Define o gerenciador
    public void SetupForManager(ToggleSwitchGroupManager manager)
    {
        _toggleSwitchGroupManager = manager;
    }

    private void Awake()
    {
        SetupToggleComponents();
    }

    private void Start()
    {
        // Só carrega automaticamente se não estiver em um gerenciador de grupo
        if (_toggleSwitchGroupManager == null && saveAutomatically)
        {
            LoadSetting();
        }
    }

    // Para quando for apertado
    public void OnPointerClick(PointerEventData eventData)
    {
        Toggle();
    }

    // Desliga ou liga
    private void Toggle()
    {
        if (_toggleSwitchGroupManager != null)
            _toggleSwitchGroupManager.ToggleGroup(this);
        else
            SetStateAndStartAnimation(!CurrentValue);
    }

    // Para fazer grupo de botões
    public void ToggleByGroupManager(bool valueToSetTo)
    {
        SetStateAndStartAnimation(valueToSetTo);
    }

    // Ativa um evento de acordo com o valor
    private void SetStateAndStartAnimation(bool state)
    {
        CurrentValue = state;

        // Salva o estado atual
        if (saveAutomatically)
        {
            SaveSetting();
        }

        if (CurrentValue)
            onToggleOn?.Invoke();
        else
            onToggleOff?.Invoke();

        if (_animateSliderCoroutine != null) // Reseta possíveis corotinas
            StopCoroutine(_animateSliderCoroutine);

        _animateSliderCoroutine = StartCoroutine(AnimateSlider());
    }

    // Animação do slider
    private IEnumerator AnimateSlider()
    {
        float startValue = _slider.value;
        float endValue = CurrentValue ? 1 : 0;
        float time = 0;

        if (animationDuration > 0)
        {
            while (time < animationDuration)
            {
                time += Time.deltaTime;
                float lerpFactor = slideEase.Evaluate(time / animationDuration);
                _slider.value = sliderValue = Mathf.Lerp(startValue, endValue, lerpFactor);
                yield return null;
            }
        }

        _slider.value = endValue;
    }

    // Método para salvar as configurações
    private void SaveSetting()
    {
        // PlayerPrefs não aceita bool, então usamos 1 para true e 0 para false
        PlayerPrefs.SetInt(saveKey, CurrentValue ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Método para carregar as configurações
    private void LoadSetting()
    {
        // Se a chave não existir, usa o valor padrão definido no slider do inspetor
        bool defaultValue = sliderValue >= 0.5f;

        int savedValue = PlayerPrefs.GetInt(saveKey, defaultValue ? 1 : 0);
        CurrentValue = savedValue == 1;

        // Atualiza o slider instantaneamente
        _slider.value = sliderValue = CurrentValue ? 1f : 0f;

        // Ativa os eventos iniciais para ajustar ao estado salvo
        if (CurrentValue)
            onToggleOn?.Invoke();
        else
            onToggleOff?.Invoke();
    }
    private bool _isInitialized = false;
    public void InitializeSetting()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        SetupToggleComponents();
        if (_toggleSwitchGroupManager == null && saveAutomatically)
        {
            LoadSetting();
        }
    }
}
