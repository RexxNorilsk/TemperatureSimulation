using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ant : MonoBehaviour
{
    public GameObject prefabPoint, prefabEdge;
    public Transform targetPoints, targetEdges;
    public LineRenderer[,] edges;
    public float colorCoef = 5f;
    public float timeStep = 0.5f;

    public static int MaxTowns = 100; //(* ����� �������*)
    public static int MaxAnts = 100; //(* ����� ��������*)
    public double Alfa = 1.0; //(* ��� ��������*)
    public double Beta = 5.0; //(* ���������*)
    public double Rho = 0.5; //(* ���������*)
    public int Q = 100; //(* ���������*)
    public double InitOdor = 1.0 / MaxTowns; //(* ��������� �����*)
    public static int MaxWay = 100; //(* ������ ���������*)
    public int MaxTour = MaxTowns * MaxWay; //(* ������ ����*)
    public int MaxTime = 20 * MaxTowns; //(* ������ ��������*)
    public int[] TVector = new int[MaxTowns];

    public TTown[] Towns; // (* ������*)
    public TAnt[] Ants; //(* �������*)
    
    public double[,] DistMap; //  (* ����� ����������*)
    public double[,] OdorMap; // (* ����� �������*)

    public TAnt Best; // (* ������ ����*)

    public long CurTime; // (* ������� �����*)

    public System.Random r;

    private bool run = false;

    public void UpdateColors()
    {

        for (int i = 0; i < MaxTowns; i++)
        {
            for (int j = 0; j < MaxTowns; j++)
            {
                if (i > j) continue;
                Color c = new Color();
                c.r = ((float)OdorMap[i,j] * colorCoef)/255;
                c.a = 1f;
                edges[i, j].startColor = c;
                edges[i, j].endColor = c;
            }
        }
    }
    

    //(* ������������� �������*)
    public void MakeTowns()
    {

        
        double xd, yd;
        //(* �������� �������*)
        for (int i = 0; i < MaxTowns; i++)
        {
            if (Towns[i] != null)
                Destroy(Towns[i].point);
            Towns[i] = new TTown();

            Towns[i].x = r.Next(1, MaxWay) + 1;//i * 10;
            Towns[i].y = r.Next(1, MaxWay) + 1;//i * 10;
                           //113.13708498985 - ������ �������
            Towns[i].point = Instantiate(prefabPoint, targetPoints);
            Towns[i].point.GetComponent<RectTransform>().position = new Vector2((float)Towns[i].x*10, (float)Towns[i].y*10);
            Towns[i].point.transform.GetChild(0).GetComponent<Text>().text = i.ToString(); ;
            
            
            
            for (int j = 0; j < MaxTowns; j++)
            {
                OdorMap[i, j] = InitOdor;
            }
        }

        for (int i = 0; i < MaxTowns; i++)
            for (int j = 0; j < MaxTowns; j++)
                if (edges!= null && edges[i, j] != null)
                    Destroy(edges[i, j].gameObject);

        edges = new LineRenderer[MaxTowns, MaxTowns];
        
        for (int i = 0; i < MaxTowns; i++)
        {
            for (int j = 0; j < MaxTowns; j++)
            {
                if (i > j) continue;
                edges[i, j] = Instantiate(prefabEdge, targetEdges).GetComponent<LineRenderer>();
                edges[i, j].SetPosition(0, new Vector3((float)Towns[i].point.transform.position.x, (float)Towns[i].point.transform.position.y, 0));
                edges[i, j].SetPosition(1, new Vector3((float)Towns[j].point.transform.position.x, (float)Towns[j].point.transform.position.y, 0));
                Color c = new Color();
                c.r = (float)InitOdor * colorCoef/255;
                c.a = 1f;
                edges[i, j].startColor = c;
                edges[i, j].endColor = c;
            }

        }

        // (* ���������� ���������� ����� ��������*)
        for (int i = 0; i < MaxTowns; i++)
        {
            DistMap[i, i] = 0;
            for (int j = i + 1; j < MaxTowns; j++)
            {
                xd = Towns[i].x - Towns[j].x;
                yd = Towns[i].y - Towns[j].y;
                DistMap[i, j] = System.Math.Sqrt(xd * xd + yd * yd);
                DistMap[j, i] = DistMap[i, j];
            }
        }

    }
    public void Init()
    {
        StopAllCoroutines();
        Best = new TAnt();
        CurTime = 0;
        Best.length = MaxTour;
        MakeTowns();

        MakeAnts(0);


        StartCoroutine(Step());
        
    }
    public void Start()
    {
        Towns = new TTown[MaxTowns];
        Ants = new TAnt[MaxAnts];
        DistMap = new double[MaxTowns, MaxTowns];
        OdorMap = new double[MaxTowns, MaxTowns];
        r = new System.Random();
        Init();   
    }

    IEnumerator Step() {
        yield return new WaitForSeconds(timeStep);
        if (CurTime < MaxTime)
        {
            if (!AntsMoving())
            {
                UpdateOdors();
                MakeAnts(1);
                Debug.Log("�����=" + CurTime + " ����=" + Best.length);
            }
            CurTime++;
            StartCoroutine(Step());
        }
        else
            Debug.Log("������ ����=" + Best.length);
    }

    //(*0/1-��������/��������� ��������*)
    public void MakeAnts(int mode)
    {
        int k = 0; //(*������� ����� *)
        for (int i = 0; i < MaxAnts; i++)
        {
            if ((mode > 0) && Ants[i] != null && (Ants[i].length < Best.length)) Best = Ants[i];
            Ants[i] = new TAnt();
            Ants[i].tekTown = k++;
            if (k > MaxTowns) k = 0;
            Ants[i].tabu = new int[MaxTowns];
            Ants[i].path = new int[MaxTowns];
            for (int j = 0; j < Ants[i].path.Length; j++)
            {
                Ants[i].path[j] = -1;
            }
            Ants[i].tabu[Ants[i].tekTown] = 1;
            Ants[i].path[0] = Ants[i].tekTown;
            Ants[i].numTowns = 1;
            Ants[i].length = 0;
        }
    }

    //(* ����������� �����*)
    public double Chance(int i, int j)
    {
        double res = System.Math.Pow(OdorMap[i, j], Alfa) *
                        System.Math.Pow(1.0 / DistMap[i, j], Beta);
        return res;
    }

    //(* ����� ���������� ������*)
    public int NextTown(int k)
    {
        int j; //��������� �����
        double d; // ��������� �����������
        double p = 1; //(*������� ����������� *)

        d = 0;
        int i = Ants[k].tekTown; //������� �����

        for (j = 0; j < Ants[k].tabu.Length; j++)
            if (Ants[k].tabu[j] == 0)
                d = d + Chance(i, j);
        j = MaxTowns - 1;


        if (d != 0)
        {
            do
            {
                j++;
                if (j >= MaxTowns) j = 0;
                if (i != j) p = Chance(i, j) / d;
            } while ((Ants[k].tabu[j] != 0) || (r.NextDouble() >= p));
        }

        return j;
    }

    // (* ����������� ��������*)
    public bool AntsMoving()
    {
        int k; //(* ����� �������*)
        bool m = false; //(* ���� �����������*)
        int next; //(* ��������� �����*)

        for (k = 0; k < MaxAnts; k++)
        {
            if (Ants[k].numTowns < MaxTowns)
            {
                next = NextTown(k);
                Ants[k].path[Ants[k].numTowns++] = next;
                Ants[k].tabu[next] = 1;
                Ants[k].length = Ants[k].length + DistMap[Ants[k].tekTown, next];
                if (Ants[k].numTowns == MaxTowns)
                    Ants[k].length = Ants[k].length + DistMap[Ants[k].path[MaxTowns - 1], Ants[k].path[0]];
                Ants[k].tekTown = next;
                m = true;

            }
        }
        return m;
    }


    public void UpdateOdors()
    {
        int i, j, k, ant;
        for (i = 0; i < MaxTowns; i++)
        {
            for (j = 0; j < MaxTowns; j++)
            {
                if (i != j)
                {
                    OdorMap[i, j] = OdorMap[i, j] * (1 - Rho);
                    if (OdorMap[i, j] < InitOdor)
                        OdorMap[i, j] = InitOdor;
                }
            }
        }

        for (ant = 0; ant < MaxAnts; ant++)
        {
            for (k = 0; k < MaxTowns; k++)
            {
                i = Ants[ant].path[k];
                if (k < MaxTowns)
                    j = Ants[ant].path[k];
                else
                    j = Ants[ant].path[0];
                OdorMap[i, j] = OdorMap[i, j] + Q / Ants[ant].length;
                OdorMap[j, i] = OdorMap[i, j];
            }
        }

        UpdateColors();
    }
}
public class TTown
{
    public double x;
    public double y;
    public GameObject point;
}

public class TAnt
{
    public int tekTown;
    public int[] tabu;
    public int[] path;
    public int numTowns;
    public double length;
}