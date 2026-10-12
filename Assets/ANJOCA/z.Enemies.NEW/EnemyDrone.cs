using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyDrone : MonoBehaviour
{
   [Header("Enemy Drone Health and Damage")] 
   private float enemyHealth = 120f;
   public float presentHealth;
   public float giveDamage = 3f;
   // public HealthBar healthBar;
   
   [Header("Enemy Drone Things")]
   // public NavMeshAgent enemyAgent;
   public Transform LookPoint;
   public Camera ShootingRaycastArea;
   public Transform playerBody;
   public LayerMask playerLayer;
   
   [Header("Enemy Drone Guarding Var")]
   public GameObject[] walkPoints;
   private int currentEnemyPosition = 0;
   public float enemySpeed;
   private float walkingPointRadius = 2;
   
   
   [Header("Sounds and UI")]
   // public AudioClip shootingSound;
   // public AudioClip flameSound;
   // public AudioSource audioSource;

   [Header("Enemy Drone Shooting Var")]
   public float timeBetweenShoot;
   bool previouslyShoot;

   [Header("Enemy Drone Animation and Spark Effect")]
   // public Animator anim;
   // public ParticleSystem muzzleSpark;
   // public ParticleSystem muzzleFlame;
   // public ParticleSystem DestroyEffect;

   [Header("Enemy Drone Mood/Situation")] 
   public float visionRadius;
   public float shootingRadius;
   public bool playerInVisionRadius;
   public bool playerInShootingRadius;
   
   // Usado para não ser necessário walkPoints nos inimigos fixos
   private Vector3 posicaoInicial;
   private Quaternion rotacaoInicial;
   
   // raios do Inspector, para repor quando volta ao posto ou à patrulha
   private float visionRadiusInicial;
   private float shootingRadiusInicial;
   
   // metros extra de visão dados por uma ZonaAlerta (0 = nenhum)
   private float bonusZona = 0f;
   
   // material que o inimigo passa a usar quando morre
   public Material materialMorto;
   
   // quão depressa roda enquanto cai
   public float giroQueda = 5f;
   
   // linha do tiro, igual à do Player
   public LineRenderer linhaTiro;
   public float duracaoLinha = 0.1f;

   private void Awake()
   {
      presentHealth = enemyHealth;
      // healthBar.GiveFullHealthBar(enemyHealth);
      playerBody = GameObject.Find("Player").transform;
      // enemyAgent =  GetComponent<NavMeshAgent>();
      
      posicaoInicial = transform.position;
      rotacaoInicial = transform.rotation;
      
      visionRadiusInicial = visionRadius;
      shootingRadiusInicial = shootingRadius;
      
      // a linha começa escondida
      if (linhaTiro != null) linhaTiro.enabled = false;
   }

   private void Update()
   {
      // morto não faz mais nada
      if (presentHealth <= 0) return;
      
      playerInVisionRadius = Physics.CheckSphere(transform.position, visionRadius, playerLayer);
      playerInShootingRadius = Physics.CheckSphere(transform.position, shootingRadius, playerLayer);

      if (!playerInVisionRadius && !playerInShootingRadius) Guard();

      if (playerInVisionRadius && !playerInShootingRadius) PursuePlayer();
      if (playerInVisionRadius && playerInShootingRadius) ShootPlayer(); 
   }

   


   /*private void Guard()
   {
      if (Vector3.Distance(walkPoints[currentEnemyPosition].transform.position, transform.position) < walkingPointRadius)
      {
         currentEnemyPosition = UnityEngine.Random.Range(0, walkPoints.Length);
         if (currentEnemyPosition >= walkPoints.Length)
         {
            currentEnemyPosition = 0;
         }
      }

      transform.position = Vector3.MoveTowards(transform.position, walkPoints[currentEnemyPosition].transform.position,
         Time.deltaTime * enemySpeed);
      // changing enemy facing
      transform.LookAt(walkPoints[currentEnemyPosition].transform.position);
   }*/
   
   private void Guard()
   {
      // perdeu o contacto com o player: os raios voltam aos do Inspector
      ReporRaios();
      
      // drone fixo: não tem Walk Points, fica a pairar no sítio onde foi posto
      if (walkPoints.Length == 0)
      {
         // voa em linha reta para a posição guardada no Awake(), à velocidade normal
         // se já lá estiver, não se mexe; se te perseguiu, volta
         VoarPara(posicaoInicial, enemySpeed);

         // a menos de 0,1 m do posto conta como chegado
         if (Vector3.Distance(transform.position, posicaoInicial) < 0.1f)
            // roda aos poucos (180 graus por segundo) até ficar virado como no início
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotacaoInicial, 180f * Time.deltaTime);

         // sai do Guard() para não correr o código da patrulha
         return;
      }
      
      Vector3 alvo = walkPoints[currentEnemyPosition].transform.position;
      VoarPara(alvo, enemySpeed);
      if (Vector3.Distance(transform.position, alvo) < 0.1f)
         currentEnemyPosition = (currentEnemyPosition + 1) % walkPoints.Length;
   }
   
   // volta aos raios que estavam no Inspector ao carregar Play
   private void ReporRaios()
   {
      visionRadius = visionRadiusInicial + bonusZona;
      shootingRadius = shootingRadiusInicial;
   }
   
   // chamada pela ZonaAlerta quando entras ou sais da zona
   public void BonusZona(float valor)
   {
      bonusZona = valor;
   }
   
   private void VoarPara(Vector3 alvo, float velocidade)
   {
      transform.position = Vector3.MoveTowards(transform.position, alvo, velocidade * Time.deltaTime);
      Vector3 direcao = alvo - transform.position;
      direcao.y = 0f;
      if (direcao.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(direcao);
   }
   
   
   private void PursuePlayer()
   {
      Vector3 alvo = new Vector3(playerBody.position.x, transform.position.y, playerBody.position.z);
      VoarPara(alvo, enemySpeed);
      visionRadius = 30f;
      // shootingRadius = 16f;
      
      
      /*if (enemyAgent.SetDestination(playerBody.position))
      {
         // animations
         // anim.SetBool("Walk", false);
         // anim.SetBool("AimRun", true);
         // anim.SetBool("Shoot", false);
         // anim.SetBool("Die", false);
         
         // + vision and shooting radius
         visionRadius = 30f;
         shootingRadius = 16f;
      }
      else
      {
         // anim.SetBool("Walk", false);
         // anim.SetBool("AimRun", false);
         // anim.SetBool("Shoot", false);
         // anim.SetBool("Die", true);
      }*/
      
      Disparar();
   }
   
   
   private void Disparar()
   {
      if (previouslyShoot) return;

      Vector3 origem = ShootingRaycastArea.transform.position;
      Vector3 direcao = (LookPoint.position - origem).normalized;
      
      // se não acertar em nada, a linha vai até ao alcance máximo
      Vector3 fim = origem + direcao * visionRadius;

      RaycastHit hit;
      if (Physics.Raycast(origem, direcao, out hit, visionRadius))
      {
         // Debug.Log("Shooting " + hit.transform.name);
         fim = hit.point;
         PlayerStats alvo = hit.transform.GetComponent<PlayerStats>();
         if (alvo != null) alvo.playerHitDamage(giveDamage);
      }
      
      
      // mostra a linha da ponta da arma até onde o tiro acabou
      if (linhaTiro != null)
      {
         linhaTiro.enabled = true;
         linhaTiro.SetPosition(0, origem);
         linhaTiro.SetPosition(1, fim);
         Invoke(nameof(EsconderLinha), duracaoLinha);
      }

      previouslyShoot = true;
      Invoke(nameof(ActiveShooting), timeBetweenShoot);
   }
   
   private void EsconderLinha()
   {
      linhaTiro.enabled = false;
   }
   
   private void ShootPlayer()
   {
      // enemyAgent.SetDestination(transform.position);
      
      transform.LookAt(LookPoint);

      /*if (!previouslyShoot)
      {
         // muzzleSpark.Play();
         // audioSource.PlayOneShot(shootingSound);
         // muzzleFlame.Play();
         // audioSource.PlayOneShot(flameSound);
         
         
         RaycastHit hit;

         if (Physics.Raycast(ShootingRaycastArea.transform.position, ShootingRaycastArea.transform.forward, out hit,
                shootingRadius))
         {
            Debug.Log("Shooting " + hit.transform.name);
            
            PlayerStats playerBody =  hit.transform.GetComponent<PlayerStats>();

            if (playerBody != null)
            {
               playerBody.playerHitDamage(giveDamage);
            }
            
            // anim.SetBool("Shoot", true);
            // anim.SetBool("Walk", false);
            // anim.SetBool("AimRun", false);
            // anim.SetBool("Die", false);
         }


         previouslyShoot = true;
         Invoke(nameof(ActiveShooting), timeBetweenShoot);
      }*/
      Disparar();
   }

   private void ActiveShooting()
   {
      previouslyShoot = false;
   }

   public void enemyDroneHitDamage(float takeDamage)
   {
      // já está morto: ignora os tiros seguintes
      if (presentHealth <= 0) return;
      
      presentHealth -= takeDamage;
      // healthBar.SetHealth(presentHealth);
      
      // + vision and shooting radius
      visionRadius = 40f;
      // shootingRadius = 30f;

      if (presentHealth <= 0)
      {
         // anim.SetBool("Shoot", false);
         // anim.SetBool("Walk", false);
         // anim.SetBool("AimRun", false);
         // anim.SetBool("Die", true);
         
         enemyDie();
      }
   }

   private void enemyDie()
   {
      // DestroyEffect.Play();
      // enemyAgent.SetDestination(transform.position);
      enemySpeed = 0f;
      shootingRadius = 0f;
      visionRadius = 0f;
      playerInVisionRadius = false;
      playerInShootingRadius =  false;
      
      // pinta de preto todas as malhas do inimigo (corpo, arma, peças filhas)
      if (materialMorto != null)
      {
         foreach (Renderer malha in GetComponentsInChildren<Renderer>())
         {
            // salta linhas e partículas, que não fazem parte do corpo
            if (malha is LineRenderer || malha is ParticleSystemRenderer) continue;

            // troca todos os materiais desta malha pelo preto
            Material[] mats = malha.sharedMaterials;
            for (int i = 0; i < mats.Length; i++) mats[i] = materialMorto;
            malha.sharedMaterials = mats;
         }
      }
      // enquanto voa é trigger e atravessa tudo; ao morrer passa a sólido para bater no chão
      GetComponent<Collider>().isTrigger = false;
      // liga a física para a gravidade o puxar para o chão
      Rigidbody rb = GetComponent<Rigidbody>();
      if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
      rb.isKinematic = false;
      rb.useGravity = true;

// põe-no a rodar numa direção ao acaso, para cair aos rebolões
      rb.angularVelocity = UnityEngine.Random.insideUnitSphere * giroQueda;
      
      Object.Destroy(gameObject, 5.0f);
   }
   
   // desenha os raios na Scene quando o inimigo está selecionado
   private void OnDrawGizmosSelected()
   {
      // raio de visão a amarelo
      Gizmos.color = Color.yellow;
      Gizmos.DrawWireSphere(transform.position, visionRadius);

      // raio de disparo a vermelho
      Gizmos.color = Color.red;
      Gizmos.DrawWireSphere(transform.position, shootingRadius);
   }
}
