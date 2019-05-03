using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnArrivedLocation(Actor actor);

public class Actor : MonoBehaviour
{
    static List<Actor> activeActors = new List<Actor>();

    public OnArrivedLocation onArrivedLocation;

    Animator animator;

    public float arrivalDistance = 0.1f;
    Vector3 dest;
    bool pathing = false;

    public static Actor GetActorByName(string name)
    {
        foreach (Actor actor in activeActors)
        {
            if (actor.name.ToLower() == name.ToLower())
                return actor;
        }

        return null;
    }

    public void Awake()
    {
        animator = GetComponent<Animator>();
        activeActors.Add(this);
    }

    public void OnDestroy()
    {
        activeActors.Remove(this);
    }

    private void Update()
    {
        if (pathing)
        {
            Vector3 vec = (dest - transform.position);
            if (vec.magnitude > arrivalDistance)
                transform.Translate(vec.normalized * Time.deltaTime * 4);
            else
            {
                pathing = false;
                if (onArrivedLocation != null)
                    onArrivedLocation(this);
            }
        }
    }

    public void MoveToLocation(Vector3 location)
    {
        dest = location;
        pathing = true;
    }

    public void PlayAnimation(string animation)
    {
        animator.Play(animation);
    }

}
