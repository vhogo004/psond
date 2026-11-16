using UnityEngine;

// Opção pro menu de criar do botão direito
[CreateAssetMenu(fileName = "NewMovementData", menuName = "Data/Movement Data")]
public class MovementData : ScriptableObject
{
    // A variável fica aqui;
    public float movementTime = 2f;
}