using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
   [Header("Enemy Health and Damage")] 
   private float enemyHealth = 120f;
   public float presentHealth;
   public float giveDamage = 5f;
   // public HealthBar healthBar;
   
   [Header("Enemy Things")]
   public NavMeshAgent enemyAgent;
   public Transform LookPoint;
   public Camera ShootingRaycastArea;
   public Transform playerBody;
   public LayerMask playerLayer;
   
   [Header("Enemy Guarding Var")]
   public GameObject[] walkPoints;
   private int currentEnemyPosition = 0;
   public float enemySpeed;
   private float walkingPointRadius = 2;


   [Header("Sounds and UI")] 
   // public AudioClip shootingSound;
   // public AudioSource audioSource;

   [Header("Enemy Shooting Var")]
   public float timeBetweenShoot;
   bool previouslyShoot;

   [Header("Enemy Animation and Spark Effect")]
   // public Animator anim;
   // public ParticleSystem muzzleSpark;

   [Header("Enemy Mood/Situation")] 
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
   
   private void Awake()
   {
      // audioSource =  GetComponent<AudioSource>();
      presentHealth = enemyHealth;
      // healthBar.GiveFullHealthBar(enemyHealth);
      playerBody = GameObject.Find("Player").transform;
      enemyAgent =  GetComponent<NavMeshAgent>();
      
      posicaoInicial = transform.position;
      rotacaoInicial = transform.rotation;
      
      visionRadiusInicial = visionRadius;
      shootingRadiusInicial = shootingRadius;
   }

   private void Update()
   {
      // morto não faz mais nada até objecto ser destruido 
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
      
      
      // Debug.Log(name + " | no NavMesh: " + enemyAgent.isOnNavMesh + " | caminho: " + enemyAgent.pathStatus + " | ponto: " + currentEnemyPosition);
      
      
      // inimigo fixo: não tem Walk Points, fica de guarda no sítio onde foi posto
      if (walkPoints.Length == 0)
      {
         // anda à velocidade normal (não à de perseguição)
         enemyAgent.speed = enemySpeed;

         // manda-o para a posição guardada no Awake()
         // se já lá estiver, fica parado; se te perseguiu, volta
         enemyAgent.SetDestination(posicaoInicial);

         // pathPending: o agente ainda está a calcular o caminho
         // remainingDistance: quanto lhe falta andar até ao destino
         // se já calculou e está a menos de 0,6 m, chegou ao posto
         // inimigo fixo: chegou ao posto
         if (!enemyAgent.pathPending && enemyAgent.remainingDistance < 0.6f)
         {
            // roda aos poucos (180 graus por segundo) até ficar virado como no início
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotacaoInicial, 180f * Time.deltaTime);
         }

         // sai do Guard() para não correr o código da patrulha
         return;
      }
      
      enemyAgent.speed = enemySpeed;

      Vector3 alvo = walkPoints[currentEnemyPosition].transform.position;
      Vector3 alvoMesmaAltura = new Vector3(alvo.x, transform.position.y, alvo.z);

      
      // chegou ao ponto atual: passa ao seguinte, por ordem
      if (Vector3.Distance(transform.position, alvoMesmaAltura) < 0.6f)
      {
         currentEnemyPosition = (currentEnemyPosition + 1) % walkPoints.Length;
         alvo = walkPoints[currentEnemyPosition].transform.position;
      }

      enemyAgent.SetDestination(alvo);
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
   
   
   private void PursuePlayer()
   {
      if (enemyAgent.SetDestination(playerBody.position))
      {
         // animations
         // anim.SetBool("Walk", false);
         // anim.SetBool("AimRun", true);
         // anim.SetBool("Shoot", false);
         // anim.SetBool("Die", false);
         //
         
         // + vision and shooting radius
         visionRadius = 30f;
         // shootingRadius = 16f;
      }
      else
      {
         // anim.SetBool("Walk", false);
         // anim.SetBool("AimRun", false);
         // anim.SetBool("Shoot", false);
         // anim.SetBool("Die", true);
      }
      Disparar();
   }
   
   private void Disparar()
   {
      if (previouslyShoot) return;

      Vector3 origem = ShootingRaycastArea.transform.position;
      Vector3 direcao = (LookPoint.position - origem).normalized;

      RaycastHit hit;
      if (Physics.Raycast(origem, direcao, out hit, visionRadius))
      {
         Debug.Log("Shooting " + hit.transform.name);
         PlayerStats alvo = hit.transform.GetComponent<PlayerStats>();
         if (alvo != null) alvo.playerHitDamage(giveDamage);
      }

      previouslyShoot = true;
      Invoke(nameof(ActiveShooting), timeBetweenShoot);
   }
   
   private void ShootPlayer()
   {
      enemyAgent.SetDestination(transform.position);
      
      transform.LookAt(LookPoint);

      /*if (!previouslyShoot)
      {
         
         // muzzleSpark.Play();
         // audioSource.PlayOneShot(shootingSound);
         
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

   public void enemyHitDamage(float takeDamage)
   {
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
      enemyAgent.SetDestination(transform.position);
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

