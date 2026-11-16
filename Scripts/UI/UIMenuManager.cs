using System.Collections;
using UnityEngine;

public class UIMenuManager : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private RectTransform painelUI;
    [SerializeField] private float duracao = 0.4f;

    [Tooltip("Para onde o menu vai quando ESCONDER. Ex: (1920, 0) vai para a direita. (-1920, 0) vai para a esquerda.")]
    [SerializeField] private Vector2 vetorDeslize = new Vector2(1920f, 0f);

    [Header("Estado Inicial")]
    [Tooltip("Marque se este menu deve começar o jogo já escondido fora da tela.")]
    [SerializeField] private bool comecarEscondido = false;

    private Vector2 posicaoNaTela;
    private Vector2 posicaoForaDaTela;
    private bool estaNaTela = true;
    private Coroutine rotinaMovimento;



    void Awake()
    {
        if (painelUI == null) painelUI = GetComponent<RectTransform>();

        // A posição no Editor SEMPRE será a posição correta na tela
        posicaoNaTela = painelUI.anchoredPosition;

        // A posição escondida será a posição da tela + o vetor de deslize
        posicaoForaDaTela = posicaoNaTela + vetorDeslize;

        // Se deve começar escondido, teleporta o menu para fora antes do jogador ver
        if (comecarEscondido)
        {
            painelUI.anchoredPosition = posicaoForaDaTela;
            estaNaTela = false;
            painelUI.gameObject.SetActive(false); // Desativa para economizar performance
        }
        else
        {
            estaNaTela = true;
        }
    }

    public void ShowMenu()
    {
        if (estaNaTela) return;

        estaNaTela = true;
        painelUI.gameObject.SetActive(true);
        ChangeState(posicaoNaTela);
    }

    public void HideMenu()
    {
        if (!estaNaTela) return;

        estaNaTela = false;
        ChangeState(posicaoForaDaTela);
    }

    private void ChangeState(Vector2 destino)
    {
        if (rotinaMovimento != null) StopCoroutine(rotinaMovimento);
        rotinaMovimento = StartCoroutine(MovePanel(destino));
    }

    private IEnumerator MovePanel(Vector2 destino)
    {
        Vector2 posicaoAtual = painelUI.anchoredPosition;
        float tempoDecorrido = 0f;

        while (tempoDecorrido < duracao)
        {
            tempoDecorrido += Time.deltaTime;
            float porcentagem = tempoDecorrido / duracao;
            float tSuave = Mathf.SmoothStep(0f, 1f, porcentagem);

            painelUI.anchoredPosition = Vector2.Lerp(posicaoAtual, destino, tSuave);
            yield return null;
        }

        painelUI.anchoredPosition = destino;

        // Se terminou de esconder, desativa o objeto
        if (!estaNaTela)
        {
            painelUI.gameObject.SetActive(false);
        }

        rotinaMovimento = null;
    }
}
