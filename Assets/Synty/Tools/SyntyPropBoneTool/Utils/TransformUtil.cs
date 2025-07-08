// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// For additional details, see the LICENSE.MD file bundled with this software.

using UnityEngine;

namespace Synty.Tools.SyntyPropBoneTool
{
    /// <summary>
    ///     Helper class containing helpful functions relating to Transform objects.
    /// </summary>
    public static class TransformUtil
    {
        /// <summary>
        ///     Performs a depth first recursive searche of the hierarchy to find a transform with the searchName.
        /// </summary>
        /// <param name="node">The current node in the recursive search.</param>
        /// <param name="searchName">The name of the node to find.</param>
        /// <returns>A <c>Transform</c> that is the first match to the searchName or null if no match is found.</returns>
        public static Transform SearchHierarchy(Transform node, string searchName)
        {
            if (node == null)
            {
                return null;
            }

            if (node.name == searchName)
            {
                return node;
            }

            Transform result = null;
            for (int childIndex = 0; childIndex < node.childCount; childIndex++)
            {
                result = SearchHierarchy(node.GetChild(childIndex), searchName);
                if (result != null)
                {
                    break;
                }
            }

            return result;
        }
    }
}
