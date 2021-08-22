using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voronoi;

public class MapInitialiser : MonoBehaviour
{
    public CountryRegion countryRegionPrefab;

    List<CountryRegion> m_countryRegionList;

    List<VoronoiShape> vShapes;

    private void Awake()
    {
        // Get Point List
        DebugCircle[] circles = FindObjectsOfType<DebugCircle>();
        List<Vector2> pointList = new List<Vector2>();
        foreach (DebugCircle circle in circles)
        {
            pointList.Add(circle.transform.position);
        }

        // Create Voronoi Diagram
        VoronoiDiagram vDiagram = new VoronoiDiagram(pointList, 100.0f);
        vShapes = vDiagram.vShapes;

        // Initialise Country List
        m_countryRegionList = new List<CountryRegion>();
        for (int i = 0; i < vShapes.Count; i++)
        {
            CountryRegion newCountry = Instantiate<CountryRegion>(countryRegionPrefab);
            m_countryRegionList.Add(newCountry);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Assigin Shapes to country region
        for(int i = 0; i < m_countryRegionList.Count; i++)
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
        
    }
}
