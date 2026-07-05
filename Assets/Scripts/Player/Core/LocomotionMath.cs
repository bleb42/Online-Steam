using UnityEngine;

public static class LocomotionMath
{
    public static float VariableOverrideDelayTimer(float timeVariable)
    {
        if (timeVariable > 0.0f)
        {
            timeVariable -= Time.deltaTime;
            timeVariable = Mathf.Clamp(timeVariable, 0.0f, 1.0f);
        }
        else
        {
            timeVariable = 0.0f;
        }

        return timeVariable;
    }
}