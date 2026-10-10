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
    [SerializeField] private float damage = 20f;
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
        Vector3 origin = SpawnPoint.position;
        Vector3 direction = SpawnPoint.forward;

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
            if (soldado != null)
                soldado.enemyHitDamage(damage);

            EnemyDrone drone = hit.collider.GetComponentInParent<EnemyDrone>();
            if (drone != null)
                drone.enemyDroneHitDamage(damage);
            /****************/
        }

        rayVisual.enabled = true;

        rayVisual.SetPosition(0, origin);
        rayVisual.SetPosition(1, endPoint);
    }

    private void HideRay() {
        rayVisual.enabled = false;
    }
}
