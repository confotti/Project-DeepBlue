using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GroundObjectBlending
{

    /// <summary>
    /// Objects bounds
    /// </summary>
    public static class ObjectsBounds
    {

        /// <summary>
        /// Temporary MeshRenderer list
        /// </summary>
        static List<MeshRenderer> _meshRendererList = null;

        // ------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Temporary MeshRenderer list
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
        /// <param name="root">root</param>
        /// <param name="includeInactive">include inactive objects</param>
        /// <param name="min">destination min</param>
        /// <param name="max">destination max</param>
        // ------------------------------------------------------------------------------------------------------
        public static void CalcObjectsBounds(
            Transform root,
            bool includeInactive,
            out Vector3 min,
            out Vector3 max
            )
        {

            // init
            {
                min = Vector3.zero;
                max = Vector3.zero;
            }

            // check
            {

                if (!root)
                {
                    return;
                }

            }

            // --------------------------

            List<MeshRenderer> meshRendererList = meshRendererListInstance;

            // --------------------------

            // clear
            {
                meshRendererList.Clear();
            }

            // calc
            {

                // init
                {

                    float minmax = 100000.0f;

                    min = new Vector3(minmax, minmax, minmax);
                    max = new Vector3(-minmax, -minmax, -minmax);

                }

                // min, max
                {

                    root.GetComponentsInChildren<MeshRenderer>(includeInactive, meshRendererList);

                    Bounds bounds = new Bounds();

                    Vector3 boundMin = Vector3.zero;
                    Vector3 boundMax = Vector3.zero;

                    foreach (var val in meshRendererList)
                    {

                        bounds = val.bounds;

                        boundMin = bounds.min;
                        boundMax = bounds.max;

                        min.x = Mathf.Min(min.x, boundMin.x);
                        min.y = Mathf.Min(min.y, boundMin.y);
                        min.z = Mathf.Min(min.z, boundMin.z);

                        max.x = Mathf.Max(max.x, boundMax.x);
                        max.y = Mathf.Max(max.y, boundMax.y);
                        max.z = Mathf.Max(max.z, boundMax.z);

                    }

                }

            }

            // clear
            {
                meshRendererList.Clear();
            }

        }

    }

}