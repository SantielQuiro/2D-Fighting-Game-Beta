using UnityEngine;


public interface IParryable
{
    void OnParried(float staminaTaken);
} //Player and Enemy must have this interface to be parried
