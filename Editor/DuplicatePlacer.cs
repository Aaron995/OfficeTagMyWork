using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EditorTools
{
    public class DuplicatePlacer : EditorWindow
    {
        private GameObject m_ObjectToDuplicate;
        private Vector3 m_Offset = Vector3.zero;
        private int m_Amount = 0;

        [MenuItem("Tools/DuplicatePlace")]
        private static void ShowWindow()
        {
            GetWindow(typeof(DuplicatePlacer));
        }

        private void OnGUI()
        {
            m_ObjectToDuplicate = EditorGUILayout.ObjectField("Item To Place", m_ObjectToDuplicate, typeof(GameObject), true) as GameObject;
            m_Offset = EditorGUILayout.Vector3Field("Placing Offset", m_Offset);
            m_Amount = EditorGUILayout.IntField("Amount placed", m_Amount);

            if (GUILayout.Button("Place!") && m_ObjectToDuplicate != null)
            {
                for (int i = 0; i < m_Amount; i++)
                {
                    GameObject newObject = Instantiate(m_ObjectToDuplicate);
                    newObject.transform.position = newObject.transform.position += m_Offset;
                    m_ObjectToDuplicate = newObject;
                }
            }
        }
    }

}