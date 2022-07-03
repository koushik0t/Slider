using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// This provides a proper singleton implementation in a base class to make creating new singletons easy. Please use this
/// rather than the lazy singletons of old to avoid future issues.
/// <para/>
/// There are two steps to creating a new singleton using this base class. First, have your MonoBehaviour inherit from Singleton&lt;YourClassHere&gt;.
/// Second, call the <see cref="InitializeSingleton"/> method inside of Awake, using the optional arguments to customize the behavior of your singleton.
/// </summary>
/// <remarks>Author: Travis</remarks>
public abstract class Singleton<T> : MonoBehaviour 
    where T : Singleton<T>
{
    protected static T _instance;

    /// <summary>
    /// This should be called in Awake inside of all singleton components. This sets up _instance to be the component instance,
    /// enforces singleton-ness, and logs an error if one or more duplicates of the component are detected.
    /// </summary>
    /// <param name="allowInactiveDuplicates">If this is true, inactive duplicates of this singleton component will not be deleted, 
    /// nor will errors be logged because of them.</param>
    /// <param name="overrideExistingInstanceWith">If this is true, this method will overwrite an existing _instance with the passed 
    /// in instance (which should probably be the calling instance.) This can be useful if you have a singleton present in multiple scenes 
    /// which does not persist as _instance will be updated to refer to the current one when Awake is called during the scene transition.</param>
    /// <returns>True if _instance was successfully updated, false otherwise</returns>
    protected virtual bool InitializeSingleton(bool allowInactiveDuplicates = false, T overrideExistingInstanceWith = null)
    {
        if (overrideExistingInstanceWith != null)
        {
            // If we allowInactiveDuplicates, then we won't override the existing instance if the passed in new instance isn't active...
            // this should probably just not be supported to be honest, but I'll clean this up later(tm)
            if (!(allowInactiveDuplicates && !overrideExistingInstanceWith.isActiveAndEnabled) && overrideExistingInstanceWith != _instance)
            {
                Destroy(_instance);
                _instance = overrideExistingInstanceWith;
                return true;
            }
            return false;
        } 
        else
        {
            if (_instance != null)
            {
                Debug.LogError($"Multiple Singleton components of type {typeof(T)} were detected. The latest one was deleted.");
                Destroy(this);
                return false;
            }
            else
            {
                CheckForAndDestroyDuplicates(allowInactiveDuplicates);
                _instance = FindObjectOfType<T>();
                return true;
            }
        }
    }

    /// <summary>
    /// This should be called in Awake inside of all singleton components. This sets up _instance to be the component instance,
    /// enforces singleton-ness, and logs an error if one or more duplicates of the component are detected.
    /// </summary>
    /// /// <param name="destroyIfInstanceIsAlreadySet">If _instance is already set, the passed in GameObject will be destroyed.</param>
    /// <param name="allowInactiveDuplicates">If this is true, inactive duplicates of this singleton component will not be deleted, 
    /// nor will errors be logged because of them</param>
    /// <returns>True if _instance was successfully updated, false otherwise</returns>
    protected virtual bool InitializeSingleton(GameObject destroyIfInstanceIsAlreadySet, bool allowInactiveDuplicates = false)
    {
        if (destroyIfInstanceIsAlreadySet != null && _instance != null)
        {
            Destroy(destroyIfInstanceIsAlreadySet);
            return true;
        } else
        {
            CheckForAndDestroyDuplicates(allowInactiveDuplicates);
            _instance = FindObjectOfType<T>();
            return false;
        }
    }

    private void CheckForAndDestroyDuplicates(bool allowInactiveDuplicates)
    {
        // I'm using LINQ here for funsies, regular for loops would be fine (and probably easier to read) :)
        IEnumerable<T> instances = FindObjectsOfType<T>();
        if (allowInactiveDuplicates)
        {
            instances = instances.Where(instance => instance.isActiveAndEnabled);
        }
        if (instances.Count() > 1)
        {
            Debug.LogError($"{instances.Count()} Singleton components of type {typeof(T)} were detected. An arbitrary one was chosen " +
                "to keep. This needs to be fixed immediately to avoid catastrophic consequences. Don't make us come after you. " +
                "We will not forgive your sins.");
            foreach (T instance in instances)
            {
                Debug.Log(instance.gameObject.name);
            }
        }
        instances.Skip(1).ToList().ForEach(instance => Destroy(instance));
    }
}