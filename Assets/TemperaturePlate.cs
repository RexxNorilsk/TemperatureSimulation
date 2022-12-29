using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TemperaturePlate : MonoBehaviour
{

    public GameObject prefab;
    public Transform target;
    


    public GameObject[,] objs;

    public float r = 0.0001f,
            tau = 0.1f,
            h = 0.1f;
    public float alpha = 0,
            beta = 0;
    public float Imax = 500;
    public int ndim = 25;
    float[,] u, v, z;
    private float Istep = 100000000;
    public Vector2 TempPoint = new Vector2();
    public float ConstantPointTemperature = 100;
    public int Radius = 10;

    void Start()
    {
        beta = (1 - 2 * r) / (1 + 2 * r);
        alpha = r / (1 + 2 * r);
        u = new float[ndim, ndim];
        v = new float[ndim, ndim];
        z = new float[ndim, ndim];
        objs = new GameObject[ndim, ndim];
        target.GetComponent<GridLayoutGroup>().constraintCount = ndim;
        target.GetComponent<GridLayoutGroup>().cellSize = new Vector2(1000/ndim, 1000/ndim);
        for (int i = 0; i < ndim; i++)
            for (int j = 0; j < ndim; j++)
                objs[i, j] = Instantiate(prefab, target);
        Init();
    }
    public void SetPointTemp(int x, int y, float temp) {
        for (int i = -Radius; i < Radius; i++)
        {
            for (int j = -Radius; j < Radius; j++)
            {
                u[i + x, j + y] = temp - (i * i + j * j);
                v[i + x, j + y] = temp - (i * i + j * j);
                z[i + x, j + y] = temp - (i * i + j * j);
            }
        }
    }
    public void Init()
    {
        for (int i = 0; i < ndim; i++)
        {
            for (int j = 0; j < ndim; j++)
            {
                u[i, j] = 0;
                v[i, j] = 0;
                z[i, j] = 0;
            }
        }

        SetPointTemp((int)TempPoint.x, (int)TempPoint.y, ConstantPointTemperature);

        Istep = 0;
    }
    
    
    void Update()
    {
        if (Istep <= Imax)
        {
            for (int i = 0; i < ndim; i++)
                for (int j = 0; j < ndim; j++)
                {
                    float[] coms = new float[] { 0, -1000, -1000, -1000 };
                    if (i - 1 >= 0 && j >= 0 && j < ndim) coms[0] = u[i - 1, j];
                    if (i + 1 < ndim && j >= 0 && j < ndim) coms[1] = u[i + 1, j];
                    if (j - 1 >= 0 && i >= 0 && i < ndim) coms[2] = u[i, j - 1];
                    if (j + 1 < ndim && i >= 0 && i < ndim) coms[3] = u[i, j + 1];

                    for (int k = 1; k < coms.Length; k++)
                    {
                        if (coms[k] != -1000)
                            coms[0] += coms[k];
                    }
                    z[i, j] = alpha * (coms[0]) + beta * v[i, j];
                }
            for (int i = 0; i < ndim; i++)
            {
                for (int j = 0; j < ndim; j++)
                {
                    v[i, j] = u[i, j];
                    u[i, j] = z[i, j];
                    SetPointTemp((int)TempPoint.x, (int)TempPoint.y, ConstantPointTemperature);
                    objs[i, j].GetComponent<Image>().color = new Color((float)u[i, j] / 255.0f, 0, 0);
                }
            }
            Istep++;
        }
        else {
            if (Istep++ == Imax + 1)
                Debug.Log("Final");
        }
    }
    
}
