#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using System.Reflection;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using static VRuler.Libs.VUtils;
using static VRuler.Libs.VGUI;


namespace VRuler
{
    static class VRuler
    {
        static void OnSceneGUI(SceneView sceneView)
        {
            void ruler()
            {
                if (!VRulerMenuItems.rulerEnabled) return;

                void reset()
                {
                    if (!shortcutPressed || boundsObjects.Any() || iObjectForScale != -1)
                        rulerStartSet = rulerEndSet = rulerStartSetFailed = false;

                }
                void setStart()
                {
                    if (!shortcutPressed) return;
                    if (eType != EventType.MouseMove) return;
                    if (rulerStartSet) return;
                    if (rulerStartSetFailed) return;
                    if (boundsObjects.Any()) return;


                    if (RaycastScene(e.mousePosition - e.delta, ref rulerStart, out _))
                        rulerStartSet = true;

                    else if (RaycastXZPlane(e.mousePosition - e.delta, ref rulerStart))
                        rulerStartSet = true;


                    rulerStartSetFailed = !rulerStartSet;

                }
                void setEnd()
                {
                    if (eType != EventType.MouseMove) return;
                    if (!rulerStartSet) return;


                    if (RaycastScene(e.mousePosition, ref rulerEnd, out GameObject go))
                        rulerEndSet = true;

                    else if (RaycastXZPlane(e.mousePosition, ref rulerEnd))
                        rulerEndSet = true;

                    else
                        rulerEndSet = false;


                }
                void draw()
                {
                    if (!rulerStartSet || !rulerEndSet) return;

                    DrawRuler(rulerStart, rulerEnd, true);

                }

                reset();
                setStart();
                setEnd();
                draw();

            }
            void bounds_()
            {
                if (!VRulerMenuItems.boundsEnabled) return;

                Bounds bounds = default;
                Matrix4x4 matrix = default;

                void reset()
                {
                    if (!shortcutPressed || iObjectForScale != -1)
                        boundsObjects.Clear();

                }
                void set()
                {
                    if (!shortcutPressed) return;

                    if (e.type == EventType.Layout)
                        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

                    if (eType != EventType.MouseDown) return;
                    // e.Use();

                    var pos = Vector3.zero;
                    if (!RaycastScene(e.mousePosition, ref pos, out GameObject go)) return;

                    if (boundsObjects.Contains(go)) return;

                    boundsObjects.Add(go);

                    rulerStartSet = rulerEndSet = rulerStartSetFailed = false;

                }
                void calc()
                {
                    if (!boundsObjects.Any()) return;

                    void single()
                    {
                        if (boundsObjects.Count != 1) return;

                        matrix = boundsObjects.First().transform.localToWorldMatrix;
                        bounds = GetBounds(boundsObjects.First(), true);
                    }
                    void multiple()
                    {
                        if (boundsObjects.Count == 1) return;

                        matrix = Matrix4x4.identity;

                        bounds = default;

                        foreach (var r in boundsObjects)
                            if (bounds == default) bounds = GetBounds(r, false);
                            else bounds.Encapsulate(GetBounds(r, false));

                    }

                    single();
                    multiple();

                }
                void draw()
                {
                    if (!boundsObjects.Any()) return;

                    var cornersLocal = new List<Vector3>();
                    cornersLocal.Add(bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, bounds.extents.z));
                    cornersLocal.Add(bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, bounds.extents.z));
                    cornersLocal.Add(bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, -bounds.extents.z));
                    cornersLocal.Add(bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, -bounds.extents.z));

                    var cornersWorld = cornersLocal.Select(r => matrix.MultiplyPoint(r));

                    var cornersSorted = cornersWorld.OrderBy(r => (sceneView.camera.transform.position - r).sqrMagnitude).ToList();

                    var origin = cornersSorted[0];
                    var p1 = cornersSorted[1];
                    var p2 = cornersSorted[2];
                    var p3 = origin + matrix.MultiplyVector(Vector3.up) * bounds.size.y;


                    var pLabelViewpor1 = sceneView.camera.WorldToViewportPoint((origin + p1) / 2);
                    var pLabelViewpor2 = sceneView.camera.WorldToViewportPoint((origin + p2) / 2);
                    var pLabelViewpor3 = sceneView.camera.WorldToViewportPoint((origin + p3) / 2);

                    bool visible(Vector3 pViewport) => pViewport.x == pViewport.x.Clamp01() && pViewport.y == pViewport.y.Clamp01() && pViewport.z > 0;
                    if (!visible(pLabelViewpor1) || !visible(pLabelViewpor2) || !visible(pLabelViewpor3))
                    {
                        origin = cornersSorted[3];
                        p1 = cornersSorted[2];
                        p2 = cornersSorted[1];
                        p3 = origin + matrix.MultiplyVector(Vector3.up) * bounds.size.y;
                    }


                    if (origin != p1) DrawRuler(origin, p1, false);
                    if (origin != p2) DrawRuler(origin, p2, false);
                    if (origin != p3) DrawRuler(origin, p3, false);

                }


                reset();
                set();
                calc();
                draw();

            }
            void objectsForScale_()
            {
                if (!VRulerMenuItems.objectsForScaleEnabled) return;

                void reset()
                {
                    if (shortcutPressed) return;

                    iObjectForScale = -1;
                    objectForScalePosSet = false;
                    scroll = 0;

                }
                void setIndex()
                {
                    if (!shortcutPressed) { scroll = 0; return; }
                    if (eType != EventType.ScrollWheel) return;

                    e.Use();

                    LoadObjectsForScale();

                    var delta = Application.platform == RuntimePlatform.OSXEditor ? e.delta.x + e.delta.y
                                                                                  : e.delta.x - e.delta.y;

                    if (delta < 0 && iObjectForScale <= -1) return;
                    if (delta > 0 && iObjectForScale >= objectsForScaleMeshes.Count - 1) return;

                    scroll += delta;

                    if (scroll > scrollThreshold) { scroll = 0; iObjectForScale++; }
                    if (scroll < -scrollThreshold) { scroll = 0; if (iObjectForScale > 0) iObjectForScale--; }

                }
                void setPos()
                {
                    if (iObjectForScale == -1) return;
                    if (e.delta == Vector2.zero) return;

                    if (RaycastScene(e.mousePosition, ref objectForScalePos, out _))
                        objectForScalePosSet = true;

                    else if (RaycastXZPlane(e.mousePosition, ref objectForScalePos))
                        objectForScalePosSet = true;

                    else
                        objectForScalePosSet = false;


                }
                void draw()
                {
                    if (iObjectForScale == -1) return;
                    if (!objectForScalePosSet) return;

                    Graphics.DrawMesh(objectsForScaleMeshes[iObjectForScale], objectForScalePos, Quaternion.identity, objectsForScaleMaterial, 0, sceneView.camera);
                }

                reset();
                setIndex();
                setPos();
                draw();

            }

            ruler();
            bounds_();
            objectsForScale_();

            if (shortcutPressed)
                sceneView.Repaint();

        }

        static Vector3 rulerStart;
        static Vector3 rulerEnd;
        static bool rulerStartSet;
        static bool rulerStartSetFailed;
        static bool rulerEndSet;
        static List<GameObject> boundsObjects = new List<GameObject>();
        static int iObjectForScale = -1;
        static float scroll;
        static float scrollThreshold => Application.platform == RuntimePlatform.OSXEditor ? .5f : .1f;
        static Vector3 objectForScalePos;
        static bool objectForScalePosSet;
        static bool shortcutPressed => holdingShift && holdingR;



        static void DrawRuler(Vector3 start, Vector3 end, bool drawStartPointer)
        {
            var lineColor = Greyscale(.8f);
            var outlineColor = Greyscale(.1f);
            var lineOccludedColor = Greyscale(.5f);
            var labelBackgroundColor = Greyscale(.3f);

            int pass;

            void drawLine(Vector3 from, Vector3 to)
            {
                if (pass == 0) // ouline
                {
                    Handles.zTest = CompareFunction.LessEqual;
                    Handles.color = outlineColor;
                    Handles.DrawLine(from, to, 2.5f);
                }
                if (pass == 1) // line
                {
                    Handles.zTest = CompareFunction.LessEqual;
                    Handles.color = lineColor;
                    Handles.DrawLine(from, to, 1.5f);
                }
                if (pass == 2) // occluded
                {
                    Handles.zTest = CompareFunction.Greater;
                    Handles.color = lineOccludedColor;
                    Handles.DrawLine(from, to, 1.5f);
                }

                Handles.zTest = CompareFunction.Always;
                Handles.color = Color.white;

            }
            void drawPointer(Vector3 pos, Vector3 dir, float size)
            {
                var skew = 1f;

                dir = dir.normalized;
                var sideDir = Vector3.Cross(dir, pos - SceneView.currentDrawingSceneView.camera.transform.position).normalized;
                var p1 = pos - dir * size + sideDir * size * skew;
                var p2 = pos - dir * size - sideDir * size * skew;

                drawLine(pos, p1);
                drawLine(pos, p2);
            }
            void drawLabel(Vector3 pos, float distance)
            {
                void setupStyle()
                {
                    if (labelStyle != null && labelStyle.normal.background != null) return;

                    labelStyle = new GUIStyle();
                    labelStyle.normal.textColor = Color.white;
                    labelStyle.alignment = TextAnchor.MiddleCenter;

                    var res = 33;
                    var cornerSize = 6;
                    var tex = new Texture2D(res, res);

                    for (int x = 0; x < res; x++)
                        for (int y = 0; y < res; y++)
                        {
                            var xCorner = (x - (res - 1) / 2).Abs() - (res / 2 - cornerSize);
                            var yCorner = (y - (res - 1) / 2).Abs() - (res / 2 - cornerSize);

                            var distToBorder = 0f;

                            if (xCorner > 0 && yCorner > 0)
                                distToBorder = cornerSize - new Vector2(xCorner, yCorner).magnitude;
                            else
                                distToBorder = Mathf.Min(x, y, res - 1 - x, res - 1 - y);

                            if (distToBorder < 0)
                                tex.SetPixel(x, y, Greyscale(lineColor.r, 0));
                            else if (distToBorder < 1)
                                tex.SetPixel(x, y, Greyscale(lineColor.r, 1));
                            else
                                tex.SetPixel(x, y, labelBackgroundColor);

                        }

                    tex.Apply();

                    labelStyle.normal.background = tex;
                    labelStyle.border = new RectOffset(cornerSize, cornerSize, cornerSize, cornerSize);
#if UNITY_2021_2_OR_NEWER
                    labelStyle.padding = new RectOffset(1, 1, 1, 1);
#endif

                }
                void drawMetric()
                {
                    if (VRulerMenuItems.imperialSystemEnabled) return;

                    var format = "0.";
                    int decimalPoints = distance >= 1 ? VRulerMenuItems.decimalPoints : VRulerMenuItems.decimalPoints + 1;
                    for (int i = 0; i < decimalPoints; i++)
                        format += "0";

#if UNITY_2021_2_OR_NEWER
                    var text = " " + distance.ToString(format) + " m ";
#else
                    var text = distance.ToString(format) + " m";
#endif

                    Handles.Label(pos, text, labelStyle);

                }
                void drawImperial()
                {
                    if (!VRulerMenuItems.imperialSystemEnabled) return;

                    var feet = (distance * 3.28f).FloorToInt();
                    var inches = ((distance * 3.28f - feet) * 12).FloorToInt();

                    var text = " " + feet + "' " + inches + "'' ";

                    Handles.Label(pos, text, labelStyle);

                }


                setupStyle();
                drawMetric();
                drawImperial();

            }


            var pointerSize = HandleUtility.GetHandleSize(start) * .08f;

            pass = 0;
            drawLine(start, end);
            drawPointer(end, end - start, pointerSize);
            if (drawStartPointer)
                drawPointer(start, start - end, pointerSize);

            pass = 1;
            drawLine(start, end);
            drawPointer(end, end - start, pointerSize);
            if (drawStartPointer)
                drawPointer(start, start - end, pointerSize);

            pass = 2;
            drawLine(start, end);
            drawPointer(end, end - start, pointerSize);
            if (drawStartPointer)
                drawPointer(start, start - end, pointerSize);

            drawLabel((start + end) / 2, (start - end).magnitude);

        }

        static GUIStyle labelStyle;



        static void LoadObjectsForScale()
        {
            void meshes()
            {
                if (objectsForScaleMeshes.Any()) return;

                var folderPath = EditorPrefs.GetString("vRuler-objectsForScalePath", "Assets/vTools/vRuler/Objects for scale");

                if (!AssetDatabase.IsValidFolder(folderPath))
                    EditorPrefs.SetString("vRuler-objectsForScalePath", folderPath = GetScriptPath("VRuler").ParentPath().CombinePath("Objects for scale"));

                if (!AssetDatabase.IsValidFolder(folderPath)) return;

                var files = System.IO.Directory.GetFiles(folderPath, "*", System.IO.SearchOption.TopDirectoryOnly);
                var gos = files.Select(r => AssetDatabase.LoadAssetAtPath<GameObject>(r)).Where(r => r).OrderBy(r => GetBounds(r, true).extents.x);
                objectsForScaleMeshes = gos.Select(r => r.GetComponentInChildren<MeshFilter>()?.sharedMesh).Where(r => r).ToList();

            }
            void material()
            {
                if (objectsForScaleMaterial != null) return;

                objectsForScaleMaterial = new Material(GraphicsSettings.defaultRenderPipeline == null ? Shader.Find("Standard") : UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset.defaultShader);

            }

            meshes();
            material();

        }

        static List<Mesh> objectsForScaleMeshes = new List<Mesh>();
        static Material objectsForScaleMaterial;




        static bool RaycastScene(Vector2 mousePos, ref Vector3 pos, out GameObject go)
        {
            go = HandleUtility.PickGameObject(mousePos, false);

            if (!go) return false;

            var ray = GetRay(mousePos);


            if (go.GetComponent<MeshFilter>() != null)
            {
                var prms = new object[] { ray, go.GetComponent<MeshFilter>().sharedMesh, go.transform.localToWorldMatrix, null };
                var hit = (bool)mi_IntersectRayMesh.Invoke(null, prms);

                if (!hit) return false;

                var raycastHit = (RaycastHit)prms[3];
                pos = ray.GetPoint(raycastHit.distance);
                return true;
            }


            if (go.GetComponent<TerrainCollider>() != null)
                if (go.GetComponent<TerrainCollider>().Raycast(ray, out RaycastHit hit, Mathf.Infinity))
                {
                    pos = hit.point;
                    return true;
                }


            return false;

        }

        static MethodInfo mi_IntersectRayMesh = typeof(Editor).Assembly.GetType("UnityEditor.HandleUtility").GetMethod("IntersectRayMesh", maxBindingFlags);


        static bool RaycastXZPlane(Vector2 mousePos, ref Vector3 pos)
        {
            var ray = GetRay(mousePos);

            var dist = 0f;
            var hit = new Plane(Vector3.up, Vector3.zero).Raycast(ray, out dist);

            if (hit)
                pos = ray.GetPoint(dist);

            return hit;

        }


        static Ray GetRay(Vector2 mousePos) => SceneView.currentDrawingSceneView.camera.ScreenPointToRay(new Vector2(e.mousePosition.x * EditorGUIUtility.pixelsPerPoint, SceneView.currentDrawingSceneView.camera.pixelHeight - e.mousePosition.y * EditorGUIUtility.pixelsPerPoint));


        public static Bounds GetBounds(GameObject go, bool local)
        {
            Bounds bounds = default;

            foreach (var r in go.GetComponentsInChildren<MeshRenderer>())
            {
                var b = local ? r.gameObject.GetComponent<MeshFilter>().sharedMesh.bounds : r.bounds;

                if (bounds == default)
                    bounds = b;
                else
                    bounds.Encapsulate(b);
            }

            foreach (var r in go.GetComponentsInChildren<Terrain>())
            {
                var b = local ? new Bounds(r.terrainData.size / 2, r.terrainData.size) : new Bounds(r.transform.position + r.terrainData.size / 2, r.terrainData.size);

                if (bounds == default)
                    bounds = b;
                else
                    bounds.Encapsulate(new Bounds(r.transform.position + r.terrainData.size / 2, r.terrainData.size));

            }

            return bounds;
        }



        static void OnKeyDown()
        {
            if (e.keyCode != KeyCode.R) return;

            if (eType == EventType.KeyDown)
            {
                holdingR = true;

                if (holdingShift)
                    e.Use();
            }

            if (eType == EventType.KeyUp)
                holdingR = false;


        }
        static bool holdingR;



#if !DISABLED
        [InitializeOnLoadMethod]
#endif
        static void Init()
        {
            SceneView.duringSceneGui += OnSceneGUI;

            var globalEventHandler = typeof(EditorApplication).GetField("globalEventHandler", (BindingFlags)62);
            globalEventHandler.SetValue(null, (EditorApplication.CallbackFunction)globalEventHandler.GetValue(null) + OnKeyDown);

        }



        const string version = "1.0.5";

    }
}
#endif
