using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapInitialiser : MonoBehaviour
{
    public CountryRegion countryRegionPrefab;

    List<CountryRegion> m_countryRegionList;

    [System.Serializable]
    public struct CountryShape
    {
        public CountryRegion countryRegion { get; set; }
        public Transform[] points;
    }

    public CountryShape[] shapes;

    private void Awake()
    {
        m_countryRegionList = new List<CountryRegion>();
        
        for(int i = 0; i < shapes.Length; i++)
        {
            CountryRegion newCountry = Instantiate<CountryRegion>(countryRegionPrefab);
            m_countryRegionList.Add(newCountry);
            shapes[i].countryRegion = newCountry;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < shapes.Length; i++)
        {
            shapes[i].countryRegion.transform.position = Vector3.zero;
            Vector3[] positions = new Vector3[shapes[i].points.Length];
            for (int j = 0; j < shapes[i].points.Length; j++)
            {
                positions[j] = shapes[i].points[j].position;
            }
            shapes[i].countryRegion.InitialiseShape(positions);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
