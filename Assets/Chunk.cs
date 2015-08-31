using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class Chunk : MonoBehaviour
{

    public class BlockList
    {   
        private Dictionary<int,Block> blockdict = new Dictionary<int,Block>();
        public BlockList()
        {

        }

        public Block Get(int x,int y, int z)
        {
            int lookupkey = x + y*chunkSize + z*chunkSize*chunkSize;
            if (blockdict.ContainsKey(lookupkey))
            {
                return blockdict[lookupkey];
            }
            else
            {
                return null;
            }
        }


        public void Set(Block block, int x,int y, int z)
        {
            int lookupkey = x + y*chunkSize + z*chunkSize*chunkSize;
            if (blockdict.ContainsKey(lookupkey))
            {
                blockdict.Remove(lookupkey);
                blockdict.Add(lookupkey,block);
            }
            else
            {
                blockdict.Add(lookupkey,block);
            }
        }
        
    
    }

    private BlockList blocks = new BlockList();

    public static int chunkSize = 16;
    public bool update = true;

    MeshFilter filter;
    MeshCollider coll;

    public World world;
    public WorldPos pos;
    public Block airdefault;

    void Start()
    {
        filter = gameObject.GetComponent<MeshFilter>();
        coll = gameObject.GetComponent<MeshCollider>();
        airdefault = new BlockAir();
    }

    //Update is called once per frame
    void Update()
    {
        if (update)
        {
            update = false;
            UpdateChunk();
        }
    }

    public Block GetBlock(int x, int y, int z)
    {
        if (InRange(x) && InRange(y) && InRange(z))
        {
            if (blocks.Get(x, y, z)== null)
            {
                return airdefault;
            }
            else
            { 
                return blocks.Get(x, y, z);
            }
        }
        return world.GetBlock(pos.x + x, pos.y + y, pos.z + z);
    }

    public static bool InRange(int index)
    {
        if (index < 0 || index >= chunkSize)
            return false;

        return true;
    }

    public void SetBlock(int x, int y, int z, Block block)
    {
        if (InRange(x) && InRange(y) && InRange(z))
        {
            blocks.Set(block,x, y, z) ;
        }
        else
        {
            world.SetBlock(pos.x + x, pos.y + y, pos.z + z, block);
        }
    }

    // Updates the chunk based on its contents
    void UpdateChunk()
    {
        MeshData meshData = new MeshData();
        int y = 7;
        for (int x = 0; x < chunkSize; x++)
        {
            //for (int y = 0; y < chunkSize; y++)
            //{
                for (int z = 0; z < chunkSize; z++)
                {
                    meshData = blocks.Get(x, y, z).Blockdata(this, x, y, z, meshData);
                }
            //}
        }

        RenderMesh(meshData);
    }

    // Sends the calculated mesh information
    // to the mesh and collision components
    void RenderMesh(MeshData meshData)
    {
        filter.mesh.Clear();
        filter.mesh.vertices = meshData.vertices.ToArray();
        filter.mesh.triangles = meshData.triangles.ToArray();

        filter.mesh.uv = meshData.uv.ToArray();
        filter.mesh.RecalculateNormals();

        coll.sharedMesh = null;
        Mesh mesh = new Mesh();
        mesh.vertices = meshData.colVertices.ToArray();
        mesh.triangles = meshData.colTriangles.ToArray();
        mesh.RecalculateNormals();

        coll.sharedMesh = mesh;
    }

}