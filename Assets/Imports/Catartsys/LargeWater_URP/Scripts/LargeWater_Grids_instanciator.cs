using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Large_Water_Grids_instanciator : MonoBehaviour
{
    public Material material;
    public int nbRows=0;
    public int nbLines=0;


    private int oldRows;
    private int oldLines;
    private Vector3 localPos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(nbRows != oldRows || nbLines != oldLines)
        {
            localPos = transform.position;
            //Delete all existing
            DeleteAllChildren();
            //Instanciate new planes
            InstanciateGrids(nbRows, nbLines);
            oldRows = nbRows;
            oldLines = nbLines;
        }
    }

    void DeleteAllChildren()
    {

        foreach (Transform child in this.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    void InstanciateGrids(int nbRows, int nbLines)
    {
        for (int r=0; r<nbRows; r++)
        {
            for (int l = 0; l < nbLines; l++)
            {
                GameObject grid = GameObject.CreatePrimitive(PrimitiveType.Plane);
                grid.transform.SetParent(this.transform);
                grid.transform.position = new Vector3(l * 10.0f + localPos.x, localPos.y, r*10.0f + localPos.z);
                Renderer gridRenderer = grid.GetComponent<Renderer>();
                gridRenderer.material = material;
            }
        }
    }


}
