using System.Collections;
using UnityEngine;
using UnityEditor;

public class SelectMeshSave : Editor
{
    [MenuItem("GameObject/Select Mesh Save")]
    static void Copy()
    {
        Transform t = Selection.activeTransform;

        GameObject obj = t ? t.gameObject : null;

        if(obj)
        {
            //정보 : MeshFilter에서 점 찍고, Renderer에서 그려줌
            MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
            Mesh mesh = meshFilter ? meshFilter.sharedMesh : null;

            Mesh newMesh = new Mesh();
            newMesh.vertices = mesh.vertices;
            newMesh.uv = mesh.uv;
            newMesh.normals = mesh.normals;
            newMesh.triangles = mesh.triangles;
            newMesh.RecalculateNormals();
            newMesh.RecalculateBounds();

#if true 

            Vector3 diff = Vector3.Scale(newMesh.bounds.extents, new Vector3(0, 0, -1));
            obj.transform.position -= Vector3.Scale(diff, obj.transform.localScale);
            Vector3[] verts = newMesh.vertices;

            for (int i =0;i<verts.Length;i++)
            {
                verts[i] += diff;
            }

            newMesh.vertices = verts;
            newMesh.RecalculateBounds();
#endif

            string fileName = EditorUtility.SaveFilePanelInProject("Save Mesh", "mesh", "asset", "");
            AssetDatabase.CreateAsset(newMesh, fileName);
        }

    }

}
