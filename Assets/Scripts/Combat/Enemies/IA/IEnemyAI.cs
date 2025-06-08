
public interface IEnemyAI
{
    // Se llama para inicializar la IA con una referencia a su enemigo.
    void Initialize(Enemy enemyInstance);

    // Este es el método que TurnManager llamará cuando sea el turno del enemigo.
    // Aquí es donde la IA del enemigo decide y realiza su acción.
    void PerformTurnAction(); 
}