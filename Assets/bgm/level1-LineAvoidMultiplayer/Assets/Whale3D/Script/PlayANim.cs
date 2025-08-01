using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayANim : MonoBehaviour
{

    public Animator pla1;
    public Animator pla2;
    public string anim1;
    public string anim2;
    public string anim3;
    public string anim4;
    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    // Update is called once per frame
    public void anim1Press()
    {
        pla1.Play(anim1);
        pla2.Play(anim1);
    }
    public void anim2Press()
    {
        pla1.Play(anim2);
        pla2.Play(anim2);
    }
    public void anim3Press()
    {
        pla1.Play(anim3);
        pla2.Play(anim3);
    }
    public void anim4Press()
    {
        pla1.Play(anim4);
        pla2.Play(anim4);
    }

}
