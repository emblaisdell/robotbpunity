using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Master : MonoBehaviour
{
    public const float CUP_DIST = 0.2f;//m
    public const float TABLE_LENGTH = 2f;//m
    public const int NUM_CUP_POS = 18;
    public const int INIT_CONFIG = 0x26fe;//standard triangle

    public static float SQRT3 = Mathf.Sqrt(3.0f);

    public GameObject cup;
    public int runningTeam = -1;

    public List<Robot> robots;

    public static Master master;

    public const float turnTime = 5f;//from docs
    public float turnTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        master = this;

        for(int team=0; team<2; team++)
        {
            string robotCodePath = UnityEditor.EditorUtility.OpenFilePanel("Load RISC-V Machine Code for Robot "+team.ToString(), "", "");
            byte[] robotCodeBytes = File.ReadAllBytes(robotCodePath);
            if(robotCodeBytes.Length % 4 != 0)
            {
                UnityEditor.EditorUtility.DisplayDialog("File Error", "File bytes must be divisible by 4 (there must be a whole number of 32bit words)", "Quit");
                Application.Quit();
            }
            if (robotCodeBytes.Length / 4 > RISCV.memLength)
            {
                UnityEditor.EditorUtility.DisplayDialog("File Warning", "File is too long, it will be truncated", "Continue");
            }
            int[] mem = new int[RISCV.memLength];
            int i = 0;
            while (i < robotCodeBytes.Length/4 && i < RISCV.memLength)
            {
                // get bytes
                byte[] bytes = robotCodeBytes.Skip(4 * i).Take(4).ToArray();
                //print(System.BitConverter.ToString(bytes));
                System.Array.Reverse(bytes); // little endian
                mem[i] = System.BitConverter.ToInt32(bytes, 0);
                //print(mem[i]);
                i++;
            }
            robots[team].Init(mem);
            SetUpCups(INIT_CONFIG, team);
        }


        runningTeam = 0;
        TeamStart(runningTeam);
        turnTimer = turnTime;
    }

    void SetUpCups(int config, int team)
    {
        Cup.RemoveTeamCups(team);

        bool flip = ((config & 1) == 0);

        for(int pos=1; pos<=NUM_CUP_POS; pos++)
        {
            if ((config & (1 << pos)) != 0) {
                AddCup(pos, flip, team);
            }
        }
    }

    void AddCup(int position, bool flip, int team)
    {
        float x = 0, y = 0;

        if(position>=1 && position <= 4)
        {
            x = (-1.5f + (float)(position - 1)) * CUP_DIST;
            y = 1.0f * SQRT3 * CUP_DIST;
        }
        else if (position >= 5 && position <= 7)
        {
            x = (-1f + (float)(position - 5)) * CUP_DIST;
            y = 0.5f * SQRT3 * CUP_DIST;
        }
        else if (position >= 8 && position <= 11)
        {
            x = (-1.5f + (float)(position - 8)) * CUP_DIST;
            y = 0f;
        }
        else if (position >= 12 && position <= 14)
        {
            x = (-1f + (float)(position - 12)) * CUP_DIST;
            y = -0.5f * SQRT3 * CUP_DIST;
        }
        else if (position >= 15 && position <= 18)
        {
            x = (-1.5f + (float)(position - 15)) * CUP_DIST;
            y = -1.0f * SQRT3 * CUP_DIST;
        }
        else
        {
            //we got a problem
        }

        if (flip)
        {
            Instantiate(cup, new Vector3((y + 0.5f * TABLE_LENGTH) * TeamDir(team), 0f, x), Quaternion.identity);
        }
        else
        {
            Instantiate(cup, new Vector3((x + 0.5f * TABLE_LENGTH) * TeamDir(team), 0f, y), Quaternion.identity);
        }
    }

    public void TeamStart(int team)
    {
        robots[team].StartUp();
    }

    public static int TeamDir(int team)
    {
        return (team == 0) ? 1 : -1;
    }

    public static int OtherTeam(int team)
    {
        return 1 - team;
    }

    // Update is called once per frame
    void Update()
    {
        turnTimer -= Time.deltaTime;

        if(turnTimer <= 0f)
        {
            if (robots[runningTeam].GetComponent<Robot>().running)
            {
                robots[runningTeam].ShutDown();
            }
        }
    }

    public void NextTurn()
    {
        turnTimer = turnTime;
        runningTeam = OtherTeam(runningTeam);
        robots[runningTeam].StartUp();
    }
}
