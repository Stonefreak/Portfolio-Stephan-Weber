/******************************************************************************
 * Project: Unity
 * File: MyMinMax
 * Author: Stephan Weber (SW)
 * 
 * Description:
 *      This class makes it easy to create a random Number inbetween to values.
 *      Also this class can be used in the Inpector.
 *      The min- and maxvalues can also be accesed separately.
 * 
 * ***************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MyMinMax 
{
    private enum STEP_SIZE { RANDOM = 0, ONES = 1, FIVES = 5, TENS = 10, HUNDRETS = 100 }

    #region Contructors
    public MyMinMax()
    {
        minValue = 0;
        maxValue = 0;
    }

    public MyMinMax(float _min, float _max)
    {
        minValue = _min;
        maxValue = _max;
    }

    public MyMinMax(int _min, int _max)
    {
        minValue = (float)_min;
        maxValue = (float)_max;
    }
    #endregion

    /// <summary>
    /// The Minimum Value
    /// </summary>
    [SerializeField] private float minValue = 0;
    public float MinValue { get { return minValue; } private set { minValue = value; } }

    /// <summary>
    /// The Maximum Value
    /// </summary>
    [SerializeField] private float maxValue = 0;
    public float MaxValue { get { return maxValue; } private set { maxValue = value; } }

    /// <summary>
    /// The Step size between the min and max values
    /// </summary>
    [SerializeField] private STEP_SIZE stepSize = STEP_SIZE.RANDOM;

    /// <summary>
    /// Get Random Value between the min and the max Value (stepSize included).
    /// </summary>
    public float RandomValue
    {
        get
        {
            // getting a Random number between the min and max values
            float randomValue = Random.Range(minValue, MaxValue);

            // changing the value to fit the step size
            if (stepSize != STEP_SIZE.RANDOM)
            {
                if (randomValue % (int)stepSize < (int)stepSize * 0.5f)
                {
                    randomValue -= randomValue % (int)stepSize;
                }
                else
                {
                    randomValue += (int)stepSize - (randomValue % (int)stepSize);
                }
            }

            return randomValue;
        }
    }

    /// <summary>
    /// To get the progress of the entered value between the Min- and MaxValues
    /// </summary>
    /// <param name="_value">The value to get the Progress for</param>
    /// <returns>Progress between 0.0f and 1.0f</returns>
    public float Progress(float _value)
    {
        _value -= MinValue;
        float fullProgress = MaxValue - MinValue;

        return _value / fullProgress;
    }

    /// <summary>
    /// To get a Lerped value between the Min- and MaxValues
    /// </summary>
    /// <param name="_value">The value to Lerp to between 0.0 and 1.0</param>
    /// <returns>The lerped Value</returns>
    public float Lerp(float _value)
    {
        return Mathf.Lerp(minValue, maxValue, _value);
    }

    /// <summary>
    /// To clamp a float between thew min and max values
    /// </summary>
    /// <param name="_value">value to clamp</param>
    /// <returns>clamped value</returns>
    public float Clamp(float _value)
    {
        return Mathf.Clamp(_value, MinValue, MaxValue);
    }
}