using UnityEngine;

public class DetectionBasic : MonoBehaviour
{
    public int detectionLevel = 0;
    public int detectionUpperLimit;
    public int detectionPursueLimit;
    public float detectionReduceTimer = 0f;

    private void Update()
    {
        detectionReduceTimer += Time.deltaTime;

        if (detectionReduceTimer >= 1f)
        {
            detectionReduceTimer = 0f;
            if (detectionLevel > 0) detectionLevel--;
        }
    }
}
