using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
//using UnityEditor.Formats.Fbx.Exporter;

using IceEngine;
using IceEditor.Framework;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;

namespace IceEditor
{
    public class ModelUVTextureEditorWindow : IceEditorWindow
    {
        #region 定制
        [MenuItem("IceEngine/贴图裁剪UV重排工具")]
        public static void OpenWindow() => GetWindow<ModelUVTextureEditorWindow>();
        protected override string Title => "贴图裁剪UV重排";
        protected override Color DefaultThemeColor => new(0.38f, 0.85f, 0.85f);
        protected override void OnThemeColorChange()
        {
            _stlItemOn = null;
            _stlItemOnHover = null;
            _stlProgressBar = null;
        }
        // 用到的样式
        GUIStyle StlDockField => _stlDockField?.Check() ?? (_stlDockField = new GUIStyle("dockarea") { alignment = TextAnchor.MiddleLeft, contentOffset = new Vector2(0f, -1f), }); GUIStyle _stlDockField;
        GUIStyle StlItem => _stlItem?.Check() ?? (_stlItem = new GUIStyle("flow node 0") { alignment = TextAnchor.MiddleLeft, stretchWidth = true, margin = new RectOffset(2, 2, 4, 4), padding = new RectOffset(4, 4, 1, 1), contentOffset = new Vector2(0f, 0f), }.Initialize(stl => stl.hover.background = StlItemHover.normal.background)); GUIStyle _stlItem;
        GUIStyle StlItemHover => _stlItemHover?.Check() ?? (_stlItemHover = new GUIStyle("flow node 0 on") { alignment = TextAnchor.MiddleLeft, stretchWidth = true, margin = new RectOffset(2, 2, 4, 4), padding = new RectOffset(4, 4, 1, 1), contentOffset = new Vector2(0f, 0f), }); GUIStyle _stlItemHover;
        GUIStyle StlItemOn => _stlItemOn?.Check() ?? (_stlItemOn = new GUIStyle($"flow node {IceGUIUtility.GetThemeColorHueIndex(ThemeColor)}") { alignment = TextAnchor.MiddleLeft, stretchWidth = true, margin = new RectOffset(2, 2, 4, 4), padding = new RectOffset(4, 4, 1, 1), contentOffset = new Vector2(0f, 0f), }.Initialize(stl => stl.hover.background = StlItemOnHover.normal.background)); GUIStyle _stlItemOn;
        GUIStyle StlItemOnHover => _stlItemOnHover?.Check() ?? (_stlItemOnHover = new GUIStyle($"flow node {IceGUIUtility.GetThemeColorHueIndex(ThemeColor)} on") { alignment = TextAnchor.MiddleLeft, stretchWidth = true, margin = new RectOffset(2, 2, 4, 4), padding = new RectOffset(4, 4, 1, 1), contentOffset = new Vector2(0f, 0f), }); GUIStyle _stlItemOnHover;
        GUIStyle StlHierarchyHeader => _stlHierarchyHeader?.Check() ?? (_stlHierarchyHeader = new GUIStyle("AnimClipToolbarButton") { fixedHeight = 0f, }); GUIStyle _stlHierarchyHeader;
        GUIStyle StlPackItem => _stlPackItem?.Check() ?? (_stlPackItem = new GUIStyle("ChannelStripEffectBar") { margin = new RectOffset(0, 0, 0, 4), padding = new RectOffset(0, 0, 4, 4), fixedHeight = 0f, }); GUIStyle _stlPackItem;
        GUIStyle StlPackItemOn => _stlPackItemOn?.Check() ?? (_stlPackItemOn = new GUIStyle("ChannelStripAttenuationBar") { margin = new RectOffset(0, 0, 0, 4), padding = new RectOffset(0, 0, 4, 4), fixedHeight = 0f, }); GUIStyle _stlPackItemOn;
        GUIStyle StlPackName => _stlPackName?.Check() ?? (_stlPackName = new GUIStyle("AnimationEventTooltip") { border = new RectOffset(6, 6, 6, 6), padding = new RectOffset(4, 4, 0, 0), overflow = new RectOffset(0, 0, 2, 2), fontSize = 14, alignment = TextAnchor.MiddleLeft, contentOffset = new Vector2(0f, 0f), }); GUIStyle _stlPackName;
        GUIStyle StlOutputTextureBox => _stlOutputTextureBox?.Check() ?? (_stlOutputTextureBox = new GUIStyle("MeBlendBackground") { fixedHeight = 0f, }); GUIStyle _stlOutputTextureBox;
        GUIStyle StlRotateHandle => _stlRotateHandle?.Check() ?? (_stlRotateHandle = new GUIStyle("label") { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(0, 0, 0, 0), fontSize = 14, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, contentOffset = new Vector2(1f, -1f), }.Initialize(stl => { stl.normal.textColor = new Color(1f, 1f, 1f); })); GUIStyle _stlRotateHandle;
        GUIStyle StlProgressBar => _stlProgressBar?.Check() ?? (_stlProgressBar = new GUIStyle($"flow node {IceGUIUtility.GetThemeColorHueIndex(ThemeColor)}") { padding = new RectOffset(3, 0, 0, 0), fontSize = 11, alignment = TextAnchor.MiddleLeft, clipping = TextClipping.Clip, contentOffset = new Vector2(0f, 0f), }.Initialize(stl => { stl.normal.textColor = new Color(0f, 0f, 0f); })); GUIStyle _stlProgressBar;
        #endregion

        #region Struct Definition
        [Serializable] public struct Line { public Vector2 p1; public Vector2 p2; }

        [Serializable]
        public class MeshUnit
        {
            public Mesh mesh;
            public Rect bound;
            public List<int> triangleList;
            public List<Line> lineList = new();  // 线框
            public List<Line> edgeList = new();  // 边缘

            public MeshUnit(Mesh mesh, List<int> triangleList)
            {
                this.mesh = mesh;
                this.triangleList = triangleList;
            }
        }
        [Serializable]
        public class MeshResource
        {
            public Mesh mesh;
            public List<MeshUnit> meshUnits = new();
            public MeshResource(Mesh mesh, Action<string, float> progressCallback = null)
            {
                this.mesh = mesh;
                var triangles = mesh.triangles;
                var uv = mesh.uv;

                var unitMap = new Dictionary<Vector2, List<int>>();
                Vector2 Compress(Vector2 p)
                {
                    const float k = 512;
                    return new Vector2(Mathf.Round(p.x * k) / k, Mathf.Round(p.y * k) / k);
                }
                for (int i = 3; i <= triangles.Length; i += 3)
                {
                    progressCallback?.Invoke($"正在读取Mesh信息...{i / 3}/{triangles.Length / 3}", ((float)i) / mesh.triangles.Length);

                    Vector2 p1 = Compress(uv[triangles[i - 1]]);
                    Vector2 p2 = Compress(uv[triangles[i - 2]]);
                    Vector2 p3 = Compress(uv[triangles[i - 3]]);

                    // 生成UnitMap
                    var newUnit = new List<int>() { i - 3 };

                    void MergeToP(Vector2 p)
                    {
                        if (unitMap.TryGetValue(p, out var unit))
                        {
                            if (unit == newUnit) return;

                            // Merge
                            newUnit.AddRange(unit);
                            foreach (var tri in unit)
                            {
                                unitMap[Compress(uv[triangles[tri + 0]])] = newUnit;
                                unitMap[Compress(uv[triangles[tri + 1]])] = newUnit;
                                unitMap[Compress(uv[triangles[tri + 2]])] = newUnit;
                            }
                        }
                        else
                        {
                            unitMap[p] = newUnit;
                        }
                    }
                    MergeToP(p1);
                    MergeToP(p2);
                    MergeToP(p3);
                }

                // 收集UnitSet
                var unitSet = new HashSet<List<int>>();
                foreach (var unit in unitMap) unitSet.Add(unit.Value);

                // 初始化Unit
                foreach (var unit in unitSet)
                {
                    var u = new MeshUnit(mesh, unit);

                    // 预处理点
                    HashSet<(Vector2 p1, Vector2 p2)> rawLineSet = new();
                    foreach (var i in unit)
                    {
                        Vector2 p1 = Compress(uv[triangles[i + 0]]);
                        Vector2 p2 = Compress(uv[triangles[i + 1]]);
                        Vector2 p3 = Compress(uv[triangles[i + 2]]);

                        // 确保方向一致
                        {
                            Vector2 v1 = p2 - p1;
                            Vector2 v2 = p3 - p1;
                            if (v1.x * v2.y - v2.x * v1.y > 0) (p3, p2) = (p2, p3);
                        }

                        // 处理点
                        {
                            rawLineSet.Add((p1, p2));
                            rawLineSet.Add((p2, p3));
                            rawLineSet.Add((p3, p1));
                        }
                    }

                    // 探测线框,边缘,包围盒
                    HashSet<(Vector2, Vector2)> lineSet = new();
                    HashSet<(Vector2, Vector2)> edgeSet = new();
                    float xMin = 1;
                    float xMax = 0;
                    float yMin = 1;
                    float yMax = 0;
                    foreach (var p in rawLineSet)
                    {
                        var l = (p.p1.x < p.p2.x || (p.p1.x == p.p2.x && p.p1.y < p.p2.y)) ? p : (p.p2, p.p1);
                        // 线框
                        lineSet.Add(l);
                        // 边缘
                        if (!edgeSet.Remove(l)) edgeSet.Add(l);

                        xMin = Mathf.Min(xMin, l.Item1.x, l.Item2.x);
                        xMax = Mathf.Max(xMax, l.Item1.x, l.Item2.x);
                        yMin = Mathf.Min(yMin, l.Item1.y, l.Item2.y);
                        yMax = Mathf.Max(yMax, l.Item1.y, l.Item2.y);
                    }
                    u.lineList = lineSet.Select(l => new Line { p1 = l.Item1, p2 = l.Item2 }).ToList();
                    u.edgeList = edgeSet.Select(l => new Line { p1 = l.Item1, p2 = l.Item2 }).ToList();

                    u.bound.xMin = xMin;
                    u.bound.xMax = xMax;
                    u.bound.yMin = yMin;
                    u.bound.yMax = yMax;

                    meshUnits.Add(u);
                }
            }
        }

        [Serializable]
        public class MeshPack
        {
            public string name;
            public IceDictionary<string, MeshResource> meshResMap = new();
            public List<Texture2D> meshTextures = new();
            public List<MeshGroup> meshGroups = new();
            public List<MeshGroup> deletedGroups = new();

            public int outputTextureIndex = 0;
            public Texture2D OutputTexture => (outputTextureIndex >= 0 && outputTextureIndex < meshTextures.Count) ? meshTextures[outputTextureIndex] : null;
            public int OutputTextureWidth => OutputTexture != null ? OutputTexture.width : 1024;

            public MeshPack(string name)
            {
                this.name = name;
            }
        }
        public enum MeshOrientation
        {
            R0 = 0,
            R90 = 1,
            R180 = 2,
            R270 = 3,
        }

        [Serializable]
        public class MeshGroup
        {
            public string name;
            public Color tintColor;
            public List<MeshUnit> meshUnits = new();

            Mesh _mesh;
            public Mesh Mesh => _mesh != null ? _mesh : UpdateMesh();
            public Rect Bound => inMainWorkspace ? BoundTransformed : _bound;
            public Rect RawBound => _bound;


            [SerializeField] Rect _bound;
            [SerializeField] bool _dirty = false;
            [SerializeField] Vector2 _offset = Vector2.zero;
            [SerializeField] MeshOrientation _orientation = MeshOrientation.R0;
            [SerializeField] float _scale = 1;
            public Vector2 Position
            {
                get => BoundTransformed.min;
                set
                {
                    OffsetInversed += value - Position;
                }
            }
            public Vector2 PositionInversed
            {
                get => new Vector2(BoundTransformed.xMin, 1 - BoundTransformed.yMax);
                set
                {
                    Offset += value - PositionInversed;
                }
            }
            public Vector2 Size => BoundTransformed.size;
            public Vector2 Offset
            {
                get => _offset;
                set
                {
                    if (_offset != value)
                    {
                        _boundTranformed = null;
                        _dirty = true;
                        _offset = value;
                    }
                }
            }
            public Vector2 OffsetInversed
            {
                get => new Vector2(_offset.x, -_offset.y);
                set => Offset = new Vector2(value.x, -value.y);
            }
            public MeshOrientation Orientation
            {
                get => _orientation;
                set
                {
                    if (_orientation != value)
                    {
                        _boundTranformed = null;
                        _dirty = true;
                        _orientation = value;
                    }
                }
            }
            public float Scale
            {
                get => _scale;
                set
                {
                    if (_scale != value)
                    {
                        _boundTranformed = null;
                        _dirty = true;
                        _scale = value;
                    }
                }
            }

            [NonSerialized] public Vector2 offsetCache;
            [NonSerialized] public float scaleCache;
            [NonSerialized] public MeshOrientation orientationCache;

            public MeshGroup(string name, IEnumerable<MeshUnit> meshUnits, MeshGroup parentSource = null)
            {
                this.name = name;
                this.meshUnits.AddRange(meshUnits);

                _bound = MergeBounds(meshUnits.Select(u => u.bound).ToArray());
                tintColor = UnityEngine.Random.ColorHSV(0, 1, 0.5f, 0.8f, 0.7f, 0.9f);

                if (parentSource != null)
                {
                    // 拆分时在这里做变换
                    Scale = parentSource.Scale;
                    Orientation = parentSource.Orientation;
                    var vec = RawBound.center - parentSource.RawBound.center;
                    var vecT = vec * Scale;
                    vecT = Orientation switch
                    {
                        MeshOrientation.R90 => new Vector2(vecT.y, -vecT.x),
                        MeshOrientation.R180 => -vecT,
                        MeshOrientation.R270 => new Vector2(-vecT.y, vecT.x),
                        _ => vecT,
                    };
                    OffsetInversed = -vec + vecT + parentSource.OffsetInversed;
                }
            }
            public Mesh UpdateMesh()
            {
                return _mesh = MergeMesh(meshUnits);
            }
            public void UpdateUV(bool force = false, float factor = 1)
            {
                if (!_dirty && !force) return;
                var uv = Mesh.uv;
                var uv2 = Mesh.uv2;
                var uv3 = Mesh.uv3;

                var scaleUV = factor * Scale * Vector2.one;
                for (int i = 0; i < uv2.Length; ++i)
                {
                    // 在这里对uv做变换
                    uv2[i] = TranformPoint(uv[i], true);
                    uv3[i] = scaleUV;
                }
                Mesh.uv2 = uv2;
                Mesh.uv3 = uv3;

                _dirty = false;
            }
            public void UpdateBound() => _bound = MergeBounds(meshUnits.Select(u => u.bound).ToArray());
            public Vector2 TranformPoint(Vector2 p, bool force = false)
            {
                if (!force && !inMainWorkspace) return p;
                // 在这里对点做变换

                // 锚点
                var center = _bound.center;
                Vector2 off = p - center;
                // 缩放
                off *= Scale;
                // 旋转
                switch (Orientation)
                {
                    case MeshOrientation.R0:
                        // 不旋转
                        break;
                    case MeshOrientation.R90:
                        off = new Vector2(off.y, -off.x);
                        break;
                    case MeshOrientation.R180:
                        off = new Vector2(-off.x, -off.y);
                        break;
                    case MeshOrientation.R270:
                        off = new Vector2(-off.y, off.x);
                        break;
                }
                // 移动
                off += OffsetInversed;

                return center + off;
            }
            [NonSerialized] Rect? _boundTranformed = null;
            Rect BoundTransformed
            {
                get
                {
                    if (_boundTranformed is null)
                    {
                        // 在这里对bound做变换
                        Rect r = _bound;
                        var center = _bound.center;
                        var size = _bound.size;
                        // 缩放
                        size *= Scale;
                        // 旋转
                        if (Orientation == MeshOrientation.R90 || Orientation == MeshOrientation.R270)
                        {
                            size = new Vector2(size.y, size.x);
                        }
                        // 移动
                        center += OffsetInversed;

                        r.size = size;
                        r.center = center;

                        _boundTranformed = r;

                    }

                    return _boundTranformed.Value;
                }
            }

            public void DrawLines(Rect tRect)
            {
                foreach (var u in meshUnits) foreach (var l in u.lineList) Handles.DrawLine(tRect.Sample(TranformPoint(l.p1), true), tRect.Sample(TranformPoint(l.p2), true));
            }
            public void DrawEdges(Rect tRect)
            {
                foreach (var u in meshUnits) foreach (var l in u.edgeList) Handles.DrawLine(tRect.Sample(TranformPoint(l.p1), true), tRect.Sample(TranformPoint(l.p2), true));
            }
            public bool Contains(Vector2 p)
            {
                if (!Bound.Contains(p)) return false;
                bool res = false;
                foreach (var u in meshUnits) foreach (var l in u.edgeList)
                    {
                        var p1 = TranformPoint(l.p1);
                        var p2 = TranformPoint(l.p2);

                        if (p1.x > p2.x) (p1, p2) = (p2, p1);

                        // 在线上
                        //if (p1.x == p2.x && p1.x == p.x && ((p.y < p1.y && p.y > p2.y) || (p.y > p1.y && p.y < p2.y))) return true;

                        if (p.x < p1.x || p.x >= p2.x) continue;

                        if (Mathf.Lerp(p1.y, p2.y, Mathf.InverseLerp(p1.x, p2.x, p.x)) > p.y) res = !res;
                    }
                return res;
            }
        }
        #endregion

        #region Utility

        #region 数学
        static Rect MergeBounds(params Rect[] bounds)
        {
            float xMin = float.MaxValue;
            float yMin = float.MaxValue;
            float xMax = float.MinValue;
            float yMax = float.MinValue;
            foreach (var r in bounds)
            {
                if (r.xMin < xMin) xMin = r.xMin;
                if (r.yMin < yMin) yMin = r.yMin;
                if (r.xMax > xMax) xMax = r.xMax;
                if (r.yMax > yMax) yMax = r.yMax;
            }
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }
        static Mesh MergeMesh(IEnumerable<MeshUnit> units)
        {
            var vertexList = new List<Vector3>();
            var normalList = new List<Vector3>();
            var tangentList = new List<Vector4>();
            var uvList = new List<Vector2>();
            var uv3List = new List<Vector2>();
            var triangleList = new List<int>();

            EditorUtility.DisplayProgressBar("Mesh融合中", "正在收集Mesh信息……", 0);
            var outputMap = new Dictionary<Mesh, List<int>>();
            foreach (var u in units)
            {
                var tl = u.mesh.triangles;
                if (!outputMap.TryGetValue(u.mesh, out var triList)) triList = outputMap[u.mesh] = new List<int>();
                foreach (var t in u.triangleList)
                {
                    triList.Add(tl[t]);
                    triList.Add(tl[t + 1]);
                    triList.Add(tl[t + 2]);
                }
            }

            var sortSet = new HashSet<int>();
            List<int> sortList;
            var redirMap = new Dictionary<int, int>();

            int id = 0;
            foreach (var kv in outputMap)
            {
                var m = kv.Key;
                var tl = kv.Value;

                sortSet.Clear();
                foreach (var t in tl) sortSet.Add(t);

                sortList = sortSet.ToList();
                sortList.Sort();

                redirMap.Clear();
                int count = sortList.Count;
                for (int i = 0; i < count; i++)
                {
                    EditorUtility.DisplayProgressBar("Mesh融合中", $"正在处理Mesh数据……{i + 1}/{count}", (i + 1) / (float)count);

                    var t = sortList[i];
                    redirMap.Add(t, id++);

                    // 这里处理其他数据
                    vertexList.Add(m.vertices[t]);
                    normalList.Add(m.normals[t]);
                    tangentList.Add(m.tangents[t]);
                    uvList.Add(m.uv[t]);
                    uv3List.Add(Vector2.one);
                }

                EditorUtility.ClearProgressBar();
                foreach (var t in tl)
                {
                    // 这里处理triangles
                    triangleList.Add(redirMap[t]);
                }
            }

            return new Mesh
            {
                vertices = vertexList.ToArray(),
                normals = normalList.ToArray(),
                tangents = tangentList.ToArray(),
                uv = uvList.ToArray(),
                uv2 = uvList.ToArray(),
                uv3 = uv3List.ToArray(),
                triangles = triangleList.ToArray()
            };
        }
        static Mesh MergeMesh(IEnumerable<MeshGroup> groups)
        {
            // 用于生成导出的Mesh
            var vertexList = new List<Vector3>();
            var normalList = new List<Vector3>();
            var tangentList = new List<Vector4>();
            var uvList = new List<Vector2>();
            var triangleList = new List<int>();

            EditorUtility.DisplayProgressBar("Mesh融合中", "正在收集Mesh信息……", 0);
            int offset = 0;
            foreach (var g in groups)
            {
                var m = g.Mesh;

                vertexList.AddRange(m.vertices);
                normalList.AddRange(m.normals);
                tangentList.AddRange(m.tangents);
                uvList.AddRange(m.uv2);

                var tris = m.triangles;
                foreach (var t in tris)
                {
                    triangleList.Add(t + offset);
                }

                offset += m.vertexCount;
                EditorUtility.DisplayProgressBar("Mesh融合中", $"正在收集Mesh信息……{offset}", 0);
            }

            EditorUtility.ClearProgressBar();
            return new Mesh
            {
                vertices = vertexList.ToArray(),
                normals = normalList.ToArray(),
                tangents = tangentList.ToArray(),
                uv = uvList.ToArray(),
                triangles = triangleList.ToArray()
            };
        }
        #endregion

        #region GUI Shortcut
        GUIStyle GetItemStyle(bool on, bool hover = false) => (on, hover) switch
        {
            (false, false) => StlItem,
            (false, true) => StlItemHover,
            (true, false) => StlItemOn,
            (true, true) => StlItemOnHover,
        };
        static bool GetKeyDown(KeyCode key) => E.type == EventType.KeyDown && E.keyCode == key && !E.control && !E.alt && !E.shift;
        static bool GetKeyDownWithControl(KeyCode key) => E.type == EventType.KeyDown && E.keyCode == key && E.control && !E.alt && !E.shift;
        static bool GetKeyDownWithAlt(KeyCode key) => E.type == EventType.KeyDown && E.keyCode == key && !E.control && E.alt && !E.shift;
        static bool GetKeyDownWithShift(KeyCode key) => E.type == EventType.KeyDown && E.keyCode == key && !E.control && !E.alt && E.shift;
        void DrawOutline(Rect rect)
        {
            Handles.DrawLine(new Vector3(rect.xMin, rect.yMin), new Vector3(rect.xMin, rect.yMax));
            Handles.DrawLine(new Vector3(rect.xMin, rect.yMin), new Vector3(rect.xMax, rect.yMin));
            Handles.DrawLine(new Vector3(rect.xMax, rect.yMax), new Vector3(rect.xMin, rect.yMax));
            Handles.DrawLine(new Vector3(rect.xMax, rect.yMax), new Vector3(rect.xMax, rect.yMin));
        }
        bool Dialog() => Dialog("确定？操作无法撤销");
        float SeparatorHorizontal(Rect rect, GUIStyle style, float width, bool inverse = false)
        {
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);
            var separator = GetControlID();
            switch (E.type)
            {
                case EventType.MouseDown:
                    if (E.button == 0 && rect.Contains(E.mousePosition))
                    {
                        GUIHotControl = separator;
                        SetFloat("MouseDownXOffset", width + (inverse ? E.mousePosition.x : -E.mousePosition.x));
                        E.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIHotControl == separator)
                    {
                        GUIHotControl = 0;
                        E.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIHotControl == separator)
                    {
                        width = GetFloat("MouseDownXOffset") + (inverse ? -E.mousePosition.x : E.mousePosition.x);
                        E.Use();
                        Repaint();
                    }
                    break;
                case EventType.Repaint:
                    StyleBox(rect, style);
                    break;
            }
            return width;
        }
        Rect MovableViewport(Rect workspace)
        {
            float scale = GetFloat("ViewScale", 0.9f * 1024 / ViewWidth);
            Vector2 offset = GetVector2("ViewOffset");
            float unscaledSize = Mathf.Min(workspace.height, workspace.width) * 0.5f * ViewWidth / 1024;

            int preMoveViewControl = GetControlID();
            int moveViewControl = GetControlID();
            bool CanMoveView() => workspace.Contains(E.mousePosition);

            var center = workspace.center;
            float size = unscaledSize * scale;
            float borderX = Mathf.Max(workspace.width / unscaledSize * 0.5f, scale);
            offset.x = Mathf.Clamp(offset.x, -borderX, borderX);
            float borderY = Mathf.Max(workspace.height / unscaledSize * 0.5f, scale);
            offset.y = Mathf.Clamp(offset.y, -borderY, borderY);

            switch (E.type)
            {
                case EventType.KeyDown:
                    if (GUIHotControl == 0 && CanMoveView() && E.keyCode == KeyCode.Space)
                    {
                        GUIHotControl = preMoveViewControl;
                        E.Use();
                    }
                    break;
                case EventType.KeyUp:
                    if (GUIHotControl == preMoveViewControl && E.keyCode == KeyCode.Space)
                    {
                        GUIHotControl = 0;
                        E.Use();
                    }
                    break;
                case EventType.MouseDown:
                    if (CanMoveView() && (GUIHotControl == preMoveViewControl || (GUIHotControl == 0 && E.button != 0)))
                    {
                        GUIHotControl = moveViewControl;
                        SetVector2("MouseDownPosOffset", offset * unscaledSize - E.mousePosition);
                        E.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIHotControl == moveViewControl)
                    {
                        GUIHotControl = 0;
                        E.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIHotControl == moveViewControl)
                    {
                        SetVector2("ViewOffset", offset = (GetVector2("MouseDownPosOffset") + E.mousePosition) / unscaledSize);
                        E.Use();
                    }
                    break;
                case EventType.ScrollWheel:
                    if (CanMoveView())
                    {
                        Vector2 delta = offset + (workspace.center - E.mousePosition) / unscaledSize;
                        var newScale = Mathf.Clamp(scale * (1 - E.delta.y * 0.05f), 0.1f, 32);
                        SetVector2("ViewOffset", offset += delta * (newScale / scale - 1));
                        SetFloat("ViewScale", scale = newScale);
                        E.Use();
                    }
                    break;
                case EventType.Repaint:
                    if (GUIHotControl == moveViewControl || GUIHotControl == preMoveViewControl)
                    {
                        EditorGUIUtility.AddCursorRect(workspace, MouseCursor.Pan);
                    }
                    break;
            }

            return new Rect
                (
                    center.x - size + offset.x * unscaledSize,
                    center.y - size + offset.y * unscaledSize,
                    size + size,
                    size + size
                );
        }
        void ImportMeshBox(string text = "将Mesh拖入此处")
        {
            bool performDrag = E.type == EventType.DragPerform;

            // 新增Mesh
            ObjectField(ref meshDropper);
            if (E.type == EventType.Repaint) GUI.Label(GUILayoutUtility.GetLastRect().MoveEdge(2, -20, 2, -2), text, StlDockField);


            if (performDrag && meshDropper != null) SetBool("ImportResource", true);
        }
        void ImportTextureBox(string text = "将贴图拖入此处")
        {
            bool performDrag = E.type == EventType.DragPerform;

            // 新增贴图
            ObjectField(ref texDropper);
            if (E.type == EventType.Repaint) GUI.Label(GUILayoutUtility.GetLastRect().MoveEdge(2, -20, 2, -2), text, StlDockField);

            if (performDrag && texDropper != null) SetBool("ImportResource", true);
        }
        void MeshGroupBox(MeshGroup g, bool forceMultiSelect = false, bool canSelect = true)
        {
            using (Horizontal(GetItemStyle(selectedGroups.Contains(g), hoveringGroups.Contains(g))))
            {
                g.tintColor = EditorGUILayout.ColorField(GUIContent.none, g.tintColor, false, false, false, GUILayout.Width(16));

                if (Button(g.name, StlLabel) && canSelect) SelectGroup(forceMultiSelect, g);

                if (!canSelect && IceButton("还原"))
                {
                    commandQueue.Enqueue(() =>
                    {
                        EditingPack.deletedGroups.Remove(g);
                        EditingPack.meshGroups.Add(g);
                        selectedGroups.Add(g);
                        ResetGroup(new MeshGroup[] { g });
                    });
                }
            }

            if (E.type == EventType.Repaint)
            {
                var sRect = GetLastRect();

                // 处理 hover
                var margin = StlItem.margin;
                if ((hoveringGroups.Count != 1 || !hoveringGroups.Contains(g)) && sRect.MoveEdge(-margin.left, margin.right, -margin.top, margin.bottom).Contains(E.mousePosition))
                {
                    hoveringGroups.Clear();
                    hoveringGroups.Add(g);
                    Repaint();
                }
            }
        }
        void MeshResBox(MeshPack p, MeshResource res, bool canRemove = true)
        {
            var mesh = res.mesh;

            using (Vertical(GetItemStyle(false)))
            {
                using (HORIZONTAL)
                {
                    Label(mesh.name);
                    Space();

                    if (canRemove && IceButton("删除".Color(Color.red)) && Dialog())
                    {
                        RemoveMesh(p, res);
                    }
                }

                // 意外情况
                {
                    if (mesh.vertices.Length == 0) LabelError("意料之外的情况：vertices 为空！");
                    if (mesh.normals.Length == 0) LabelError("意料之外的情况：normals 为空！");
                    if (mesh.tangents.Length == 0) LabelError("意料之外的情况：tangents 为空！");
                    if (mesh.uv.Length == 0) LabelError("意料之外的情况：uv 为空！");
                    if (mesh.triangles.Length == 0) LabelError("意料之外的情况：triangles 为空！");
                    if (mesh.uv2.Length > 0) LabelError("意料之外的情况：uv2 不为空！");
                    if (mesh.uv3.Length > 0) LabelError("意料之外的情况：uv3 不为空！");
                    if (mesh.uv4.Length > 0) LabelError("意料之外的情况：uv4 不为空！");
                    if (mesh.uv5.Length > 0) LabelError("意料之外的情况：uv5 不为空！");
                    if (mesh.uv6.Length > 0) LabelError("意料之外的情况：uv6 不为空！");
                    if (mesh.uv7.Length > 0) LabelError("意料之外的情况：uv7 不为空！");
                    if (mesh.uv8.Length > 0) LabelError("意料之外的情况：uv8 不为空！");
                    if (mesh.colors.Length > 0) LabelError("意料之外的情况：colors 不为空！");
                    if (mesh.colors32.Length > 0) LabelError("意料之外的情况：colors32 不为空！");
                    if (mesh.boneWeights.Length > 0) LabelError("意料之外的情况：boneWeights 不为空！");
                }
            }
        }
        void MeshPackBox(MeshPack p, bool editing = false)
        {
            if (p.meshResMap.Any())
            {
                PreviewBox(groupSetOverride: p.meshGroups);
            }
            using (GROUP)
            {
                TextField("名字", ref p.name);
                if (!editing && Button("导出FBX")) ExportToFbx(EditorUtility.SaveFilePanel("导出FBX", "", p.name, "fbx"), p.meshGroups);
            }
            using (BOX) using (SectionFolder("Mesh"))
            {
                Space(4);
                foreach (var res in p.meshResMap.Values) MeshResBox(p, res, editing);
                Space(4);
                ImportMeshBox();
            }
            using (BOX) using (SectionFolder("贴图"))
            {
                Space(4);
                for (int i = 0; i < p.meshTextures.Count; i++)
                {
                    var tex = p.meshTextures[i];
                    string resolution = tex.width == tex.height ? $"{tex.width}" : $"{tex.width} : {tex.height}";
                    TextureBoxComplex
                    (tex, true, false, false, $"{resolution}", GetItemStyle(p.outputTextureIndex == i),
                    ("选择", () =>
                    {
                        if (p.outputTextureIndex != i)
                        {
                            p.outputTextureIndex = i;
                            if (!editing)
                            {
                                RenderCanvas();
                                FulfillCanvas();
                            }
                        }
                    }
                    ),
                    (editing ? "删除".Color(Color.red) : null, () =>
                    {
                        if (Dialog())
                        {
                            int index = i;
                            commandQueue.Enqueue(() =>
                            {
                                p.meshTextures.RemoveAt(index);
                                if (p.outputTextureIndex >= index && p.outputTextureIndex > 0) --p.outputTextureIndex;
                            });
                        }
                    }
                    ));
                }
                Space(4);
                ImportTextureBox();
            }
        }
        void PreviewBox(float height = 322, GUIStyle styleOverride = null, IEnumerable<MeshGroup> groupSetOverride = null)
        {
            int control = GetControlID();
            var r = GetRect(GUILayout.ExpandWidth(true), GUILayout.Height(height));
            if (r.width <= 0 || r.height <= 0) return;

            switch (E.type)
            {
                case EventType.MouseDown:
                    var mousePos = E.mousePosition;
                    if (r.ApplyBorder(-16).Contains(mousePos))
                    {
                        if (E.button == 1)
                        {
                            var gm = new GenericMenu();
                            gm.AddItem(new GUIContent("重置视角"), false, () =>
                            {
                                Pack.SetVector2("previewAngle", Vector2.zero);
                            });
                            gm.ShowAsContext();
                        }
                        else
                        {
                            GUIHotControl = control;
                            SetVector2("MouseDownPreviewOffset", GetVector2("previewAngle") - mousePos / r.width);
                        }
                        E.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIHotControl == control)
                    {
                        GUIHotControl = 0;
                        E.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIHotControl == control)
                    {
                        SetVector2("previewAngle", GetVector2("MouseDownPreviewOffset") + E.mousePosition / r.width);
                        E.Use();
                    }
                    break;
                case EventType.Repaint:
                    Vector2 previewAngle = GetVector2("previewAngle");
                    Quaternion rot = Quaternion.AngleAxis(previewAngle.y * 180, Vector3.left) * Quaternion.AngleAxis(-previewAngle.x * 180, Vector3.up);

                    Preview.camera.transform.position = new Vector3(0, 0, -8);
                    Preview.camera.transform.rotation = Quaternion.identity;
                    Preview.camera.nearClipPlane = 0.01f;
                    Preview.camera.farClipPlane = 20;

                    Preview.BeginPreview(new Rect(r) { x = 0, y = 0 }, styleOverride ?? StlGroup);
                    Vector3 imin = Vector3.one * float.MaxValue;
                    Vector3 imax = Vector3.one * float.MinValue;
                    var groupSet = groupSetOverride ?? selectedGroups;
                    foreach (var g in groupSet)
                    {
                        var b = g.Mesh.bounds;
                        imin = Vector3.Min(imin, b.min);
                        imax = Vector3.Max(imax, b.max);
                    }

                    Vector3 center = (imin + imax) * 0.5f;
                    Vector3 ex = imax - center;
                    float sca = Mathf.Max(ex.x, ex.y, ex.z);
                    Matrix4x4 mat4 = Matrix4x4.TRS(-center / sca, Quaternion.identity, Vector3.one / sca);
                    mat4 = Matrix4x4.Rotate(rot) * mat4;

                    Material mat;
                    if (EditingPack != null)
                    {
                        mat = PreviewPackMat;
                        mat.SetTexture("_MainTex", EditingPack.OutputTexture ?? Texture2D.grayTexture);
                    }
                    else
                    {
                        mat = PreviewMat;
                        mat.SetTexture("_MainTex", canvasRT);
                    }
                    foreach (var g in groupSet) Preview.DrawMesh(g.Mesh, mat4, mat, 0);

                    Preview.Render();
                    Preview.EndAndDrawPreview(r);
                    break;
            }
        }
        #endregion

        #region 选择组
        void SelectGroup(bool forceMultiSelect, params MeshGroup[] gs)
        {
            if (!forceMultiSelect && !E.control && !E.shift) selectedGroups.Clear();
            if (gs.All(g => selectedGroups.Contains(g)))
            {
                foreach (var g in gs) selectedGroups.Remove(g);
            }
            else
            {
                foreach (var g in gs) selectedGroups.Add(g);
            }
            GUIUtility.keyboardControl = 0;
            Repaint();
        }
        void SelectInverseGroups()
        {
            foreach (var g in MeshGroups) if (!selectedGroups.Remove(g)) selectedGroups.Add(g);
            GUIUtility.keyboardControl = 0;
            Repaint();
        }
        void SelectAllGroups()
        {
            selectedGroups.Clear();
            foreach (var g in MeshGroups) selectedGroups.Add(g);
            Repaint();
        }
        void SelectNoneGroup()
        {
            selectedGroups.Clear();
            GUIUtility.keyboardControl = 0;
            Repaint();
        }
        #endregion

        #region 编辑组
        void SortGroups(bool order = true)
        {
            MeshGroups.Sort((a, b) => (order ? 1 : -1) * a.name.CompareTo(b.name));
            Repaint();
        }
        void Group(MeshPack p, params MeshGroup[] groups)
        {
            selectedGroups.Clear();

            var gf = groups.First();
            string name = gf.name;
            int _index = name.LastIndexOf("_00");
            if (_index > 0) name = name.Substring(0, _index);

            var units = new List<MeshUnit>();
            foreach (var g in groups)
            {
                units.AddRange(g.meshUnits);
                p.meshGroups.Remove(g);
            }
            var newGroup = new MeshGroup(name, units);
            newGroup.Offset = gf.Offset;
            newGroup.Scale = gf.Scale;
            newGroup.Orientation = gf.Orientation;
            p.meshGroups.Add(newGroup);
            selectedGroups.Add(newGroup);

            hoveringGroups.Clear();
            Repaint();
        }
        void Disgroup(MeshPack p, MeshGroup g)
        {
            selectedGroups.Clear();
            p.meshGroups.Remove(g);
            int id = 0;
            foreach (var u in g.meshUnits)
            {
                var newGroup = new MeshGroup($"{g.name}_{id++:00}", new MeshUnit[] { u }, g);
                p.meshGroups.Add(newGroup);
                selectedGroups.Add(newGroup);
            }

            hoveringGroups.Clear();
            Repaint();
        }
        void ResetGroup(IEnumerable<MeshGroup> groups, MeshPack inPack = null, bool resetPos = true)
        {
            float? inPackScale = inPack?.OutputTextureWidth / (float)CanvasWidth;

            float GetTexScale(MeshGroup g)
            {
                if (inPackScale != null) return inPackScale.Value;

                MeshPack pack = null;
                foreach (var p in meshPacks)
                {
                    if (p.meshGroups.Contains(g))
                    {
                        pack = p;
                        break;
                    }
                }
                return (pack?.OutputTextureWidth ?? 1024.0f) / (float)CanvasWidth;
            }

            foreach (var g in groups)
            {
                float scale = GetTexScale(g);
                g.Scale = scale;
                if (resetPos)
                {
                    var center = g.RawBound.center;
                    center.y = 1 - center.y;
                    g.Offset = center * (scale - 1);
                    g.Orientation = MeshOrientation.R0;
                }
            }
        }
        void AutoLayout(int margin)
        {

            ResetGroup(MeshGroups, resetPos: false);

            var l = new List<Rect>() { new Rect(0, 0, 1, 1) };

            bool CheckContain(Rect r, Vector2 size) => r.width >= size.x && r.height >= size.y;
            HashSet<Rect> Split(Rect r, Rect obj)
            {
                var res = new HashSet<Rect>();
                if (r.xMin < obj.xMin)
                {
                    res.Add(Rect.MinMaxRect(r.xMin, r.yMin, obj.xMin, r.yMax));
                }
                if (r.xMax > obj.xMax)
                {
                    res.Add(Rect.MinMaxRect(obj.xMax, r.yMin, r.xMax, r.yMax));
                }
                if (r.yMin < obj.yMin)
                {
                    res.Add(Rect.MinMaxRect(r.xMin, r.yMin, r.xMax, obj.yMin));
                }
                if (r.yMax > obj.yMax)
                {
                    res.Add(Rect.MinMaxRect(r.xMin, obj.yMax, r.xMax, r.yMax));
                }
                return res;
            }

            Vector2 m = Vector2.one * ((float)margin / CanvasWidth);

            foreach (var g in MeshGroups)
            {
                var size = g.Size + m + m;

                float minY = float.MaxValue;
                float minX = float.MaxValue;
                int target = -1;
                // 找到符合要求的r
                for (int i = 0; i < l.Count; i++)
                {
                    var r = l[i];
                    if (CheckContain(r, size))
                    {
                        // 可包含
                        float top = r.yMin + size.y;
                        float left = r.xMin;
                        if (top < minY || (top == minY && minX < left))
                        {
                            minY = top;
                            minX = left;
                            target = i;
                        }
                    }
                }

                // 没找到
                if (target < 0)
                {
                    g.Position = new Vector2(0, 1);
                    continue;
                }

                // 分裂
                var tr = l[target];
                l.RemoveAt(target);
                g.PositionInversed = tr.min + m;
                var obj = new Rect(tr.min, size);
                l.AddRange(Split(tr, obj));

                // 整体分裂
                for (int i = l.Count - 1; i >= 0; --i)
                {
                    var r = l[i];
                    if (r.Overlaps(obj))
                    {
                        l.RemoveAt(i);
                        l.AddRange(Split(r, obj));
                    }
                }

                // 去重
                l.Sort((a, b) =>
                {
                    var res = a.width * a.height - b.width * b.height;
                    return res > 0 ? 1 : res < 0 ? -1 : 0;
                });
                for (int i = l.Count - 1; i > 0; --i)
                {
                    if (l[i].Contains(l[i - 1]))
                    {
                        l.RemoveAt(i - 1);
                    }
                }
            }
        }
        #endregion

        #region 图形处理
        Material PreviewMat => _previewMat != null ? _previewMat : (_previewMat = new Material(Shader.Find("Hidden/UVPreview"))); [NonSerialized] Material _previewMat = null;
        Material PreviewPackMat => _previewPackMat != null ? _previewPackMat : (_previewPackMat = new Material(Shader.Find("Standard"))); [NonSerialized] Material _previewPackMat = null;
        Material UVMat => _uvMat != null ? _uvMat : (_uvMat = new Material(Shader.Find("Hidden/UVDisplayer"))); [NonSerialized] Material _uvMat = null;
        GameObject _camObject;
        Camera _canvasCamera = null;
        Camera CanvasCamera
        {
            get
            {
                if (_canvasCamera == null)
                {
                    if (_camObject == null)
                    {
                        _camObject = new GameObject("Canvas Camera", typeof(Camera));
                        _camObject.transform.position = Vector3.back;
                        _camObject.hideFlags = HideFlags.HideAndDontSave;
                        _camObject.SetActive(false);
                    }
                    _canvasCamera = _camObject.GetComponent<Camera>();
                    _canvasCamera.clearFlags = CameraClearFlags.Nothing;
                    _canvasCamera.backgroundColor = Color.black;
                    _canvasCamera.cullingMask = 1 << 31;
                }
                return _canvasCamera;
            }
        }
        int ViewWidth
        {
            get => EditingPack != null ? EditingPack.OutputTextureWidth : _canvasWidth;
        }
        int CanvasWidth
        {
            get => _canvasWidth;
            set
            {
                if (_canvasWidth != value)
                {
                    float scale = (float)_canvasWidth / value;

                    foreach (var g in MeshGroups)
                    {
                        g.Scale *= scale;
                        var center = g.RawBound.center;
                        center.y = 1 - center.y;
                        g.Offset = center * (scale - 1) + g.Offset * scale;
                    }
                    _canvasWidth = value;
                    CreateCanvasResources();
                    RenderCanvas();
                    FulfillCanvas();
                }
            }
        }
        int _canvasWidth = 1024;
        RenderTexture canvasRT = null;
        RenderTexture canvasComputeRT = null;

        ComputeShader UVUtility => _uvUtility != null ? _uvUtility : (_uvUtility = AssetDatabase.LoadAssetAtPath<ComputeShader>(uvUtilityPath)); ComputeShader _uvUtility = null;
        const string uvUtilityPath = "Assets/Scripts/UVUtility.compute";
        int FulfillUVInitializeKernel => _fulfillUVInitializeKernel ?? (_fulfillUVInitializeKernel = UVUtility.FindKernel("FulfillUVInitialize")).Value; int? _fulfillUVInitializeKernel = null;
        int FulfillUVForwardKernel => _fulfillUVForwardKernel ?? (_fulfillUVForwardKernel = UVUtility.FindKernel("FulfillUVForward")).Value; int? _fulfillUVForwardKernel = null;
        int FulfillUVBackwardKernel => _fulfillUVBackwardKernel ?? (_fulfillUVBackwardKernel = UVUtility.FindKernel("FulfillUVBackward")).Value; int? _fulfillUVBackwardKernel = null;
        int FulfillCanvasKernel => _fulfillCanvasKernel ?? (_fulfillCanvasKernel = UVUtility.FindKernel("FulfillCanvas")).Value; int? _fulfillCanvasKernel = null;
        void InitializeCanvasResource()
        {
            canvasRT = new RenderTexture(CanvasWidth, CanvasWidth, 0, RenderTextureFormat.ARGBFloat);
            canvasRT.enableRandomWrite = true;
            canvasRT.filterMode = FilterMode.Point;
            canvasRT.name = "Output Texture";
            canvasRT.Create();

            canvasComputeRT = new RenderTexture(CanvasWidth, CanvasWidth, 0, RenderTextureFormat.ARGBFloat);
            canvasComputeRT.enableRandomWrite = true;
            canvasComputeRT.filterMode = FilterMode.Point;
            canvasComputeRT.Create();

        }
        void DisposeCanvasResource()
        {
            CanvasCamera.targetTexture = null;
            if (canvasRT != null)
            {
                canvasRT.DiscardContents();
                canvasRT.Release();
                canvasRT = null;
            }
            if (canvasComputeRT != null)
            {
                canvasComputeRT.DiscardContents();
                canvasComputeRT.Release();
                canvasComputeRT = null;
            }
        }
        void CreateCanvasResources()
        {
            DisposeCanvasResource();
            InitializeCanvasResource();
        }
        void RenderCanvas(Mesh mesh, Texture tex, bool compute, bool first, bool packStart)
        {
            Graphics.DrawMesh(mesh, Matrix4x4.Scale(Vector3.zero), UVMat, 31, CanvasCamera);
            if (packStart) UVMat.SetTexture("_MainTex", compute ? Texture2D.whiteTexture : tex);
            if (first)
            {
                if (compute || Pack.GetBool("检查尺寸")) CanvasCamera.clearFlags = CameraClearFlags.SolidColor;
                CanvasCamera.targetTexture = compute ? canvasComputeRT : canvasRT;
                CanvasCamera.Render();
                CanvasCamera.clearFlags = CameraClearFlags.Nothing;
            }
            else
            {
                CanvasCamera.Render();
            }
        }
        Task fulfillTask;
        float? fulfillProgress;
        bool autoFulfill = true;
        async void FulfillCanvas(bool force = false)
        {
            fulfillProgress = null;
            if (Pack.GetBool("检查尺寸")) return;
            if (!autoFulfill && !force) return;
            if (fulfillTask != null)
            {
                await fulfillTask;
                fulfillTask = null;
            }
            fulfillTask = _FulfillCanvas(force);
        }
        async Task _FulfillCanvas(bool force)
        {
            fulfillProgress = 0;
            int group = CanvasWidth / 16;

            UVUtility.SetInt("size", CanvasWidth);
            UVUtility.SetTexture(FulfillUVInitializeKernel, "base", canvasComputeRT);
            UVUtility.Dispatch(FulfillUVInitializeKernel, group, group, 1);

            int counter = 0;
            int maxCounter = (1 << (force ? 18 : 16)) / CanvasWidth;
            UVUtility.SetTexture(FulfillUVForwardKernel, "base", canvasComputeRT);
            for (int y = 0; y < CanvasWidth; y++)
            {
                UVUtility.SetInt("y", y);
                UVUtility.Dispatch(FulfillUVForwardKernel, 1, 1, 1);
                if (++counter >= maxCounter)
                {
                    counter = 0;
                    await Task.Delay(1);
                    if (fulfillProgress is null) return;
                    fulfillProgress = (0.5f * y) / CanvasWidth;
                    Repaint();
                }
            }

            UVUtility.SetTexture(FulfillUVBackwardKernel, "base", canvasComputeRT);
            for (int y = CanvasWidth - 1; y >= 0; y--)
            {
                UVUtility.SetInt("y", y);
                UVUtility.Dispatch(FulfillUVBackwardKernel, 1, 1, 1);
                if (++counter >= maxCounter)
                {
                    counter = 0;
                    await Task.Delay(1);
                    if (fulfillProgress is null) return;
                    fulfillProgress = 1 - (0.5f * y) / CanvasWidth;
                    Repaint();
                }
            }

            UVUtility.SetTexture(FulfillCanvasKernel, "canvas", canvasRT);
            UVUtility.SetTexture(FulfillCanvasKernel, "base", canvasComputeRT);
            UVUtility.Dispatch(FulfillCanvasKernel, group, group, 1);

            fulfillProgress = null;
            Repaint();
        }
        void RenderCanvas(bool compute)
        {
            if (compute && Pack.GetBool("检查尺寸"))
            {
                UVMat.EnableKeyword("SCALE_DEBUG");
            }
            else
            {
                UVMat.DisableKeyword("SCALE_DEBUG");
            }
            bool first = true;
            foreach (var p in meshPacks)
            {
                bool packStart = true;
                foreach (var g in p.meshGroups)
                {
                    if (compute) g.UpdateUV(false, ((float)CanvasWidth) / (p.OutputTextureWidth));
                    RenderCanvas(g.Mesh, p.OutputTexture ?? Texture2D.blackTexture, compute, first, packStart);
                    packStart = false;
                    first = false;
                }
            }
        }
        void RenderCanvas()
        {
            if (canvasRT == null || canvasComputeRT == null) CreateCanvasResources();
            RenderCanvas(true);
            RenderCanvas(false);
        }
        #endregion

        #region 资源
        MeshPack AddNewPack(string name = "新资源组")
        {
            meshPacks.Add((MeshPack)(inspectingPack = new MeshPack(name)));
            return inspectingPack;
        }
        void OnImportResource(MeshPack p)
        {
            bool bImport = meshDropper != null || texDropper != null;
            if (bImport && p is null) p = AddNewPack();

            // 导入Mesh
            if (meshDropper != null)
            {
                if (!p.meshResMap.ContainsKey(meshDropper.name))
                {
                    var res = new MeshResource(meshDropper, UpdateProgressBar);
                    p.meshResMap.Add(meshDropper.name, res);
                    p.meshGroups.Add(new MeshGroup(meshDropper.name, res.meshUnits));
                    _meshGroups.AddRange(p.meshGroups);
                    ResetGroup(p.meshGroups, p);
                    ClearProgressBar();
                }
                meshDropper = null;
            }

            // 导入Texture
            if (texDropper != null)
            {
                if (!p.meshTextures.Contains(texDropper))
                {
                    p.meshTextures.Add(texDropper);
                }
                texDropper = null;
            }

            if (bImport)
            {
                RenderCanvas();
                FulfillCanvas();
            }

            Pack.SetBool("ImportResource", false);
        }
        void RemoveMesh(MeshPack p, MeshResource res)
        {
            commandQueue.Enqueue(() => _RemoveMesh(p, res));
        }
        void _RemoveMesh(MeshPack p, MeshResource res)
        {
            selectedGroups.Clear();
            hoveringGroups.Clear();
            Mesh m = res.mesh;

            ProcessGroupList(p.meshGroups);
            ProcessGroupList(p.deletedGroups);
            void ProcessGroupList(List<MeshGroup> gList)
            {
                for (int i = gList.Count - 1; i >= 0; --i)
                {
                    var g = gList[i];
                    bool dirty = false;
                    for (int j = g.meshUnits.Count - 1; j >= 0; --j)
                    {
                        if (g.meshUnits[j].mesh == m)
                        {
                            g.meshUnits.RemoveAt(j);
                            dirty = true;
                        }
                    }
                    if (g.meshUnits.Count == 0)
                    {
                        gList.RemoveAt(i);
                    }
                    else if (dirty)
                    {
                        g.UpdateBound();
                        g.UpdateMesh();
                        g.UpdateUV(true, ((float)CanvasWidth) / (p.OutputTextureWidth));
                    }
                }
            }

            p.meshResMap.Remove(m.name);
        }
        #endregion

        #region 导出相关
        public void ExportToTga(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            File.WriteAllBytes(path, canvasRT.ReadPixels().EncodeToTGA());
        }
        public void ExportToPng(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            File.WriteAllBytes(path, canvasRT.ReadPixels().EncodeToPNG());
        }
        public void ExportToFbx(string path, IEnumerable<MeshGroup> groups)
        {
            if (string.IsNullOrEmpty(path)) return;

            PreviewPackMat.SetTexture("_MainTex", null);
            var outputObj = new GameObject("OutputObject");
            outputObj.AddComponent<MeshFilter>().mesh = MergeMesh(groups);
            outputObj.AddComponent<MeshRenderer>().sharedMaterial = PreviewPackMat;

            // 导出设置
            //var options = new ExportModelSettingsSerialize();
            //options.SetUseMayaCompatibleNames(false);
            //options.SetExportBinormals(false);
            //options.SetExportBindPose(true);
            //options.SetExportTangents(true);
            //options.SetExportBlendShapes(false);
            //options.SetExportFormat(ExportSettings.ExportFormat.Binary);
            //options.SetRefreshEditor(false);

            //ModelExporter.ExportObject(path, outputObj);
            DestroyImmediate(outputObj);
        }
        #endregion


        #endregion

        #region Fields
        static bool inMainWorkspace;
        MeshPack EditingPack
        {
            get => _editingPack; set
            {
                {
                    // 切换行为
                    hoveringGroups.Clear();
                }
                if (value is null)
                {
                    // 切换到主工作台
                    inMainWorkspace = true;
                    _meshGroups.Clear();
                    foreach (var p in meshPacks) _meshGroups.AddRange(p.meshGroups);
                    RenderCanvas();
                    FulfillCanvas();
                }
                else
                {
                    // 切换到Pack
                    inMainWorkspace = false;
                    selectedGroups.Clear();
                }
                _editingPack = value;
            }
        }
        [NonSerialized] MeshPack _editingPack = null;
        [NonSerialized] MeshPack inspectingPack = null;
        List<MeshPack> meshPacks = new();

        public List<MeshGroup> MeshGroups => EditingPack?.meshGroups ?? _meshGroups;
        List<MeshGroup> _meshGroups = new();

        readonly HashSet<MeshGroup> selectedGroups = new();
        readonly HashSet<MeshGroup> hoveringGroups = new();

        [NonSerialized] Mesh meshDropper;
        [NonSerialized] Texture2D texDropper;

        readonly Queue<Action> commandQueue = new();

        PreviewRenderUtility Preview => _preview ?? (_preview = new PreviewRenderUtility()); PreviewRenderUtility _preview = null;

        [NonSerialized] double lastMouseUpTime;
        [NonSerialized] Rect? selectRect = null;

        // 辅助线
        [NonSerialized] float? xRefLine = null;
        [NonSerialized] float? yRefLine = null;
        [NonSerialized] List<Rect> xRefRect = null;
        [NonSerialized] List<Rect> yRefRect = null;
        readonly Dictionary<float, List<Rect>> xRefMap = new();
        readonly Dictionary<float, List<Rect>> yRefMap = new();
        #endregion

        #region Events & Callbacks
        protected override void OnEnable()
        {
            base.OnEnable();
            CreateCanvasResources();
            wantsMouseMove = true;
            EditingPack = null;
        }
        void OnFocus() => Pack.SetBool("ImportResource", true);
        void Update()
        {
            if (commandQueue.Any()) commandQueue.Dequeue()?.Invoke();
            if (Pack.GetBool("ImportResource")) OnImportResource(EditingPack ?? inspectingPack);
        }
        void OnDisable()
        {
            DisposeCanvasResource();
            if (_preview != null)
            {
                _preview.Cleanup();
                _preview = null;
            }
        }
        void OnDestroy()
        {
            if (_camObject != null) DestroyImmediate(_camObject);
        }

        #endregion

        protected override void OnWindowGUI(Rect position)
        {
            const float separatorWidth = 2;
            const float separatorBorder = 2;
            const float separatorOutWidth = separatorWidth + separatorBorder + separatorBorder;

            // 左边选项
            float hierarchyWidth = GetFloat("hierarchyWidth", 256);
            Rect lRect = new Rect(position) { width = hierarchyWidth };
            Rect lsRect = new Rect(position) { x = hierarchyWidth - separatorWidth - separatorBorder, width = separatorOutWidth };

            // 右边选项
            float inspectorWidth = GetFloat("inspectorWidth", 354);
            Rect rRect = position.MoveEdge(position.width - inspectorWidth + separatorWidth);
            Rect rsRect = position.MoveEdge(position.width - inspectorWidth, separatorWidth - inspectorWidth).ApplyBorder(separatorBorder);

            // 顶端Dock
            const float dockHeight = 22;
            Rect dRect = position.MoveEdge(left: hierarchyWidth - separatorWidth - separatorBorder, right: -inspectorWidth + separatorWidth + separatorBorder, bottom: dockHeight - position.height);

            // 工作区
            Rect wRect = position.MoveEdge(left: hierarchyWidth, right: -inspectorWidth, top: dockHeight);

            // 真正的绘制
            using (LabelWidth(60))
            {
                OnWorkSpaceGUI(wRect);
                using (Area(lRect, "RL Background")) OnHierarchyGUI();
                using (Area(rRect, "RL Background")) using (ScrollInvisible("rRectScroll")) OnInspectorGUI();
                SetFloat("inspectorWidth", SeparatorHorizontal(rsRect, "dragtab scroller next", inspectorWidth, true));
                SetFloat("hierarchyWidth", SeparatorHorizontal(lsRect, "dragtab scroller prev", hierarchyWidth, false));
                using (Area(dRect, StlDock)) using (HORIZONTAL) OnDockGUI();
            }
        }


        void OnHierarchyGUI()
        {
            // 顶部信息
            if (EditingPack is null)
            {
                using (Horizontal(StlHierarchyHeader))
                {
                    Label("主面板", StlIce);
                    Space();
                    if (Button(TempContent("", "添加资源组"), "OL Plus"))
                    {
                        AddNewPack();
                    }
                }
            }
            else
            {
                using (Horizontal(StlHierarchyHeader))
                {
                    if (IceButton("<"))
                    {
                        EditingPack = null;
                        return;
                    }
                    Label(EditingPack.name, StlIce);
                    Space();
                    if (IceButton(new GUIContent("升序", "按名称升序排列")))
                    {
                        SortGroups(true);
                    }
                    if (IceButton(new GUIContent("降序", "按名称降序排列")))
                    {
                        SortGroups(false);
                    }
                }
            }

            // 下部信息
            using (ScrollInvisible("lRectScroll"))
            {
                if (EditingPack is null)
                {
                    // 主面板

                    // Packs一览
                    foreach (var p in meshPacks)
                    {
                        using (GROUP)
                        {
                            using (Horizontal(inspectingPack == p ? StlPackItemOn : StlPackItem))
                            {
                                // 点击区域
                                if (Button(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true)))
                                {
                                    inspectingPack = p;
                                    SelectGroup(false, p.meshGroups.ToArray());
                                }

                                // 标题
                                if (E.type == EventType.Repaint) StyleBox(GetLastRect(), StlPackName, p.name);

                                // 快捷选择Texture
                                int texWidth = 96;
                                if (p.meshTextures.Any())
                                {
                                    using (GUICHECK)
                                    {
                                        p.outputTextureIndex = EditorGUILayout.Popup(p.outputTextureIndex, p.meshTextures.Select(t => t.name).ToArray(), GUILayout.Width(texWidth));
                                        if (GUIChanged)
                                        {
                                            RenderCanvas();
                                            FulfillCanvas();
                                        }
                                    }
                                }
                                else EditorGUILayout.Popup(0, new string[] { "No textrue!" }, GUILayout.Width(texWidth));

                                // 编辑按钮
                                if (IceButton(">"))
                                {
                                    EditingPack = p;
                                    return;
                                }
                            }
                            // Groups一览
                            foreach (var g in p.meshGroups)
                            {
                                MeshGroupBox(g);
                            }
                        }
                    }

                    // 空白区域
                    var emptyRect = GetRect(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    if (emptyRect.Contains(E.mousePosition))
                    {
                        if (E.type == EventType.Repaint)
                        {
                            hoveringGroups.Clear();
                        }
                        else if (E.type == EventType.MouseDown && E.button == 0)
                        {
                            inspectingPack = null;
                            Repaint();
                        }
                    }
                }
                else
                {
                    // Pack面板

                    var p = EditingPack;

                    using (GROUP)
                    {
                        using (Horizontal(StlPackItemOn))
                        {
                            // 点击区域
                            if (Button(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true)))
                            {
                                SelectAllGroups();
                            }

                            // 标题
                            if (E.type == EventType.Repaint) StyleBox(GetLastRect(), StlPackName, p.name);

                            // 快捷选择Texture
                            int texWidth = 112;
                            if (p.meshTextures.Any()) p.outputTextureIndex = EditorGUILayout.Popup(p.outputTextureIndex, p.meshTextures.Select(t => t.name).ToArray(), GUILayout.Width(texWidth));
                            else EditorGUILayout.Popup(0, new string[] { "No textrue!" }, GUILayout.Width(texWidth));
                        }
                        // Groups一览
                        foreach (var g in p.meshGroups)
                        {
                            MeshGroupBox(g);
                        }
                    }
                    // 空白区域
                    var emptyRect = GetRect(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    if (emptyRect.Contains(E.mousePosition))
                    {
                        if (E.type == EventType.Repaint)
                        {
                            hoveringGroups.Clear();
                        }
                        else if (E.type == EventType.MouseDown && E.button == 0)
                        {
                            SelectNoneGroup();
                        }
                    }

                    if (p.deletedGroups.Any())
                    {
                        using (GROUP)
                        {
                            using (HORIZONTAL)
                            {
                                SectionHeader("已删除的Mesh块", false);
                                Space();
                                if (IceButton("全部还原"))
                                {
                                    p.meshGroups.AddRange(p.deletedGroups);
                                    selectedGroups.Clear();
                                    foreach (var g in p.deletedGroups) selectedGroups.Add(g);
                                    GUIUtility.keyboardControl = 0;
                                    ResetGroup(p.deletedGroups, p);
                                    p.deletedGroups.Clear();
                                    Repaint();
                                }
                            }
                            using (Folder("已删除的Mesh块"))
                            {
                                // Groups一览
                                foreach (var g in p.deletedGroups)
                                {
                                    MeshGroupBox(g, canSelect: false);
                                }
                            }
                        }
                    }
                }


            }
        }
        void OnWorkSpaceGUI(Rect wRect)
        {
            if (!meshPacks.Any())
            {
                using (Area(wRect))
                {
                    Space();
                    using (HORIZONTAL)
                    {
                        Space();
                        using (GROUP)
                        {
                            Label("还没有创建资源组！".Color(ThemeColorExp).Size(16));
                            Space(8);
                            using (HORIZONTAL)
                            {
                                Label("你可以", GUILayout.Width(64));
                                if (Button("点此创建新资源组", "button"))
                                {
                                    AddNewPack();
                                }
                            }
                            using (HORIZONTAL)
                            {
                                Label("或者", GUILayout.Width(64));
                                ImportMeshBox();
                            }
                            using (HORIZONTAL)
                            {
                                Label("或者", GUILayout.Width(64));
                                ImportTextureBox();
                            }
                        }
                        Space();
                    }
                    Space();
                }
                return;
            }

            bool bEditingPack = EditingPack != null;

            // 视口缩放
            Rect tRect = MovableViewport(wRect);
            bool mouseInRect = wRect.Contains(E.mousePosition);

            // 处理hover
            if (E.type == EventType.MouseMove)
            {
                if (mouseInRect)
                {
                    hoveringGroups.Clear();
                    foreach (var g in MeshGroups)
                    {
                        if (g.Contains(tRect.InverseSample(E.mousePosition, true))) hoveringGroups.Add(g);
                    }
                }
                Repaint();
            }

            // 绘制
            if (E.type == EventType.Repaint)
            {
                if (bEditingPack)
                {
                    // Pack视图

                    // 背景
                    StyleBox(wRect, "GameViewBackground");

                    var tex = EditingPack.OutputTexture;
                    if (tex != null) TextureBox(tex, tRect);
                    else StyleBox(tRect, StlDock);
                }
                else
                {
                    // 主视图

                    // 背景
                    if (GetBool("检查尺寸"))
                    {
                        if (canvasComputeRT != null) TextureBox(canvasComputeRT, tRect);
                    }
                    else
                    {
                        if (canvasRT != null) TextureBox(canvasRT, tRect);
                    }
                }

                // 画Mesh边缘
                foreach (var g in MeshGroups)
                {
                    Handles.color = g.tintColor;
                    g.DrawEdges(tRect);
                }

                // 画hovering
                foreach (var g in hoveringGroups)
                {
                    Handles.color = g.tintColor;
                    g.DrawLines(tRect);
                }

                // 画selected
                foreach (var g in selectedGroups)
                {
                    //Handles.color = g.tintColor;
                    //g.DrawLines(tRect);
                    Handles.color = Color.white;
                    g.DrawEdges(tRect);

                    Handles.color = new Color(0.7f, 0.7f, 0.7f);
                    DrawOutline(tRect.Viewport(g.Bound, true));
                }

                // 画选框
                if (selectRect != null)
                {
                    Handles.color = ThemeColor;
                    DrawOutline(selectRect.Value);
                }
            }

            // 控件
            {
                // 点选
                void SelectPoint()
                {
                    // 单选判断
                    if (!E.shift && !E.control) selectedGroups.Clear();

                    int gi = GetInt("LastSelectedGroupIndex");
                    int i = 0;

                    foreach (var g in hoveringGroups)
                    {
                        if (gi == i)
                        {
                            SelectGroup(true, g);
                            SetInt("LastSelectedGroupIndex", gi >= hoveringGroups.Count - 1 ? 0 : gi + 1);
                            Repaint();
                            break;
                        }
                        ++i;
                    }
                }
                // 框选
                void SelectRect()
                {
                    Vector2 p1 = E.mousePosition;
                    Vector2 p2 = GetVector2("MouseDownPos");
                    selectRect = Rect.MinMaxRect(Mathf.Min(p1.x, p2.x), Mathf.Min(p1.y, p2.y), Mathf.Max(p1.x, p2.x), Mathf.Max(p1.y, p2.y));
                    var sr = selectRect.Value;

                    if (!E.shift && !E.control) selectedGroups.Clear();
                    foreach (var g in MeshGroups)
                    {
                        var gr = tRect.Viewport(g.Bound, true);
                        if ((GetBool("框选相交/包含") ? sr.Overlaps(gr) : sr.Contains(gr)))
                        {
                            selectedGroups.Add(g);
                        }
                    }
                }
                // 双击编辑
                void EditPack()
                {
                    bool IsDoubleClick()
                    {
                        double delta = EditorApplication.timeSinceStartup - lastMouseUpTime;
                        return delta > 0 && delta < 0.25;
                    }
                    if (!E.shift && !E.control && IsDoubleClick() && selectedGroups.Count == 1 && hoveringGroups.Count == 1)
                    {
                        var g = selectedGroups.First();
                        EditingPack = meshPacks.Find(p => p.meshGroups.Contains(g));
                        SelectGroup(true, g);
                    }
                    lastMouseUpTime = EditorApplication.timeSinceStartup;
                }

                bool mouseDownInRect = mouseInRect && E.type == EventType.MouseDown;

                // 全局逻辑
                if (GUIHotControl == 0 && mouseDownInRect)
                {
                    if (inspectingPack != null) inspectingPack = null;
                    SetVector2("MouseDownPos", E.mousePosition);
                }
                if (E.type == EventType.MouseMove)
                {
                    SetInt("LastSelectedGroupIndex", 0);
                    Repaint();
                }

                // 控件
                int selPoint = GetControlID();  // 点选
                int selRect = GetControlID();   // 框选

                if (GUIHotControl == 0 && mouseDownInRect)
                {
                    if (E.button == 0)
                    {
                        GUIHotControl = selPoint;   // 默认点选
                    }
                }

                // 点选
                if (GUIHotControl == selPoint)
                {
                    if (E.type == EventType.MouseDrag)
                    {
                        GUIHotControl = selRect;
                    }
                    else if (E.type == EventType.MouseUp)
                    {
                        SelectPoint();
                        GUIHotControl = 0;
                        E.Use();
                    }
                }
                // 框选
                if (GUIHotControl == selRect)
                {
                    if (E.type == EventType.MouseDrag)
                    {
                        SelectRect();
                        E.Use();
                    }
                    else if (E.type == EventType.MouseUp)
                    {
                        selectRect = null;
                        GUIHotControl = 0;
                        E.Use();
                    }
                }

                if (!bEditingPack && selectedGroups.Any())
                {
                    var outterBound = tRect.Viewport(MergeBounds(selectedGroups.Select(g => g.Bound).ToArray()), true);

                    // 主界面物体编辑控件
                    int preObj = GetControlID();    // 准备对物体操作
                    int dragObj = GetControlID();   // 拖拽

                    // 激活条件
                    bool HoveringOnSelected()
                    {
                        if (E.control) return true;
                        foreach (var g in hoveringGroups)
                        {
                            if (selectedGroups.Contains(g))
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                    if (mouseDownInRect && E.button == 0 && HoveringOnSelected())
                    {
                        GUIHotControl = preObj;
                        foreach (var g in selectedGroups) g.offsetCache = g.Offset;
                        xRefMap.Clear();
                        yRefMap.Clear();
                        E.Use();
                    }

                    if (E.type == EventType.Repaint && HoveringOnSelected())
                    {
                        EditorGUIUtility.AddCursorRect(wRect, MouseCursor.MoveArrow);
                    }

                    // 分配操作
                    if (GUIHotControl == preObj)
                    {
                        // 拖拽
                        if (E.type == EventType.MouseDrag)
                        {
                            GUIHotControl = dragObj;
                            E.Use();
                        }
                    }
                    // 拖拽
                    if (GUIHotControl == dragObj)
                    {
                        if (E.type == EventType.MouseDrag)
                        {

                            var off = (E.mousePosition - GetVector2("MouseDownPos")) / tRect.size;
                            foreach (var g in selectedGroups) g.Offset = g.offsetCache + off;

                            if (GetBool("边缘吸附") != E.alt)
                            {
                                outterBound = tRect.Viewport(MergeBounds(selectedGroups.Select(g => g.Bound).ToArray()), true);

                                // 初始化参考线集
                                if (xRefMap.Count == 0 || yRefMap.Count == 0)
                                {
                                    void RecordRef(Rect r)
                                    {
                                        static void Rec(Dictionary<float, List<Rect>> map, float val, Rect r)
                                        {
                                            if (!map.TryGetValue(val, out var l))
                                            {
                                                l = map[val] = new List<Rect>();
                                            }
                                            l.Add(r);
                                        }

                                        Rec(xRefMap, r.xMin, r);
                                        Rec(xRefMap, r.xMax, r);
                                        Rec(yRefMap, r.yMin, r);
                                        Rec(yRefMap, r.yMax, r);
                                    }

                                    foreach (var g in MeshGroups) if (!selectedGroups.Contains(g)) RecordRef(tRect.Viewport(g.Bound, true));

                                    RecordRef(tRect);
                                }

                                bool CheckRefOffset(ref float min, ref float off, float off1, float off2)
                                {
                                    float absOff1 = Mathf.Abs(off1);
                                    float absOff2 = Mathf.Abs(off2);
                                    float dis = Mathf.Min(absOff1, absOff2);
                                    if (dis < min)
                                    {
                                        min = dis;
                                        off = absOff1 < absOff2 ? off1 : off2;
                                        return true;
                                    }
                                    return false;
                                }

                                xRefRect = null;
                                xRefLine = null;
                                float xMin = GetFloat("吸附距离", 8);
                                float xOff = 0;
                                foreach (var p in xRefMap)
                                {
                                    float x = p.Key;
                                    if (CheckRefOffset(ref xMin, ref xOff, x - outterBound.xMin, x - outterBound.xMax))
                                    {
                                        xRefLine = x;
                                        xRefRect = p.Value;
                                    }
                                }

                                yRefRect = null;
                                yRefLine = null;
                                float yMin = GetFloat("吸附距离", 8);
                                float yOff = 0;
                                foreach (var p in yRefMap)
                                {
                                    float y = p.Key;
                                    if (CheckRefOffset(ref yMin, ref yOff, y - outterBound.yMin, y - outterBound.yMax))
                                    {
                                        yRefLine = y;
                                        yRefRect = p.Value;
                                    }
                                }

                                Vector2 offInversed = new Vector2(xOff, yOff) / tRect.width;
                                foreach (var g in selectedGroups) g.Offset += offInversed;
                            }

                            if (autoFulfill) RenderCanvas();
                            E.Use();
                        }
                        else if (E.type == EventType.Repaint)
                        {
                            Handles.color = ThemeColor;
                            if (xRefLine != null) Handles.DrawLine(new Vector3(xRefLine.Value, wRect.yMin), new Vector3(xRefLine.Value, wRect.yMax));
                            if (yRefLine != null) Handles.DrawLine(new Vector3(wRect.xMin, yRefLine.Value), new Vector3(wRect.xMax, yRefLine.Value));
                            if (xRefRect != null) foreach (var r in xRefRect) DrawOutline(r);
                            if (yRefRect != null) foreach (var r in yRefRect) DrawOutline(r);
                        }
                    }

                    // 结束操作
                    if (E.type == EventType.MouseUp)
                    {
                        // 结束编辑
                        if (GUIHotControl == dragObj)
                        {
                            GUIHotControl = 0;
                            if (autoFulfill)
                            {
                                FulfillCanvas();
                            }
                            else
                            {
                                RenderCanvas();
                            }
                            Repaint();
                            E.Use();
                        }
                        // 点选
                        else if (GUIHotControl == preObj)
                        {
                            SelectPoint();
                            EditPack();
                            GUIHotControl = 0;
                            E.Use();
                        }
                    }

                    if (E.type == EventType.MouseMove)
                    {
                        lastMouseUpTime = 0;
                    }

                    // 绘制
                    if (E.type == EventType.Repaint)
                    {
                        Handles.color = Color.white;
                        DrawOutline(outterBound);
                    }

                    // 旋转控件
                    const float rotateControlRadius = 8;
                    const float rotateControlSize = rotateControlRadius + rotateControlRadius;
                    var rotControlRect = new Rect(outterBound.xMax, outterBound.yMin - rotateControlSize, rotateControlSize, rotateControlSize);
                    RotateControl(rotControlRect);

                    static MeshOrientation NextOrientation(MeshOrientation o)
                    {
                        return o switch
                        {
                            MeshOrientation.R0 => MeshOrientation.R90,
                            MeshOrientation.R90 => MeshOrientation.R180,
                            MeshOrientation.R180 => MeshOrientation.R270,
                            MeshOrientation.R270 => MeshOrientation.R0,
                            _ => throw new Exception("Invalid value")
                        };
                    }
                    void RotateControl(Rect r)
                    {
                        int id = GetControlID();
                        int idDrag = GetControlID();
                        switch (E.type)
                        {
                            case EventType.Repaint:
                                if (GUIHotControl == idDrag)
                                {
                                    r.center = E.mousePosition;
                                    Vector2 center = outterBound.center;
                                    Vector2 vec = GetVector2("RotateCenterVector");
                                    Vector2 newVec = E.mousePosition - outterBound.center;
                                    var rot = Quaternion.FromToRotation(vec, newVec);
                                    float x1 = center.x - vec.x;
                                    float x2 = center.x + vec.x;
                                    float y1 = center.y + vec.y;
                                    float y2 = center.y - vec.y;
                                    Vector2 p11 = (Vector2)(rot * (new Vector2(x1, y1) - center)) + center;
                                    Vector2 p12 = (Vector2)(rot * (new Vector2(x1, y2) - center)) + center;
                                    Vector2 p21 = (Vector2)(rot * (new Vector2(x2, y1) - center)) + center;
                                    Vector2 p22 = (Vector2)(rot * (new Vector2(x2, y2) - center)) + center;
                                    Handles.color = Color.white * 0.8f;
                                    const float dotDis = 4;
                                    Handles.DrawDottedLine(p11, p12, dotDis);
                                    Handles.DrawDottedLine(p11, p21, dotDis);
                                    Handles.DrawDottedLine(p22, p21, dotDis);
                                    Handles.DrawDottedLine(p22, p12, dotDis);

                                    Handles.color = ThemeColor;
                                    Vector2 oVec = outterBound.max - center;
                                    oVec.x *= Mathf.Sign(newVec.x);
                                    oVec.y *= Mathf.Sign(newVec.y);

                                    Handles.DrawLine(p21, oVec + center);

                                    oVec = new Vector2(Mathf.Sign(-oVec.y) * Mathf.Abs(oVec.x), Mathf.Sign(oVec.x) * Mathf.Abs(oVec.y));
                                    Handles.DrawLine(p22, oVec + center);

                                    oVec = new Vector2(Mathf.Sign(-oVec.y) * Mathf.Abs(oVec.x), Mathf.Sign(oVec.x) * Mathf.Abs(oVec.y));
                                    Handles.DrawLine(p12, oVec + center);

                                    oVec = new Vector2(Mathf.Sign(-oVec.y) * Mathf.Abs(oVec.x), Mathf.Sign(oVec.x) * Mathf.Abs(oVec.y));
                                    Handles.DrawLine(p11, oVec + center);

                                    Handles.DrawLine(p21, E.mousePosition);

                                    Handles.color = ThemeColor * 0.3f;
                                    Handles.DrawSolidArc(center, Vector3.forward, vec, Vector2.SignedAngle(vec, newVec), vec.magnitude);
                                }
                                EditorGUIUtility.AddCursorRect(r, MouseCursor.RotateArrow);
                                StyleBox(r, StlRotateHandle, "↻", isActive: GUIHotControl == id);
                                Handles.color = Color.white;
                                Handles.DrawWireDisc(r.center, Vector3.forward, rotateControlRadius);

                                DrawOutline(outterBound);
                                break;
                            case EventType.MouseDown:
                                if (r.Contains(E.mousePosition))
                                {
                                    SetVector2("RotateCenterVector", new Vector2(outterBound.xMax, outterBound.yMin) - outterBound.center);
                                    var center = tRect.InverseSample(outterBound.center, true);
                                    foreach (var g in selectedGroups)
                                    {
                                        g.orientationCache = g.Orientation;
                                        g.offsetCache = g.Bound.center - center;
                                    }

                                    SetVector2("RotateCenterUV", center);


                                    GUIHotControl = id;
                                    E.Use();
                                }
                                break;
                            case EventType.MouseUp:
                                if (GUIHotControl == id)
                                {
                                    // 旋转90度
                                    var center = tRect.InverseSample(outterBound.center, true);

                                    // 在这里旋转
                                    foreach (var g in selectedGroups)
                                    {
                                        var off = g.Bound.center - center;
                                        off = new Vector2(off.y, -off.x);
                                        g.OffsetInversed = center + off - g.RawBound.center;
                                        g.Orientation = NextOrientation(g.Orientation);
                                    }

                                    RenderCanvas();
                                    FulfillCanvas();
                                    Repaint();

                                    GUIHotControl = 0;
                                    E.Use();
                                }
                                else if (GUIHotControl == idDrag)
                                {
                                    FulfillCanvas();
                                    Repaint();
                                    GUIHotControl = 0;
                                    E.Use();
                                }
                                break;
                            case EventType.MouseDrag:
                                if (GUIHotControl == id) GUIHotControl = idDrag;
                                if (GUIHotControl == idDrag)
                                {
                                    float angle = Vector2.SignedAngle(GetVector2("RotateCenterVector"), E.mousePosition - outterBound.center);
                                    int angleStep = (Mathf.RoundToInt(angle / 90.0f) + 4) % 4;
                                    var center = GetVector2("RotateCenterUV");

                                    bool changed = false;
                                    foreach (var g in selectedGroups)
                                    {
                                        var off = angleStep switch
                                        {
                                            1 => new Vector2(g.offsetCache.y, -g.offsetCache.x),
                                            2 => new Vector2(-g.offsetCache.x, -g.offsetCache.y),
                                            3 => new Vector2(-g.offsetCache.y, g.offsetCache.x),
                                            _ => g.offsetCache,
                                        };
                                        g.OffsetInversed = center + off - g.RawBound.center;

                                        MeshOrientation o = (MeshOrientation)(((int)g.orientationCache + angleStep) % 4);

                                        if (g.Orientation != o)
                                        {
                                            g.Orientation = o;
                                            changed = true;
                                        }
                                    }
                                    if (changed) RenderCanvas();

                                    E.Use();
                                }
                                break;
                        }
                    }

                    // 尺寸控件
                    const float scaleControlRadius = 8;
                    const float scaleControlSize = scaleControlRadius + scaleControlRadius;
                    ScaleControl(new Rect(outterBound.xMin - scaleControlRadius, outterBound.yMin - scaleControlRadius, scaleControlSize, scaleControlSize), MouseCursor.ResizeUpLeft, LockPivotToCenter(new Vector2(outterBound.xMax, outterBound.yMax)));
                    ScaleControl(new Rect(outterBound.xMax - scaleControlRadius, outterBound.yMin - scaleControlRadius, scaleControlSize, scaleControlSize), MouseCursor.ResizeUpRight, LockPivotToCenter(new Vector2(outterBound.xMin, outterBound.yMax)));
                    ScaleControl(new Rect(outterBound.xMin - scaleControlRadius, outterBound.yMax - scaleControlRadius, scaleControlSize, scaleControlSize), MouseCursor.ResizeUpRight, LockPivotToCenter(new Vector2(outterBound.xMax, outterBound.yMin)));
                    ScaleControl(new Rect(outterBound.xMax - scaleControlRadius, outterBound.yMax - scaleControlRadius, scaleControlSize, scaleControlSize), MouseCursor.ResizeUpLeft, LockPivotToCenter(new Vector2(outterBound.xMin, outterBound.yMin)));
                    Vector2 LockPivotToCenter(Vector2 pivot) => (E.control || E.shift) ? outterBound.center : pivot;
                    float GetPivotLength(Vector2 pivot)
                    {
                        var off = E.mousePosition - pivot;
                        return Mathf.Sign(off.x) * off.magnitude;
                    }
                    void ScaleControl(Rect r, MouseCursor cursor, Vector2 pivot)
                    {
                        int id = GetControlID();
                        switch (E.type)
                        {
                            case EventType.Repaint:
                                StyleBox(r, "WinBtnInactiveMac", isActive: GUIHotControl == id);
                                EditorGUIUtility.AddCursorRect(r, cursor);
                                break;
                            case EventType.MouseDown:
                                if (r.Contains(E.mousePosition))
                                {
                                    GUIHotControl = id;
                                    var pivotUV = tRect.InverseSample(pivot, true);
                                    SetFloat("ScaleCenterLength", GetPivotLength(pivot));
                                    SetVector2("ScalePivotUV", pivotUV);
                                    SetVector2("ScalePivot", pivot);
                                    foreach (var g in selectedGroups)
                                    {
                                        g.scaleCache = g.Scale;
                                        g.offsetCache = g.Bound.center - pivotUV;
                                    }
                                    E.Use();
                                }
                                break;
                            case EventType.MouseUp:
                                if (GUIHotControl == id)
                                {
                                    GUIHotControl = 0;
                                    foreach (var g in selectedGroups)
                                    {
                                        if (g.Scale < 0)
                                        {
                                            g.Scale = -g.Scale;
                                            g.Orientation = NextOrientation(NextOrientation(g.Orientation));
                                        }
                                    }
                                    if (autoFulfill)
                                    {
                                        FulfillCanvas();
                                    }
                                    else
                                    {
                                        RenderCanvas();
                                    }
                                    Repaint();
                                    E.Use();
                                }
                                break;
                            case EventType.MouseDrag:
                                if (GUIHotControl == id)
                                {
                                    var pivotUV = GetVector2("ScalePivotUV");
                                    var factor = GetPivotLength(GetVector2("ScalePivot")) / GetFloat("ScaleCenterLength");
                                    foreach (var g in selectedGroups)
                                    {
                                        g.Scale = g.scaleCache * factor;
                                        g.OffsetInversed = g.offsetCache * factor + pivotUV - g.RawBound.center;
                                    }
                                    if (autoFulfill) RenderCanvas();
                                    E.Use();
                                }
                                break;
                        }
                    }
                }
            }

            Handles.color = Color.white;
        }
        void OnDockGUI()
        {
            static void DockSeparator() => Label("|".Color(Color.gray), GUILayout.ExpandWidth(false));

            // 左侧导航区
            if (EditingPack is null)
            {
                IceButton("主面板", true);
            }
            else
            {
                if (IceButton("主面板", false))
                {
                    EditingPack = null;
                    return;
                }
                Label("/", GUILayout.ExpandWidth(false));
                IceButton(EditingPack.name, true);
            }
            if (!meshPacks.Any()) return;

            Space();

            // 右侧功能区
            {

                if (EditingPack != null)
                {
                    var p = EditingPack;

                    if (selectedGroups.Count > 1)
                    {
                        if (IceButton("组合", "Ctrl + G") || GetKeyDownWithControl(KeyCode.G))
                        {
                            Group(p, selectedGroups.ToArray());
                        }
                        DockSeparator();
                    }
                    else if (selectedGroups.Count == 1)
                    {
                        var g = selectedGroups.First();
                        if (g.meshUnits.Count > 1)
                        {
                            if (IceButton("拆分", "Ctrl + T") || GetKeyDownWithControl(KeyCode.T))
                            {
                                Disgroup(p, g);
                            }
                            DockSeparator();
                        }
                    }
                }
                else
                {
                    using (GUICHECK)
                    {
                        IceToggle("检查尺寸", tooltip: "2");
                        if (GetKeyDown(KeyCode.Alpha2))
                        {
                            SetBool("检查尺寸", !GetBool("检查尺寸"));
                            GUI.changed = true;
                        }

                        if (GUIChanged)
                        {
                            RenderCanvas();
                            FulfillCanvas();
                            Repaint();
                        }
                    }
                }

                if (IceButton("重置视图", "1") || GetKeyDown(KeyCode.Alpha1))
                {
                    SetFloat("ViewScale", 0.9f * 1024 / ViewWidth);
                    SetVector2("ViewOffset", Vector2.zero);
                    if (GetBool("检查尺寸"))
                    {
                        SetBool("检查尺寸", false);
                        RenderCanvas();
                        FulfillCanvas();
                    }
                    Repaint();
                }
                DockSeparator();

                if (IceButton("反选", "Ctrl + I") || GetKeyDownWithControl(KeyCode.I)) SelectInverseGroups();
                if (IceButton("全选", "Ctrl + A") || GetKeyDownWithControl(KeyCode.A)) SelectAllGroups();
                DockSeparator();

                var mag = IceToggle("边缘吸附", true, "    ", tooltip: "边缘吸附模式，按住Alt切换");
                if (E.type == EventType.Repaint)
                {
                    var text = "⋑".Size(18);
                    if (mag) text = text.Color(ThemeColorExp);
                    StyleBox(GetLastRect().ApplyBorder(-2), StlLabel, text);
                }
                bool bCanOverlap = GetBool("框选相交/包含", false);
                IceToggle("框选相交/包含", textOverride: bCanOverlap ? "◈" : "▣", tooltip: $"设置框选模式，当前为{(bCanOverlap ? "相交模式" : "包含模式")}");
                Space(4);
            }
        }
        void OnInspectorGUI()
        {
            if (EditingPack is null)
            {
                if (inspectingPack is null)
                {
                    // 主面板
                    var count = selectedGroups.Count;

                    // 头部信息
                    using (GROUP)
                    {
                        using (HORIZONTAL)
                        {
                            Label("主面板".Bold().Color(ThemeColorExp));
                            Space();
                            Label((count > 0 ? $"当前选择的Mesh块:{count}" : "当前没有选择任何Mesh块").Color(Color.gray));
                        }
                        using (HORIZONTAL)
                        {
                            Label("画布尺寸".Color(Color.gray));
                            Space();
                            if (IceButton("128", CanvasWidth == 128)) CanvasWidth = 128;
                            if (IceButton("256", CanvasWidth == 256)) CanvasWidth = 256;
                            if (IceButton("512", CanvasWidth == 512)) CanvasWidth = 512;
                            if (IceButton("1024", CanvasWidth == 1024)) CanvasWidth = 1024;
                            if (IceButton("2048", CanvasWidth == 2048)) CanvasWidth = 2048;
                            if (IceButton("4096", CanvasWidth == 4096)) CanvasWidth = 4096;
                        }
                    }

                    // 当前选择的Groups
                    if (count > 0)
                    {
                        using (GROUP)
                        {
                            PreviewBox();
                            if (count > 1)
                            {
                                using (GROUP)
                                {
                                    using (Disable(true))
                                    {
                                        int uCount = 0;
                                        int tCount = 0;
                                        foreach (var g in selectedGroups)
                                        {
                                            uCount += g.meshUnits.Count;
                                            tCount += g.meshUnits.Sum(u => u.triangleList.Count);
                                        }
                                        _IntField("Units", uCount);
                                        _IntField("Triangles", tCount);
                                    }
                                    if (Button("导出FBX")) ExportToFbx(EditorUtility.SaveFilePanel("导出FBX", "", selectedGroups.First().name, "fbx"), selectedGroups);
                                }

                                using (GROUP)
                                {
                                    if (Button("重置"))
                                    {
                                        ResetGroup(selectedGroups);
                                        RenderCanvas();
                                        FulfillCanvas();
                                    }
                                }
                            }
                            else
                            {
                                // 单选
                                var g = selectedGroups.First();
                                MeshPack p = null;
                                foreach (var pp in meshPacks)
                                {
                                    if (pp.meshGroups.Contains(g)) p = pp;
                                }
                                // 详细信息
                                using (GROUP)
                                {
                                    using (Disable(true))
                                    {
                                        _IntField("Units", g.meshUnits.Count);
                                        _IntField("Triangles", g.meshUnits.Sum(u => u.triangleList.Count));
                                    }
                                    if (Button("导出FBX")) ExportToFbx(EditorUtility.SaveFilePanel("导出FBX", "", selectedGroups.First().name, "fbx"), selectedGroups);
                                }

                                using (GROUP)
                                {
                                    TextField("名字", ref g.name);
                                    Space(8);
                                    using (Disable(true))
                                    {
                                        using (HORIZONTAL)
                                        {
                                            var pos = g.Bound.min;
                                            Label("位置", GUILayout.Width(EditorGUIUtility.labelWidth - 2));
                                            _TextField($"{pos.x: 0.000}");
                                            _TextField($"{pos.y: 0.000}");
                                        }
                                        _FloatField("尺寸", g.Scale * ((float)CanvasWidth / (p.OutputTextureWidth)));
                                        _EnumPopup("方向", g.Orientation);
                                    }
                                    if (Button("重置尺寸"))
                                    {
                                        ResetGroup(selectedGroups, p, false);
                                        RenderCanvas();
                                        FulfillCanvas();
                                    }
                                    if (Button("重置"))
                                    {
                                        ResetGroup(selectedGroups, p);
                                        RenderCanvas();
                                        FulfillCanvas();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        using (GROUP) using (LabelWidth(84))
                        {
                            Label($"当前所有Mesh块数量: {MeshGroups.Count}");
                            using (HORIZONTAL)
                            {
                                int margin = Mathf.Max(0, IntField("排布间隔(像素)", 1));
                                if (IceButton("自动排布所有Mesh块"))
                                {
                                    AutoLayout(margin);
                                    RenderCanvas();
                                    FulfillCanvas();
                                }
                            }
                        }
                    }
                    using (GROUP)
                    {
                        using (HORIZONTAL)
                        {
                            IceGUIAuto.SectionHeader("导出贴图", labelOverride: "贴图");
                            // 填充进度
                            if (fulfillProgress != null)
                            {
                                var r = GetRect(StlIce, GUILayout.ExpandWidth(true));
                                r = StyleBox(r, StlIce, "画布填充中……", hasMargin: true);
                                StyleBox(new Rect(r) { width = r.width * (0.05f + 0.95f * fulfillProgress.Value) }, StlProgressBar, "画布填充中……");
                            }
                            else
                            {
                                Space();
                            }
                            if (!autoFulfill && IceButton("填充"))
                            {
                                FulfillCanvas(true);
                            }
                            using (GUICHECK)
                            {
                                IceToggle("自动填充", ref autoFulfill);
                                if (GUIChanged)
                                {
                                    if (autoFulfill) FulfillCanvas();
                                    else fulfillProgress = null;
                                }
                            }
                        }
                        using (Folder("导出贴图"))
                        {
                            Space(8);
                            TextureBoxComplex(canvasRT, showMip: false, infoOverride: $"{canvasRT.width}", bakgroundStyleOverride: StlOutputTextureBox, foldout: true);
                            Space(8);
                            if (Button("导出PNG")) ExportToPng(EditorUtility.SaveFilePanel("导出PNG", "", "OutputTexture", "png"));
                            if (Button("导出TGA")) ExportToTga(EditorUtility.SaveFilePanel("导出TGA", "", "OutputTexture", "tga"));
                        }
                    }
                }
                else
                {
                    // pack面板
                    var p = inspectingPack;
                    // 头部信息
                    using (GROUP)
                    {
                        using (HORIZONTAL)
                        {
                            Label(p.name.Bold().Color(ThemeColorExp));
                            Space();
                            if (IceButton("返回"))
                            {
                                inspectingPack = null;
                                return;
                            }
                        }
                        using (HORIZONTAL)
                        {
                            Label("资源组".Color(Color.gray));
                            Space();
                            if (IceButton("编辑"))
                            {
                                EditingPack = p;
                            }
                        }
                    }
                    // 详细信息
                    using (GROUP)
                    {
                        MeshPackBox(p, false);

                        // 操作
                        using (BOX) using (SectionFolder("危险操作".Color(Color.red), false))
                        {
                            if (Button("删除".Color(Color.red)) && Dialog())
                            {
                                inspectingPack = null;
                                selectedGroups.Clear();
                                meshPacks.Remove(p);
                            }
                        }
                    }
                }
            }
            else
            {
                if (selectedGroups.Any())
                {
                    // Group面板
                    if (selectedGroups.Count > 1)
                    {
                        // 多选

                        // 头部信息
                        using (GROUP)
                        {
                            using (HORIZONTAL)
                            {
                                Label($"{selectedGroups.Count}个Mesh块".Bold().Color(ThemeColorExp));
                                Space();
                                if (IceButton("返回"))
                                {
                                    selectedGroups.Clear();
                                }
                            }

                            Label("Mesh块".Color(Color.gray));
                        }
                        using (GROUP)
                        {
                            PreviewBox();
                            using (GROUP) using (Disable(true))
                            {
                                int uCount = 0;
                                int tCount = 0;
                                foreach (var g in selectedGroups)
                                {
                                    uCount += g.meshUnits.Count;
                                    tCount += g.meshUnits.Sum(u => u.triangleList.Count);
                                }
                                _IntField("Units", uCount);
                                _IntField("Triangles", tCount);
                            }
                            using (BOX) using (SectionFolder("危险操作".Color(Color.red), false))
                            {
                                if (Button(new GUIContent("删除".Color(Color.red), "Delete")) || GetKeyDown(KeyCode.Delete))
                                {
                                    foreach (var g in selectedGroups)
                                    {
                                        EditingPack.meshGroups.Remove(g);
                                        EditingPack.deletedGroups.Add(g);
                                    }
                                    hoveringGroups.Clear();
                                    SelectNoneGroup();
                                }
                            }
                        }
                    }
                    else
                    {
                        // 单选
                        var g = selectedGroups.First();
                        // 头部信息
                        using (GROUP)
                        {
                            using (HORIZONTAL)
                            {
                                Label(g.name.Bold().Color(ThemeColorExp));
                                Space();
                                if (IceButton("返回"))
                                {
                                    selectedGroups.Clear();
                                }
                            }
                            using (HORIZONTAL)
                            {
                                Label("Mesh块".Color(Color.gray));
                                Space();
                                Label("…编辑中".Color(Color.gray));
                            }
                        }
                        // 详细信息
                        using (GROUP)
                        {
                            PreviewBox();
                            using (GROUP)
                            {
                                using (Disable(true))
                                {
                                    _IntField("Units", g.meshUnits.Count);
                                    _IntField("Triangles", g.meshUnits.Sum(u => u.triangleList.Count));
                                }
                            }
                            using (GROUP)
                            {
                                TextField("名字", ref g.name);
                                using (Disable(true))
                                {
                                    Label($"UV Bound: x({g.RawBound.xMin: 0.00}, {g.RawBound.xMax: 0.00}) y({g.RawBound.yMin: 0.00}, {g.RawBound.yMax: 0.00})");
                                }
                            }
                            using (BOX) using (SectionFolder("危险操作".Color(Color.red), false))
                            {
                                if (Button(new GUIContent("删除".Color(Color.red), "Delete")) || GetKeyDown(KeyCode.Delete))
                                {
                                    EditingPack.meshGroups.Remove(g);
                                    EditingPack.deletedGroups.Add(g);
                                    hoveringGroups.Clear();
                                    SelectNoneGroup();
                                }
                            }
                        }
                    }
                }
                else
                {
                    // pack面板
                    var p = EditingPack;
                    // 头部信息
                    using (GROUP)
                    {
                        using (HORIZONTAL)
                        {
                            Label(p.name.Bold().Color(ThemeColorExp));
                            Space();
                            if (IceButton("返回"))
                            {
                                EditingPack = null;
                                inspectingPack = p;
                                return;
                            }
                        }
                        using (HORIZONTAL)
                        {
                            Label("资源组".Color(Color.gray));
                            Space();
                            Label("…编辑中".Color(Color.gray));
                        }
                    }
                    // 详细信息
                    using (GROUP)
                    {
                        MeshPackBox(p, true);
                    }
                }
            }
        }
    }
}