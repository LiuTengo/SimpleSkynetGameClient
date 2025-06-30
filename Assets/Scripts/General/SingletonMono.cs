using UnityEngine;

namespace General
{
    public class SingletonMono<T>  : MonoBehaviour where T : MonoBehaviour
    {
        public static T instance => m_Instance;
        private static T m_Instance;

        protected virtual void Awake()
        {
            if (instance != null)
            {
                Destroy(this.gameObject);
            }
            m_Instance = this as T;
        }
    }
}
