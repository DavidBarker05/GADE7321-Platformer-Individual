using System;
using UnityEngine;

// EnemyFactory is just a MonoBehavior wrapper for the internal real factory
public class EnemyFactory : MonoBehaviour
{
    static EnemyFactory s_Instance;
    public static EnemyFactory Instance
    {
        get
        {
            if (!s_Instance)
            {
                GameObject go = new GameObject("EnemyFactory");
                s_Instance = go.AddComponent<EnemyFactory>();
                s_Instance.m_EnemyFactory = new EnemyFactoryInternal();
            }
            return s_Instance;
        }
    }

    EnemyFactoryInternal m_EnemyFactory;

	void Awake()
	{
        if (s_Instance && s_Instance != this) Destroy(gameObject);
        else
        {
            s_Instance = this;
            m_EnemyFactory = new EnemyFactoryInternal();
        }
	}

	/// <summary>
	/// <para>To create a <seealso cref="StationaryEnemy"/>, args = (<seealso cref="StationaryEnemy"/> prefab, <seealso cref="Transform"/> parent)</para>
	/// </summary>
	/// <typeparam name="T">The type of <seealso cref="BaseEnemy"/> to create</typeparam>
	/// <param name="args">The arguments needed to create the enemy</param>
	/// <returns>An enemy of the type <typeparamref name="T"/></returns>
	public T Create<T>(params object[] args) where T : BaseEnemy => m_EnemyFactory.Create(typeof(T), args) as T;

    class EnemyFactoryInternal : AbstractFactory<BaseEnemy>
	{
		public override BaseEnemy Create(params object[] args)
		{
            if (args == null || args.Length == 0 || args[0] is not Type t)
            {
                Debug.LogWarning("To spawn an enemy args[0] should be the type of enemy to spawn");
                return null;
            }
            if (t == typeof(StationaryEnemy))
            {
                if (args.Length != 3 || args[1] is not StationaryEnemy prefab || args[2] is not Transform parent)
                {
                    Debug.LogWarning("To spawn a stationary enemy args[0] needs to be the prefab and args[1] needs to be the parent transform");
                    return null;
                }
                return CreateStationaryEnemy(prefab, parent);
            }
            throw new NotImplementedException($"There is no definition to create an enemy of type {t}");
		}

        public StationaryEnemy CreateStationaryEnemy(StationaryEnemy prefab, Transform parent) => Instantiate(prefab, parent);
	}
}
