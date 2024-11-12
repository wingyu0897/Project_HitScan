using System;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager<T> : IDisposable where T : Component
{
    [SerializeField] private T _prefab;
    
    public ObjectPool<T> Pool { get; private set; }

    private bool _aboutToDestroy = false;

    public PoolManager(T prefab, int defaultSize, int maxSize)
	{
        _prefab = prefab;
        Pool = new ObjectPool<T>(CreateObject, OnGetObject, OnReleaseObject, OnDestroyObject, false, defaultSize, maxSize);
	}

    private T CreateObject()
	{
        T obj = GameObject.Instantiate(_prefab);
        return obj;
	}

    private void OnGetObject(T obj)
	{
        obj.gameObject.SetActive(true);
	}

    private void OnReleaseObject(T obj)
	{
        obj.gameObject.SetActive(false);

        if (_aboutToDestroy)
		{
            GameObject.Destroy(obj.gameObject);
		}
    }

    private void OnDestroyObject(T obj)
	{
        GameObject.Destroy(obj.gameObject);
	}

	public void Dispose()
	{
        Pool.Clear();
        _aboutToDestroy = true;
    }
}
