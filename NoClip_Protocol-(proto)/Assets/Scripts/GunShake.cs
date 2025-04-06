using System.Collections;
using UnityEngine;

public class GunShake : MonoBehaviour
{
    public IEnumerator Shake(float duration, float magnitude) {
        Vector3 ogPos = transform.localPosition;
        float elapsed = 0.0f;

        while(elapsed < duration) {
            float x = Random.Range(-0.01f, 0.01f) * magnitude;
            float y = Random.Range(-0.01f, 0.01f) * magnitude;
            float z = Random.Range(-0.01f, 0.01f) * magnitude;

            transform.localPosition = new Vector3(x, y, z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = ogPos;
    }
}
