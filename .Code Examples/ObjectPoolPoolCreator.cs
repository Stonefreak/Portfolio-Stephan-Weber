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
    private enum DeleteMode { DontDelete, DeleteComponent, DeleteObject }

    /// <summary>
    /// The Preset for every single Pool you want to create via the Inspector
    /// </summary>
    [System.Serializable]
    public struct PoolPreset
    {
        /// <summary>
        /// Basic Constructor to fill every variable.
        /// </summary>
        /// <param name="_poolName">The Name of the new Pool</param>
        /// <param name="_poolPrefab">The Prefab of the new Pool</param>
        /// <param name="_poolSize">The Ammount of Objects in the new Pool</param>
        public PoolPreset(string _poolName, GameObject _poolPrefab, int _poolSize)
        {
            poolName = _poolName;
            poolPrefab = _poolPrefab;
            poolSize = _poolSize;
        }

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

    /// <summary>
    /// Delete after wokr? If yes, how
    /// </summary>
    [SerializeField] private DeleteMode deleteMode = DeleteMode.DeleteObject;

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
        switch (deleteMode)
        {
            case DeleteMode.DeleteComponent:
            {
                Destroy(this);
            }
            break;

            case DeleteMode.DeleteObject:
            {
                Destroy(this.gameObject);
            }
            break;

            case DeleteMode.DontDelete:
            default:
            break;
        }
    }
}