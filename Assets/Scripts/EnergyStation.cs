using UnityEngine;

// Estação de energia: recarrega o Fato enquanto o jogador estiver dentro.
// Pôr num objeto com Collider marcado como "Is Trigger".
public class EnergyStation : MonoBehaviour
{
    public float chargeRate = 25f;   // energia por segundo

    float nextLogTime;               // para escrever na Console só 1 vez por segundo

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerStats>() != null)
            Debug.Log("A carregar a energia do Fato...");
    }

    void OnTriggerStay(Collider other)
    {
        PlayerStats stats = other.GetComponentInParent<PlayerStats>();
        if (stats == null || stats.isDead) return;

        stats.ChargeEnergy(chargeRate * Time.deltaTime);

        if (Time.time >= nextLogTime)
        {
            string message = "Energia do Fato: " + stats.energy.ToString("0") + "/" + stats.maxEnergy.ToString("0");
            if (stats.energy >= stats.maxEnergy) message += " (cheio)";
            Debug.Log(message);

            nextLogTime = Time.time + 1f;
        }
    }

    void OnTriggerExit(Collider other)
    {
        PlayerStats stats = other.GetComponentInParent<PlayerStats>();
        if (stats != null)
            Debug.Log("Carregamento parado. Energia do Fato: " + stats.energy.ToString("0"));
    }
}
