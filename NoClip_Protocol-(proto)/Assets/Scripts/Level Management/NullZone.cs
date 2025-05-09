using UnityEngine;

public class NullZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        NoClipEffect noClip = other.GetComponent<NoClipEffect>();
        if ( noClip!= null) {
            noClip.isInNullZone = true;
        }
    }
    private void OnTriggerExit(Collider other) {
        NoClipEffect noClip = other.GetComponent<NoClipEffect>();
        if ( noClip!= null) {
            noClip.isInNullZone = false;
        }
    }
}
