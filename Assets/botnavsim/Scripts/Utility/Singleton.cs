/// <summary>
/// Singleton is a design pattern that restricts instantiation of a class to one object.
/// This class was taken from an example on the internet.
/// </summary>

using UnityEngine;
using System.Collections;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour 
{
	private static T _instance;
	
	private static object _lock = new object();
	private static bool applicationIsQuitting = false;
	
	public void OnDestroy()
	{
		//applicationIsQuitting = true;
	}
	
	public static T Instance
	{
		get
		{
			if (applicationIsQuitting)
			{
				Debug.LogWarning("[Singleton] Instance '"+ typeof(T) +
				                 "' already destroyed on application quit." +
				                 " Won't create again - returning null.");
				return null;
			}
			
			lock(_lock)
			{
				if (_instance == null)
				{
					_instance = (T) FindObjectOfType(typeof(T));
					
					if (FindObjectsOfType(typeof(T)).Length > 1)
					{
						Debug.LogError("[Singleton] Something went really wrong " +
						               " - there should never be more than one singleton!" +
						               "Reopening the scene might fix this...");
						return _instance;
					}
					
					if (_instance == null)
					{
						GameObject singleton = new GameObject();
						_instance = singleton.AddComponent<T>();
						singleton.name = "(singleton) " + typeof(T).ToString();
						
						DontDestroyOnLoad(singleton);
						
						Debug.Log("[Singleton] An instance of " + typeof(T) + 
						          " is needed in the scene, so '" + singleton +
						          "' was created with DonDestoryOnLoad.");
					} else{
						Debug.Log ("[Singleton] Using instance already created: " +
						           _instance.gameObject.name);
					}
				}
				return _instance;
			}
		}
	}
}