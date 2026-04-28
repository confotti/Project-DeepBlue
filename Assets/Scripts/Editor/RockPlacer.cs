using UnityEngine;
using UnityEditor;

public class RockPlacer : EditorWindow
{
    public GameObject prefab;
    public LayerMask terrainLayer;
    public bool alignToNormal = true;

    [MenuItem("Tools/Rock Click Placer")]
    static void Init()
    {
        GetWindow<RockPlacer>("Rock Placer");
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnGUI()
    {
        prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);
        terrainLayer = EditorGUILayout.LayerField("Terrain Layer", terrainLayer);
        alignToNormal = EditorGUILayout.Toggle("Align To Surface", alignToNormal);

        EditorGUILayout.HelpBox("Click in Scene View to place rocks", MessageType.Info);
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (prefab == null) return;

        Event e = Event.current;

        // Prevent placing when navigating
        if (e.alt) return;

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 500f, 1 << terrainLayer))
        {
            // Draw preview marker
            Handles.color = Color.green;
            Handles.SphereHandleCap(0, hit.point, Quaternion.identity, 0.3f, EventType.Repaint);

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                PlaceRock(hit);
                e.Use();
            }
        }
    }

    void PlaceRock(RaycastHit hit)
    {
        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        Undo.RegisterCreatedObjectUndo(obj, "Place Rock");

        obj.transform.position = hit.point;

        if (alignToNormal)
        {
            obj.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        }

        // Random Y rotation (natural look)
        obj.transform.Rotate(Vector3.up, Random.Range(0, 360f), Space.Self);
    }
}