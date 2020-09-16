using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AppInGap.Managers
{
    /// <summary>
    /// A class that enables Unity's game loop to be used on instances of classes not derived from MonoBehaviour.
    /// </summary>
    public sealed class Updater : MonoBehaviour
    {
        #region Properties

        private static Updater m_DefaultUpdater;
        public static Updater defaultUpdater
        {
            get
            {
                if(!m_DefaultUpdater)
                {
                    GameObject gameObject = new GameObject(string.Format("[{0}.defaultUpdater]", typeof(Updater)));
                    GameObject.DontDestroyOnLoad(gameObject);

                    m_DefaultUpdater = gameObject.AddComponent<Updater>();
                }

                return m_DefaultUpdater;
            }
        }

        #endregion

        #region Private fields
        
        private ICollection<IUpdatable> m_Targets = new HashSet<IUpdatable>();
        private ICollection<IUpdatable> m_PhysicsTargets = new HashSet<IUpdatable>();
        private static WaitForFixedUpdate m_WaitForFixedUpdate = new WaitForFixedUpdate();

        #endregion

        #region Unity life cycle

        private void Start()
        {
            StartCoroutine(RoutineInvokedAfterUnityUpdate());
            StartCoroutine(RoutineInvokedAfterUnityFixedUpdate());
        }

        #endregion

        #region Public methods

        public void AddTarget(IUpdatable target, bool physics)
        {
            const string message = "Attempt to add the target that already has been added";
            Debug.Assert(m_Targets.Contains(target) || m_PhysicsTargets.Contains(target), message);
            
            ICollection<IUpdatable> targets = physics ? m_PhysicsTargets : m_Targets;
            targets.Add(target);
        }

        public void RemoveTarget(IUpdatable target)
        {
            m_Targets.Remove(target);
            m_PhysicsTargets.Remove(target);
        }

        #endregion

        #region Private methods

        private IEnumerator RoutineInvokedAfterUnityUpdate()
        {
            while (true)
            {
                yield return null;

                InvokeUpdates(m_Targets);
            }
        }

        private IEnumerator RoutineInvokedAfterUnityFixedUpdate()
        {
            while (true)
            {
                yield return m_WaitForFixedUpdate;

                InvokeUpdates(m_PhysicsTargets);
            }
        }

        #endregion

        #region Static private methods

        private static void InvokeUpdates(ICollection<IUpdatable> updatables)
        {
            foreach(IUpdatable target in updatables)
            {
                target.DoUpdate();
            }
        }

        #endregion
    }
}