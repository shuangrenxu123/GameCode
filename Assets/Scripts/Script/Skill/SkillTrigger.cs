using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillTrigger : MonoBehaviour
{
    public SkillRunner runner;
    public Animator anim;
    public AudioSource AudioSource;
    public bool isFinish { get {
            if(runner != null)
                return runner.isFinish;
            else return false;
        } }
    public virtual void Start()
    {
        anim = GameObject.Find("Enemy").transform.GetChild(0).gameObject.GetComponent<Animator>();
        AudioSource = GetComponent<AudioSource>();
        runner = new SkillRunner(anim,AudioSource,transform);
    }
    private void Update()
    {
        runner.Update();
    }
    public virtual void OnStart()
    {

    }
    public virtual void OnUpdate(float time)
    {

    }
    public virtual void OnFinish()
    {

    }
}
