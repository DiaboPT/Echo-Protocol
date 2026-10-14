using UnityEngine;

// Vida humana + energia do Fato do ANJOCA-01. Pôr no jogador.
public class PlayerStats : MonoBehaviour
{
    [Header("Vida humana (só recupera com packs)")]
    public float maxHealth = 100f;

    [Header("Energia do Fato")]
    public float maxEnergy = 100f;
    public float regenCap = 30f;    // a regeneração automática só enche até aqui
    public float regenRate = 2f;    // energia por segundo a regenerar
    public float regenDelay = 3f;   // segundos sem gastar energia antes de regenerar

    [Header("Valores atuais (ver durante o Play)")]
    public float health;
    public float energy;
    public bool isDead;

    SuitAbilities abilities;
    float lastEnergyUseTime;

    void Awake()
    {
        health = maxHealth;
        energy = maxEnergy;
        abilities = GetComponent<SuitAbilities>();
    }

    void Update()
    {
        if (isDead) return;

        // Regeneração lenta: só se não gastou energia há algum tempo, e só até regenCap
        bool semGastarHaTempo = Time.time - lastEnergyUseTime >= regenDelay;
        if (semGastarHaTempo && energy < regenCap)
        {
            energy += regenRate * Time.deltaTime;
            if (energy > regenCap) energy = regenCap;
        }
    }

    // ---------- Função corrigida para tirar vida
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        // Blindagem ligada: sofre só uma parte do dano
        if (abilities != null && abilities.armorActive)
            amount = amount * abilities.armorDamageMultiplier;

        health -= amount;

        if (health <= 0f)
        {
            health = 0f;
            isDead = true;
            Debug.Log("Sinal perdido");

            // tomba o player
            PlayerQueda queda = GetComponent<PlayerQueda>();
            if (queda != null) queda.Morrer();

            PlayerDie();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        health = Mathf.Min(health + amount, maxHealth);
    }

    // ---------- Energia ----------
    // Gasta energia (aprimoramentos, laser do drone). Nunca desce abaixo de 0 e nunca mata.
    public void DrainEnergy(float amount)
    {
        energy = Mathf.Max(energy - amount, 0f);
        lastEnergyUseTime = Time.time;
    }

    // Estação de energia
    public void ChargeEnergy(float amount)
    {
        energy = Mathf.Min(energy + amount, maxEnergy);
    }
    
    
    
    // public void playerHitDamage(float takeDamage)
    // {
    //     health -= takeDamage;
    //     // healthBar.SetHealth(presentHealth);
    //     //  audioSource.PlayOneShot(playerHurtSound);
    //
    //     if (health <= 0)
    //     {
    //         // tomba o player
    //         GetComponent<PlayerQueda>().Morrer();
    //         
    //         PlayerDie();
    //     }
    // }
    
    private void PlayerDie()
    {
        // avisa o terminal de autodestruição: se a contagem estiver a correr, pára e mostra a derrota
        TerminalAutodestruicao terminal = FindFirstObjectByType<TerminalAutodestruicao>();
        if (terminal != null) terminal.JogadorMorreu();
        
        // EndGameMenuUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        // deathCamera.SetActive(true);
        Object.Destroy(gameObject, 1.0f);
    }
}
