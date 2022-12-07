using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(BoxCollider))]
public class MeshCreate : MonoBehaviour
{
    #region 1. 할당된 정보를 바탕으로 메쉬를 그리기 위해, 유니티에서 제공하는 컴포넌트
    /*
     * 메쉬에 대한 정보를 담고있는 객체
     * 1. 메쉬를 구성하는 정점의 정보(위치값)
     * 2. 각 정점의 UV좌표 데이터
     * 3. 각 정점이 가지는 법선 데이터
     * 4. 메쉬를 구성하는 삼각형의 구성정점 넘버
     */
    public Mesh mesh;

    public Mesh origine_mesh;

    public MeshFilter meshFilter;

    public Collider col;

    public MeshRenderer meshRenderer;
    #endregion

    //public bool isGizmo;

    public Vector3[] vertices;

    [Range(0, 100f)]
    public float size = 1f;
    float hsize = 10f;

    GameObject[] pos;
    public Texture m_texture;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        col = GetComponent<BoxCollider>(); //박스 아니어도 됨~

        Material material = new Material(Shader.Find("Standard"));
        material.SetTexture("_MainTex", m_texture);
        meshRenderer.material = material;

        origine_mesh = meshFilter.sharedMesh;

        if(origine_mesh)
        {
            pos = new GameObject[origine_mesh.vertexCount];
        }
        else
        {
            pos = new GameObject[4];
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if(origine_mesh)
        {
            for (int i = 0; i < origine_mesh.vertexCount;i++)
            {
                pos[i] = new GameObject("Pos");
                pos[i].transform.parent = this.transform;
                pos[i].transform.localPosition = origine_mesh.vertices[i];
                pos[i].AddComponent<CreateGizmo>();
            }
        }
        else 
        {
            hsize = 0.5f * size;

            for(int i =0; i<4; i++)
            {
                pos[i] = new GameObject("Pos");
                pos[i].transform.parent = this.transform;
                if (i == 0)
                {
                    pos[i].transform.localPosition = new Vector3(-hsize, hsize, 0f);
                }
                else if(i==1)
                {
                    pos[i].transform.localPosition = new Vector3(hsize, hsize, 0f);
                }
                else if(i==2)
                {
                    pos[i].transform.localPosition = new Vector3(hsize, -hsize, 0f);
                }
                else if(i==3)
                {
                    pos[i].transform.localPosition = new Vector3(-hsize, -hsize, 0f);
                }
                pos[i].AddComponent<CreateGizmo>();
            }
        }

        //// Quad의 정점 데이터 할당하기
        //vertices = new Vector3[]
        //{
        //    new Vector3(-hsize, hsize, 0f),
        //    new Vector3(hsize, hsize, 0f),
        //    new Vector3(hsize, -hsize, 0f),
        //    new Vector3(-hsize, -hsize, 0f)
        //};
    }

    // Update is called once per frame
    void Update()
    {
        mesh = new Mesh();
        //hsize=0.5f*size;

        if(origine_mesh)
        {
            vertices = new Vector3[origine_mesh.vertexCount];

            for(int i=0; i<origine_mesh.vertexCount; i++)
            {
                vertices[i] = pos[i].transform.localPosition;
            }
        }
        else
        {
            //Quad의 정점 데이터 할당하기 (앞에 배열 할 때 배웠쥬?^_^)
            vertices = new Vector3[]
            {
                pos[0].transform.localPosition,
                pos[1].transform.localPosition,
                pos[2].transform.localPosition,
                pos[3].transform.localPosition
            };
        }

        mesh.vertices = vertices;

        if(origine_mesh)
        {
            mesh.uv = origine_mesh.uv;
            mesh.normals = origine_mesh.normals;
            mesh.triangles = origine_mesh.triangles;
        }
        else
        {
            Vector2[] uv = new Vector2[]
            {
                //new Vector2(0, 0),
                //new Vector2(1, 0),
                //new Vector2(1, 1),
                //new Vector2(0, 1)
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(0, 0)
            };

            Vector3[] normals = new Vector3[]
            {
                new Vector3(0f, 0f, -1f),//0번 정점의 법선
                new Vector3(0f, 0f, -1f),//1번 정점의 법선
                new Vector3(0f, 0f, -1f),//2번 정점의 법선
                new Vector3(0f, 0f, -1f),//3번 정점의 법선
            };

            int[] triangles = new int[]
            {
                //순서가 꼬이면 삼각형이 거꾸로 그려지던지 하겠쥬,,,?
                0,1,2,
                2,3,0
            };
            mesh.uv = uv;
            mesh.normals = normals;
            mesh.triangles = triangles;
        }

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

        //OnDrawGizmos();

    }

    //private void OnDrawGizmos()
    //{
    //    if (!isGizmo) return;

    //    Gizmos.color = Color.blue;

    //    foreach(var pos in vertices)
    //    {
    //        Gizmos.DrawSphere(pos, 0.5f);
    //    }
    //}

    // PropertyDrawer

    [ContextMenu("Point Reset")]
    void FuncStart()
    {
        if(origine_mesh)
        {
            for(int i=0; i<origine_mesh.vertexCount;i++)
            {
                pos[i].transform.localPosition = origine_mesh.vertices[i];
            }
        }
        else
        {
            hsize = 0.5f * size;

            for(int i=0;i<4;i++)
            {
                if (i == 0)
                {
                    pos[i].transform.localPosition = new Vector3(-hsize, hsize, 0f);
                }
                else if (i == 1)
                {
                    pos[i].transform.localPosition = new Vector3(hsize, hsize, 0f);
                }
                else if (i == 2)
                {
                    pos[i].transform.localPosition = new Vector3(hsize, -hsize, 0f);
                }
                else if (i == 3)
                {
                    pos[i].transform.localPosition = new Vector3(-hsize, -hsize, 0f);
                }
            }
        }

        Debug.Log("Point Reset");
    }
}
