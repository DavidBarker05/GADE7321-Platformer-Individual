using UnityEngine;

public class FloatScript : MonoBehaviour
{
    Vector3 startingPosition;

    void Start() => startingPosition = transform.position;

    void LateUpdate() => transform.position = startingPosition + Vector3.up * WaterWaveManager.Instance.GetWaterHeight(startingPosition);
}
