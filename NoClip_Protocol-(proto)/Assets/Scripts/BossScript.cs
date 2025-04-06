using UnityEngine;

/*
Plan if only one boss:
    Just really big enemy that's only slightly slower than the player
    Will be able to see the player at all times and can move through walls

Plan if multiple bosses:
    First Boss:
        Really big and fast. Tons of health and hits hard. Always drawn on top to confuse player.
    Second Boss:
        Uses NoCLip Effect to travel through walls. It will turn purple and transluscent when doing so.
        Will fight with NoClipSword.
    Third Boss:
        Restricts Player's ability to NoCLip, can use NoClip energy itself.
        Player can still activate NoClip to use the Sowrd but they cannot defy gravity or see through walls.
    Final Boss:
        Rendered on top, can NoClip, will run away to heal if they get too hurt, will
        take away all NoClip Energy from player and fully restores self. Makes scary noises to spook the player
*/

public class BossScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
