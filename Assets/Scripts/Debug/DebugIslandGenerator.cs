using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voronoi;

public class DebugIslandGenerator : MonoBehaviour
{
    public CountryRegion countryRegionPrefab;

    List<CountryRegion> m_countryRegionList;

    public int seed = 0;

    public int mapWidth = 10;
    public int mapHeight = 10;

    public int pointcount = 10;

    // Start is called before the first frame update
    void Start()
    {
        Random.InitState(seed);

        List<Vector2> points = new List<Vector2>();

        float halfWidth = (float)mapWidth / 2.0f;
        float halfHeight = (float)mapHeight / 2.0f;

        for (int i = 0; i < pointcount; i++)
        {
            Vector2 newPoint = Vector2.zero;

            newPoint.x = Random.Range(-halfWidth, halfWidth);
            newPoint.y = Random.Range(-halfHeight, halfHeight);

            points.Add(newPoint);
        }

        VoronoiDiagram vDiagram = new VoronoiDiagram(points, 100.0f, mapWidth, mapHeight);
        List<VoronoiShape> vShapes = vDiagram.vShapes;

        // Initialise Country List
        m_countryRegionList = new List<CountryRegion>();
        for (int i = 0; i < vShapes.Count; i++)
        {
            CountryRegion newCountry = Instantiate<CountryRegion>(countryRegionPrefab);
            m_countryRegionList.Add(newCountry);
        }

        // Assigin Shapes to country region
        for (int i = 0; i < m_countryRegionList.Count; i++)
        {
            CountryRegion countryRegion = m_countryRegionList[i];
            countryRegion.InitialiseShape(vShapes[i], m_countryRegionList);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        float halfWidth = (float)mapWidth / 2.0f;
        float halfHeight = (float)mapHeight / 2.0f;

        Vector2[] UV = new Vector2[4];
        UV[0] = new Vector2(-halfWidth, -halfHeight);
        UV[1] = new Vector2(-halfWidth, halfHeight);
        UV[2] = new Vector2(halfWidth, halfHeight);
        UV[3] = new Vector2(halfWidth, -halfHeight);

        for(int i = 0; i < 3; i++)
        {
            Gizmos.DrawLine(UV[i], UV[i + 1]);
        }
        Gizmos.DrawLine(UV[3], UV[0]);
    }

    private void OnValidate()
    {
        if(mapWidth < 1)
        {
            mapWidth = 1;
        }
        if (mapHeight < 1)
        {
            mapHeight = 1;
        }

        if(pointcount < 3)
        {
            pointcount = 3;
        }
    }
}
