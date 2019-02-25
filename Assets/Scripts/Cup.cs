using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cup : MonoBehaviour
{
    public int team;

    public static List<Cup> cups = new List<Cup>();

    // Start is called before the first frame update
    void Start()
    {
        cups.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void RemoveTeamCups(int removeTeam)
    {
        foreach(Cup cup in cups)
        {
            if(cup.team == removeTeam)
            {
                Destroy(cup.gameObject);
            }
        }
    }
}
