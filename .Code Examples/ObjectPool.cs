/******************************************************************************
 * Project: Unity
 * File: ObjectPool
 * Author: Stephan Weber (SW)
 * 
 * Description:
 *      This Singleton creates and manages any ObjectPool.
 *      It can be used for any GameObject.
 *      Every pool has a Prefab and a name, that is also used as a Key.
 *      
 *      You can easylie store (StoreObject(<poolname>, <object>))
 *      and get (GetObject(<poolname>)) object(s) of any created pool.
 *      
 *      If every pool slot is full, and another object should be stored,
 *      it automatically creates a slot for it. Also if every Slot is empty
 *      and you still want to get an Object, it will create a Slot and fill
 *      it with the entered Prefab.
 *      
 *      To Create Pools simply with the Unity Inspector, use the
 *      ObjectPoolPoolCreator class.
 * 
 * ***************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class PoolObject
    {
        public PoolObject(bool _isUsed, GameObject _gameObject)
        {
            isUsed = _isUsed;
            gameObject = _gameObject;
            gameObject.SetActive(false);
        }

        public bool isUsed = false;
        public GameObject gameObject = null;
    }

    /// <summary>
    /// The name of the Singleton Instance
    /// </summary>
    public static ObjectPool Instance { get; set; } = null;
    /// <summary>
    /// Shorter name to get the Singleton Instance
    /// </summary>
    public static ObjectPool OP { get { return Instance; } }

    /// <summary>
    /// The Prefabs for every Pool
    /// </summary>
    private readonly Dictionary<string, GameObject> poolPrefabs = new Dictionary<string, GameObject>();
    /// <summary>
    /// The Parent for every Pool (cepps the Hirarchy Organized)
    /// </summary>
    private readonly Dictionary<string, Transform> poolParents = new Dictionary<string, Transform>();
    /// <summary>
    /// All Pools
    /// </summary>
    private readonly Dictionary<string, List<PoolObject>> pools = new Dictionary<string, List<PoolObject>>();
    /// <summary>
    /// The names of every Pool
    /// </summary>
    public List<string> poolnames = new List<string>();

    private void Awake()
    {
        // Creating an Instance if no other already exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// Used to create a new Pool.
    /// </summary>
    /// <param name="_name">The name of the new Pool</param>
    /// <param name="_prefab">The Objects of the new Pool</param>
    /// <param name="_ammount">The size of the pool (how many Objects)</param>
    public void CreatePool(string _name, GameObject _prefab, int _ammount)
    {
        if (poolnames.Contains(_name))
        {
            Debug.LogError("There is already a pool with this Name.");
        }

        poolPrefabs.Add(_name, _prefab);
        pools.Add(_name, new List<PoolObject>());
        poolnames.Add(_name);

        GameObject newParent = new GameObject();
        newParent.transform.position = Vector3.zero;
        newParent.transform.rotation = Quaternion.identity;
        newParent.name = $"Pool of {_name}";
        newParent.transform.SetParent(this.transform);
        poolParents.Add(_name, newParent.transform);

        for (int i = 0; i < _ammount; i++)
        {
            pools[_name].Add(new PoolObject(false, Instantiate(_prefab, newParent.transform)));
        }
    }

    /// <summary>
    /// Used to get one object of an existing pool.
    /// </summary>
    /// <param name="_name">The name of the pool</param>
    /// <returns>One Object of the named pool.</returns>
    public GameObject GetObject(string _name)
    {
        if (!poolnames.Contains(_name))
        {
            Debug.LogError($"There is no pool with the name: {_name}");
            return null;
        }

        for (int i = 0; i < pools[_name].Count; i++)
        {
            if (!pools[_name][i].isUsed)
            {
                pools[_name][i].isUsed = true;
                pools[_name][i].gameObject.SetActive(true);
                return pools[_name][i].gameObject;
            }
        }

        pools[_name].Add(new PoolObject(false, Instantiate(poolPrefabs[_name], poolParents[_name])));
        pools[_name][pools[_name].Count - 1].isUsed = true;
        pools[_name][pools[_name].Count - 1].gameObject.SetActive(true);

        GameObject returnObject = pools[_name][pools[_name].Count - 1].gameObject;

        return returnObject;
    }

    /// <summary>
    /// Used to get a List of objects of an existing pool.
    /// </summary>
    /// <param name="_name">The name of the pool</param>
    /// <param name="_ammount">The ammount of Objects ou want to get</param>
    /// <returns>Returns a List of objects from the named pool with the entered size.</returns>
    public List<GameObject> GetObjects(string _name, int _ammount)
    {
        if (!poolnames.Contains(_name))
        {
            Debug.LogError($"There is no pool with the name: {_name}");
            return null;
        }

        List<GameObject> temp = new List<GameObject>();
        for (int i = 0; i < _ammount; i++)
        {
            temp.Add(GetObject(_name));
        }
        return temp;
    }

    /// <summary>
    /// Used to get an array of objects of an existing pool.
    /// </summary>
    /// <param name="_name">The name of the pool</param>
    /// <param name="_ammount">The ammount of Objects ou want to get</param>
    /// <returns>Returns an array of objects from the named pool with the entered size.</returns>
    public GameObject[ ] GetObjectsArr(string _name, int _ammount)
    {
        if (!poolnames.Contains(_name))
        {
            Debug.LogError($"There is no pool with the name: {_name}");
            return null;
        }

        return GetObjects(_name, _ammount).ToArray();
    }

    /// <summary>
    /// Used to stora an Object back into a pool.
    /// </summary>
    /// <param name="_name">The name of the pool</param>
    /// <param name="_object">The Object to store</param>
    public void StoreObject(string _name, GameObject _object)
    {
        if (!poolnames.Contains(_name))
        {
            CreatePool(_name, _object, 4);
        }

        for (int i = 0; i < pools[_name].Count; i++)
        {
            if (pools[_name][i].gameObject == _object)
            {
                pools[_name][i].isUsed = false;
                _object.SetActive(false);
                _object.transform.SetParent(poolParents[_name]);

                return;
            }
        }

        pools[_name].Add(new PoolObject(false, _object));
        _object.transform.SetParent(poolParents[_name]);
    }

    /// <summary>
    /// Used to store Multipls Objects back into a pool.
    /// </summary>
    /// <param name="_name">The name of the pool</param>
    /// <param name="_objects">The Objects to store</param>
    public void StoreObjects(string _name, List<GameObject> _objects)
    {
        if (!poolnames.Contains(_name))
        {
            CreatePool(_name, _objects[0], 5);
        }

        for (int i = 0; i < _objects.Count; i++)
        {
            StoreObject(_name, _objects[i]);
        }
    }

    /// <summary>
    /// Used to store Multipls Objects back into a pool.
    /// </summary>
    /// <param name="_name">The name of the pool</param>
    /// <param name="_objects">The Objects to store</param>
    public void StoreObjects(string _name, GameObject[ ] _objects)
    {
        for (int i = 0; i < _objects.Length; i++)
        {
            StoreObject(_name, _objects[i]);
        }
    }

    /// <summary>
    /// Used to store all Objects of one pool.
    /// </summary>
    /// <param name="_name">The name of the pool</param>
    public void StoreAllObjects(string _name)
    {
        if (!poolnames.Contains(_name)) return;

        for (int i = 0; i < pools[_name].Count; i++)
        {
            if (pools[_name][i].isUsed)
            {
                StoreObject(_name, pools[_name][i].gameObject);
            }
        }
    }
}
