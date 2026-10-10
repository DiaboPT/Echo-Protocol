using UnityEngine;

// Zona (por exemplo numa porta) que aumenta o raio de visão dos inimigos
// enquanto o Player estiver lá dentro.
// Precisa de um Collider com Is Trigger ligado no mesmo objeto.
public class ZonaAlerta : MonoBehaviour
{
   [Header("Inimigos afetados por esta zona")]
   public Enemy[] soldados;
   public EnemyDrone[] drones;

   [Header("Aumento")]
   // metros somados ao Vision Radius do Inspector de cada inimigo
   public float aumentoVisao = 10f;

   private void OnTriggerEnter(Collider other)
   {
      // só reage ao Player (é o único com PlayerStats)
      if (other.GetComponent<PlayerStats>() == null) return;
      AplicarBonus(aumentoVisao);
   }

   private void OnTriggerExit(Collider other)
   {
      if (other.GetComponent<PlayerStats>() == null) return;
      AplicarBonus(0f);
   }

   private void AplicarBonus(float valor)
   {
      // o != null salta os inimigos que já morreram e foram destruídos
      foreach (Enemy soldado in soldados)
         if (soldado != null) soldado.BonusZona(valor);

      foreach (EnemyDrone drone in drones)
         if (drone != null) drone.BonusZona(valor);
   }
}
