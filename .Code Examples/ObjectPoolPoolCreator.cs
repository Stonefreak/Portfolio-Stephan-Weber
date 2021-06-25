/******************************************************************************
 * Project: Unity
 * File: ObjectPoolPoolCreator
 * Author: Stephan Weber (SW)
 * 
 * Description:
 *      !! CAUTION !! this file onyl works together with ObjectPool.cs!
 *      
 *      This MonoBehaviour is to simply create ObjectPools in a simple way
 *      within the Unity Inspector.
 *      It helps keeping the Code organized and creates the ability to
 *      create Prefabs of ObjectPool creations.
 * 
 * ***************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolPoolCreator : MonoBehaviour
{
    /// <summary>
    /// The Preset for every single Pool you want to create via the Inspector
    /// </summary>
    [System.Serializable]
    public struct PoolPreset
    {
        /// <summary>
        /// The Name of the Pool
        /// </summary>
        [SerializeField] private string poolName;
        public string PoolName { get { return poolName; } }

        /// <summary>
        /// the Prefab for the Pool
        /// </summary>
        [SerializeField] private GameObject poolPrefab;
        public GameObject PoolPrefab { get { return poolPrefab; } }

        /// <summary>
        /// The size of the Pool || how many object should be in the pool
        /// </summary>
        [SerializeField] private int poolSize;
        public int PoolSize { get { return poolSize; } }

    }

    /// <summary>
    /// Defines if the pools should only be created, if the created ObjectPool instance
    /// does not have any pools yet or not
    /// </summary>
    [SerializeField] private bool onlyAtStart = true;

    /// <summary>
    /// A List of all Pools that will be created
    /// </summary>
    [SerializeField] private List<PoolPreset> poolPresets = new List<PoolPreset>();

    private void Start()
    {
        // Only create if it should (if not, destroy Object)
        if (onlyAtStart && ObjectPool.OP.poolnames.Count != 0)
        {
            Destroy(this.gameObject);
            return;
        }

        // Looping over every pool preset to create the pools
        for (int i = 0; i < poolPresets.Count; i++)
        {
            ObjectPool.OP.CreatePool(poolPresets[i].PoolName, poolPresets[i].PoolPrefab, poolPresets[i].PoolSize);
        }

        // Destroy Object after creating the pools
        Destroy(this.gameObject);
    }
}