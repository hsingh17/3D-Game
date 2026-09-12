using System;
using UnityEngine;

public struct AnimationState
{
    public string Name { get; set; }
    public int Hash { get; set; }
    public float Speed { get; set; }

    public AnimationState(string name)
    {
        Name = name;
        Hash = Animator.StringToHash(name);
        Speed = 1;
    }

    public AnimationState(string name, float speed)
    {
        Name = name;
        Hash = Animator.StringToHash(name);
        Speed = speed;
    }

    public override bool Equals(object obj)
    {
        return obj != null
            && obj.GetType() == GetType()
            && obj is AnimationState animationState
            && Name == animationState.Name;
    }

    public override int GetHashCode()
    {
        return Hash;
    }
}
