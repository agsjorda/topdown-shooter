using UnityEngine;

public class DarknessController : MonoBehaviour
{
    public Material darknessMat;
    public Transform player;
    public float viewRadius = 5f;

    void Update()
    {
        if (darknessMat && player) {
            darknessMat.SetVector("_PlayerPosition", player.position);
            darknessMat.SetFloat("_ViewRadius", viewRadius);
        }
    }
}
