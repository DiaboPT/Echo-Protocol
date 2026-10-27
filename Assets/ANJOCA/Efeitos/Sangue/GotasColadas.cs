using UnityEngine;

// Vai no prefab Sangue (Particle System), para as gotas ficarem coladas onde batem.
//
// Porque é preciso: com o Collision em Dampen 1, uma gota que bate na parede fica sem velocidade,
// mas a gravidade do Particle System continua a puxá-la e ela escorrega pela parede abaixo.
// Aqui é o script que faz de gravidade, e só nas gotas que ainda estão no ar.
// Uma gota parada (já bateu em algo) deixa de levar gravidade e fica onde está.
//
// No Particle System: Gravity Modifier a 0, Collision com Dampen 1 e Bounce 0.
[RequireComponent(typeof(ParticleSystem))]
public class GotasColadas : MonoBehaviour
{
   // 1 = gravidade normal (como o Gravity Modifier a 1)
   public float gravidade = 1f;

   private ParticleSystem ps;
   private ParticleSystem.Particle[] gotas;

   private void Awake()
   {
      ps = GetComponent<ParticleSystem>();
      gotas = new ParticleSystem.Particle[ps.main.maxParticles];
   }

   private void LateUpdate()
   {
      int quantas = ps.GetParticles(gotas);
      Vector3 queda = Physics.gravity * gravidade * Time.deltaTime;

      for (int i = 0; i < quantas; i++)
      {
         // velocidade quase 0: já bateu numa parede ou no chão, fica colada
         if (gotas[i].velocity.sqrMagnitude < 0.0001f) continue;
         gotas[i].velocity += queda;
      }

      ps.SetParticles(gotas, quantas);
   }
}
