using UnityEngine;

// Esse Script serve para gerar uma cor aleatória para o material de cada partícula

public class ParticleColor : MonoBehaviour
{
    public GameObject gameObjectMat;
    public Color color;
    public Material material;
    private float randomChannelOne, randomChannelTwo, randomChannelThree;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObjectMat = this.gameObject;
        material = gameObjectMat.GetComponent<MeshRenderer>().material;
        RandomColor();

        // se for float RandomColor(), da pra usar apenas uma variável de canal e chamar a função 3x
        // material.color = new Color(RandomColor(), RandomColor(), RandomColor(), 1f);
    }

    // Update is called once per frame
    void Update()
    {
        // Isso aqui faz o material ser atualizado a cada frame
        // RandomColor();
    }

    // Gera 3 valores aleatórios para cada canal de cor possível, e aplica a cor do material. 
    void RandomColor()
    {
        randomChannelOne = Random.Range(0f, 1f); // Valor 1
        randomChannelTwo = Random.Range(0f, 1f); // Valor 2
        randomChannelThree = Random.Range(0f, 1f); // Valor 3

        color = new Color(randomChannelOne, randomChannelTwo, randomChannelThree, 1f); // Aplica os valores aos 3 canais, respectivamente
        material.color = color; // Atualiza a cor do material
    }
}
