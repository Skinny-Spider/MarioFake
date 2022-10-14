using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public FirstPersonController firstPersonController;

    public AudioSource coinAudioSource;
    public AudioSource jumpAudioSource;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(hit.gameObject.tag == "Deadly")
        {
            Respawn();
        }
        if(hit.gameObject.tag == "Bouncy")
        {
            jumpAudioSource.Play();
            firstPersonController.DoHighJump();
        }
        if(hit.gameObject.tag == "Coin")
        {
            coinAudioSource.Play();
            Destroy(hit.gameObject);
        }
        if(hit.gameObject.tag == "FallingPlatform")
        {
            Rigidbody rigidbody = hit.gameObject.GetComponent<Rigidbody>();
            rigidbody.useGravity = true;
        }
    }

    void Update()
    {
        if(transform.position.y < -50)
        {
            Respawn();
        }
    }

    void Respawn()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
