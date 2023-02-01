using System;
using System.Collections.Generic;
using UnityEngine;

public class Chunk3DManager : MonoBehaviour
{
    ////观察者坐标
    public Transform viewer;
    ////观察的长度。也就是在多少方向生成
    public int viewDistance = 32;
    ////区块的长度
    public float chunkSize = 16;


    public Material mat;
    public bool showBoundsGizmo;
    public Color boundsColor;
    protected GameObject chunkHolder;
    protected List<Chunk3D> chunks;
    ////显示中的区块
    protected Dictionary<Vector3Int, Chunk3D> existingChunks;
    ////可回收的区块
    protected Queue<Chunk3D> recycleableChunks;
    public void Start()
    {
        recycleableChunks = new Queue<Chunk3D>();
        chunks = new List<Chunk3D>();
        existingChunks = new Dictionary<Vector3Int, Chunk3D>();



        InitVisibleChunks();
    }
    void Update()
    {
        InitVisibleChunks();
    }

    private void InitVisibleChunks()
    {
        if (chunks == null)
            return;
        CreateChunkHolder();
        Vector3 pos = viewer.position;
        Vector3Int viewercoord = GetChunkCoord(pos);
        //视线内生成多少个区块
        int maxchuncksInview = Mathf.CeilToInt(viewDistance / chunkSize);
        float sqrViewDis = viewDistance * viewDistance;

        for (int i = chunks.Count - 1; i >= 0; i--)
        {
            chunks[i].gameObject.SetActive(true);
            chunks[i].SetColl(chunks[i].coord == viewercoord);

            Chunk3D chunk = chunks[i];
            Vector3 center = CenterFromCoord(chunk.coord);
            Vector3 offset = pos - center;
            Vector3 o = new Vector3(Math.Abs(offset.x), Math.Abs(offset.y), Math.Abs(offset.z)) - Vector3.one * chunkSize * 0.5f;
            //获得摄像机与每一个区块的距离大小
            float sqr = new Vector3(Mathf.Max(o.x, 0), Mathf.Max(o.y, 0), Mathf.Max(o.z, 0)).sqrMagnitude;

            if (sqr > sqrViewDis)
            {
                existingChunks.Remove(chunk.coord);
                recycleableChunks.Enqueue(chunk);
                chunks.RemoveAt(i);
            }
        }

        ///更新区块
        for (int x = -maxchuncksInview; x <= maxchuncksInview; x++)
        {
            for (int y = -maxchuncksInview; y <= maxchuncksInview; y++)
            {
                for (int z = -maxchuncksInview; z <= maxchuncksInview; z++)
                {
                    //摄像机所在的区块周围
                    Vector3Int coord = new Vector3Int(x, y, z) + viewercoord;
                    if (existingChunks.ContainsKey(coord))
                    {
                        continue;
                    }
                    Vector3 center = CenterFromCoord(coord);
                    Vector3 viewerOffset = pos - center;
                    Vector3 o = new Vector3(Mathf.Abs(viewerOffset.x), Mathf.Abs(viewerOffset.y), Mathf.Abs(viewerOffset.z)) - Vector3.one * chunkSize * 0.5f;
                    float sqrDst = new Vector3(Mathf.Max(o.x, 0), Mathf.Max(o.y, 0), Mathf.Max(o.z, 0)).sqrMagnitude;
                    if (sqrDst <= sqrViewDis)
                    {
                        //chunk 所在的包围盒
                        Bounds bounds = new Bounds(center, chunkSize * Vector3.one);
                        if (IsVisibleFrom(bounds, Camera.main))
                        {
                            if (recycleableChunks.Count > 0)
                            {
                                Chunk3D chunk = recycleableChunks.Dequeue();
                                chunk.coord = coord;
                                existingChunks.Add(coord, chunk);
                                chunks.Add(chunk);
                                UpdateChunkMesh(chunk);
                            }
                            else
                            {
                                Chunk3D chunk = CreateChunk(coord);
                                chunk.coord = coord;
                                chunk.SetUp(mat);
                                existingChunks.Add(coord, chunk);
                                chunks.Add(chunk);
                                UpdateChunkMesh(chunk);
                            }
                        }

                    }
                }
            }
        }
    }
    protected void CreateChunkHolder()
    {
        // Create/find mesh holder object for organizing chunks under in the hierarchy
        if (chunkHolder == null)
        {
            if (GameObject.Find("chunkHoder"))
            {
                chunkHolder = GameObject.Find("chunkHoder");
            }
            else
            {
                chunkHolder = new GameObject("chunkHoder");
            }
        }
    }
    // <summary>
    // 获得摄像机所在的区块坐标
    // </summary>
    // <param name="pos"></param>
    // <returns></returns>
    private Vector3Int GetChunkCoord(Vector3 pos)
    {
        Vector3 p = pos / chunkSize;
        return new Vector3Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y), Mathf.RoundToInt(p.z));
    }
    /// <summary>
    /// 区块坐标转化为世界坐标
    /// </summary>
    /// <param name="c"></param>
    /// <returns>中心点的世界坐标</returns>
    protected Vector3 CenterFromCoord(Vector3Int c)
    {
        return new Vector3(c.x, c.y, c.z) * chunkSize;
    }
    // <summary>
    // 判断bounds是否在视锥体内
    // </summary>
    // <param name="bounds"></param>
    // <param name="camera"></param>
    // <returns></returns>
    protected bool IsVisibleFrom(Bounds bounds, Camera camera)
    {
        Plane[] plans = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(plans, bounds);
    }


    protected virtual void UpdateChunkMesh(Chunk3D chunk)
    {

    }


    protected Chunk3D CreateChunk(Vector3Int coord)
    {
        GameObject chunk = new GameObject($"Chunk ({coord.x}, {coord.y}, {coord.z})");
        chunk.transform.parent = chunkHolder.transform;
        Chunk3D newChunk = chunk.AddComponent<Chunk3D>();
        newChunk.coord = coord;
        return newChunk;
    }
    void OnDrawGizmos()
    {
        if (showBoundsGizmo)
        {
            Gizmos.color = Color.red;
            if (chunks != null)
            {
                foreach (var chunk in chunks)
                {
                    Gizmos.DrawWireCube(CenterFromCoord(chunk.coord), Vector3.one * chunkSize);
                }
            }
        }
    }

}
