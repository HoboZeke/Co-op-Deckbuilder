using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinObject : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag("Table"))
        {
            AudioManager.main.CoinHittingWoodAudioEvent();
        }
    }
}
