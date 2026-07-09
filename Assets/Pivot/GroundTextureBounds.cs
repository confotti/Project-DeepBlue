using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GroundObjectBlending
{

    /// <summary>
    /// Root GameObject to create ground textures
    /// </summary>
    public class GroundTextureBounds : MonoBehaviour
    {

#if UNITY_EDITOR

        /// <summary>
        /// Margin meter of Bounds
        /// </summary>
        [SerializeField]
        [Tooltip("Margin meter of Bounds")]
        Vector3 m_boundsMarginMeter = Vector3.one;

        /// <summary>
        /// Bounds min
        /// </summary>
        [SerializeField]
        [Tooltip("Bounds min")]
        Vector3 m_boundsMin = Vector3.zero;

        /// <summary>
        /// Bounds max
        /// </summary>
        [SerializeField]
        [Tooltip("Bounds max")]
        Vector3 m_boundsMax = Vector3.zero;

        /// <summary>
        /// SerializedObject
        /// </summary>
        UnityEditor.SerializedObject m_so = null;

        /// <summary>
        /// StringBuilder
        /// </summary>
        static StringBuilder _sb = null;

        /// <summary>
        /// MeshRenderer list
        /// </summary>
        static List<MeshRenderer> _meshRendererList = null;

        /// <summary>
        /// _BoundsMin
        /// </summary>
        static int _BoundsMin = Shader.PropertyToID("_BoundsMin");

        /// <summary>
        /// _BoundsMax
        /// </summary>
        static int _BoundsMax = Shader.PropertyToID("_BoundsMax");

        // ------------------------------------------------------------------------------------------------------

        /// <summary>
        /// SerializedObject
        /// </summary>
        UnityEditor.SerializedObject soInstance
        {

            get
            {

                if (this.m_so == null)
                {
                    this.m_so = new UnityEditor.SerializedObject(this);
                }

                return this.m_so;

            }

        }

        /// <summary>
        /// StringBuilder
        /// </summary>
        static StringBuilder sbInstance
        {

            get
            {

                if (_sb == null)
                {
                    _sb = new StringBuilder();
                }

                return _sb;

            }

        }

        /// <summary>
        /// MeshRenderer list
        /// </summary>
        static List<MeshRenderer> meshRendererListInstance
        {

            get
            {

                if (_meshRendererList == null)
                {
                    _meshRendererList = new List<MeshRenderer>();
                }

                return _meshRendererList;

            }

        }

        /// <summary>
        /// Claculate the bounds of objects
        /// </summary>
        /// <param name="includeInactive">include inactive objects</param>
        /// <param name="maxHalfDistance">max half distance</param>
        /// <param name="min">destination min</param>
        /// <param name="max">destination max</param>
        // ------------------------------------------------------------------------------------------------------
        public void CalcObjectsBounds(
            bool includeInactive,
            out float maxHalfDistance,
            out Vector3 boundsMin,
            out Vector3 boundsMax
            )
        {

            ObjectsBounds.CalcObjectsBounds(
                this.transform,
                includeInactive,
                out boundsMin,
                out boundsMax
                );

            boundsMin -= this.m_boundsMarginMeter;
            boundsMax += this.m_boundsMarginMeter;

            Vector3 boundsCenter = (boundsMin + boundsMax) * 0.5f;

            float distanceX = Mathf.Abs(boundsMax.x - boundsMin.x);
            float distanceZ = Mathf.Abs(boundsMax.z - boundsMin.z);

            maxHalfDistance = Mathf.Max(distanceX, distanceZ) * 0.5f;

            // boundsMin, boundsMax
            {

                boundsMax.x = boundsCenter.x + maxHalfDistance;
                boundsMax.z = boundsCenter.z + maxHalfDistance;

                boundsMin.x = boundsCenter.x - maxHalfDistance;
                boundsMin.z = boundsCenter.z - maxHalfDistance;

            }

            this.UpdateBoundsValue(boundsMin, boundsMax);

        }

        /// <summary>
        /// Update bounds value
        /// </summary>
        /// <param name="boundsMin">bounds min</param>
        /// <param name="boundsMax">bounds max</param>
        // ------------------------------------------------------------------------------------------------------
        void UpdateBoundsValue(
            Vector3 boundsMin,
            Vector3 boundsMax
            )
        {

            UnityEditor.SerializedObject so = this.soInstance;

            so.Update();

            so.FindProperty(nameof(this.m_boundsMin)).vector3Value = boundsMin;
            so.FindProperty(nameof(this.m_boundsMax)).vector3Value = boundsMax;

            so.ApplyModifiedProperties();

        }

        /// <summary>
        /// Get texture file name without extension
        /// </summary>
        /// <returns>name</returns>
        // ------------------------------------------------------------------------------------------------------
        public string GetBaseTextureFileNameWithoutExtension()
        {

            string ret = "";

            StringBuilder sb = sbInstance;

            // clear
            {
                sb.Clear();
            }

            // calc
            {

                sb.Append(this.name);

                Transform parent = this.transform.parent;

                while (parent)
                {
                    sb.Insert(0, string.Format("{0}-", parent.name));
                    parent = parent.parent;
                }

            }

            // ret
            {
                ret = sb.ToString();
            }

            // clear
            {
                sb.Clear();
            }

            return ret;

        }

        /// <summary>
        /// Update material values
        /// </summary>
        /// <param name="addToUndo">true to add to Undo</param>
        // ------------------------------------------------------------------------------------------------------
        public void UpdateMaterialValues(bool addToUndo)
        {

            List<MeshRenderer> list = meshRendererListInstance;

            // list
            {
                this.GetComponentsInChildren<MeshRenderer>(true, list);
            }

            // Undo
            {

                if (addToUndo)
                {
                    UnityEditor.Undo.IncrementCurrentGroup();
                    UnityEditor.Undo.SetCurrentGroupName("Undo Modify Material Values");
                }

            }

            // ---------------------

            int undoGroupIndex = UnityEditor.Undo.GetCurrentGroup();

            // ---------------------

            // update
            {

                Material mat = null;

                string undoName = "Material Update";

                foreach (var val in list)
                {

                    mat = val.sharedMaterial;

                    if (mat)
                    {

                        // Undo
                        {
                            if (addToUndo)
                            {
                                UnityEditor.Undo.RecordObject(mat, undoName);
                            }
                        }

                        // update
                        {

                            if (mat.HasVector(_BoundsMin))
                            {
                                mat.SetVector(_BoundsMin, this.m_boundsMin);
                            }

                            if (mat.HasVector(_BoundsMax))
                            {
                                mat.SetVector(_BoundsMax, this.m_boundsMax);
                            }

                        }

                    }

                }

            }

            // Undo
            {
                if (addToUndo)
                {
                    UnityEditor.Undo.CollapseUndoOperations(undoGroupIndex);
                }
            }

        }

#endif

    }

}