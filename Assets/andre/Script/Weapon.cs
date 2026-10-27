using UnityEngine;
using UnityEngine.InputSystem;

public class Weapon : MonoBehaviour {
    [Header("Raycast")]
    [SerializeField] private float range = 100f;
    [SerializeField] private LayerMask hitLayers;

    [Header("Visual")]
    [SerializeField] private LineRenderer rayVisual;
    [SerializeField] private Transform SpawnPoint;

    [Header("Input")]
    [SerializeField] private InputActionReference fireAction;
    
    
    /**** CARLOS ****/
    [Header("Dano")]
    public float damage;

    [Header("Mira")]
    // de onde sai o raio do tiro (vazio = Main Camera, ou seja, o centro do ecrã)
    [SerializeField] private Transform aimOrigin;
    /****************/
    
    
    private void OnEnable() {
        fireAction.action.Enable();
    }

    private void OnDisable() {
        fireAction.action.Disable();
    }

    
    private void LateUpdate() {
        if (fireAction.action.WasPressedThisFrame())
            Shoot();
        else if (!fireAction.action.IsPressed())
            HideRay();
    }
    
    // private void LateUpdate() {
    //     if (fireAction.action.IsPressed())
    //         Shoot();
    //     else
    //         HideRay();
    // }

    private void Shoot() {
        // Vector3 origin = SpawnPoint.position;
        // Vector3 direction = SpawnPoint.forward;

        /**** CARLOS ****/
        // o raio parte da câmara (centro do ecrã), para acertar exatamente onde está a mira.
        // a linha verde continua a sair da arma (SpawnPoint)
        Transform mira = aimOrigin;
        if (mira == null) mira = Camera.main != null ? Camera.main.transform : SpawnPoint;
        Vector3 origin = mira.position;
        Vector3 direction = mira.forward;
        /****************/

        Vector3 endPoint = origin + direction * range;

        if (Physics.Raycast(
            origin,
            direction,
            out RaycastHit hit,
            range,
            hitLayers)
        ) {
            endPoint = hit.point;

            Debug.Log($"Hit: {hit.collider.name}");
            
            /**** CARLOS ****/
            Enemy soldado = hit.collider.GetComponentInParent<Enemy>();
            if (soldado != null) {
                // sangue no ponto onde a bala acertou
                soldado.MostrarSangue(hit.point, hit.normal);

                // tiro na cabeça (collider com a tag Head): tira a vida toda de uma vez
                if (hit.collider.CompareTag("Head"))
                    soldado.enemyHitDamage(soldado.presentHealth, true);
                else
                    soldado.enemyHitDamage(damage);
            }

            EnemyDrone drone = hit.collider.GetComponentInParent<EnemyDrone>();
            if (drone != null)
                drone.enemyDroneHitDamage(damage);
            /****************/
        }

        /**** CARLOS ****/
        // a linha do tiro só aparece com o debug visual (F3)
        rayVisual.enabled = ModoDebugVisual.Tiros;
        /****************/

        rayVisual.SetPosition(0, SpawnPoint.position);
        rayVisual.SetPosition(1, endPoint);
    }

    private void HideRay() {
        rayVisual.enabled = false;
    }
}
