using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TempShaderTest : MonoBehaviour
{
    public ComputeShader shader;
    public RenderTexture renderTexture;
    public int size = 256;
    public int radiusCenterHeater = 5;
    public float centerTemperature = 600;
    public float borderTemperature = 128;
    public float r = 0.1f;
    public int pipeSize = 256;
    public int outerLayer = 32;
    public Vector2 heater;

    private float[] u, v, z;
    private ComputeBuffer uBuffer, vBuffer, zBuffer;
    private int kernelHandle = 0;
    private float[] uCurrent;
    public float time = 0f;
    public static TempShaderTest instance;
    private bool run = false;
    public float[] GetNowDataU()
    {
        uBuffer.GetData(u);
        for (int i = 0; i < size; i++)
            uCurrent[i] = u[size * size / 2 + i];
        return uCurrent;
    }
    public void Save()
    {
        
        uBuffer.GetData(u);
        string pathData = Path.Combine(Application.streamingAssetsPath, "test"+time+".csv");
        StreamWriter writer = new StreamWriter(pathData, false);
        writer.WriteLine(u[size * size / 2 + 0].ToString().Replace('.', ',')+ "; ������ �������: " +size.ToString());
        writer.WriteLine(u[size * size / 2 + 1].ToString().Replace('.', ',')+ "; ������ �������: " + radiusCenterHeater.ToString());
        writer.WriteLine(u[size * size / 2 + 2].ToString().Replace('.', ',')+ "; ����������� �������: " + centerTemperature.ToString());
        writer.WriteLine(u[size * size / 2 + 3].ToString().Replace('.', ',')+ "; ������������: " + r.ToString());
        writer.WriteLine(u[size * size / 2 + 4].ToString().Replace('.', ',')+ "; ������ ����������: " + pipeSize.ToString());
        writer.WriteLine(u[size * size / 2 + 5].ToString().Replace('.', ',')+ "; ������� �������� ����: " + outerLayer.ToString());
        writer.WriteLine(u[size * size / 2 + 6].ToString().Replace('.', ',')+ "; �����: " + time.ToString());
        for (int i = 7; i < size; i++)
        {
            writer.WriteLine(u[size*size/2+i].ToString().Replace('.', ','));   
        }
        
        writer.Close();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            StartCalc();
    }

    bool CheckCenter(int x, int y, int radius) {
        if (Mathf.Sqrt((x-heater.x)* (x - heater.x) + (y - heater.y)* (y - heater.y)) < radius) {
            return true;
        }
        return false;
    }

    public void Dots() {
        int dots = 0;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++){
                if(CheckCenter(i, j, pipeSize))dots++;
            }
        }
    
        Debug.Log(dots);
    }
    private void StartCalc()
    {
        Dots();
        uCurrent = new float[size];
        instance = this;
        u = new float[size * size];
        v = new float[size * size];
        z = new float[size * size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                u[i * size + j] = 0;
                v[i * size + j] = 0;
                z[i * size + j] = 0;
            }
        }
        if (pipeSize > size)
            pipeSize = size;
        renderTexture = new RenderTexture(size, size, 24);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();
        uBuffer = new ComputeBuffer(u.Length, sizeof(float));
        vBuffer = new ComputeBuffer(v.Length, sizeof(float));
        zBuffer = new ComputeBuffer(z.Length, sizeof(float));

        kernelHandle = shader.FindKernel("CSMain");
        shader.SetTexture(kernelHandle, "Result", renderTexture);
        shader.SetFloat("Resolution", renderTexture.width);
        shader.SetInt("size", size);
        shader.SetInt("radiusCenterHeater", radiusCenterHeater);
        uBuffer.SetData(u);
        vBuffer.SetData(v);
        zBuffer.SetData(z);
        shader.SetBuffer(0, "u", uBuffer);
        shader.SetBuffer(0, "v", vBuffer);
        shader.SetBuffer(0, "z", zBuffer);
        shader.SetFloat("xCenter", size / 2);
        shader.SetFloat("yCenter", size / 2);
        shader.SetFloat("xHeater", heater.x);
        shader.SetFloat("yHeater", heater.y);
        shader.SetFloat("CenterTemperature", centerTemperature);
        shader.SetFloat("BorderTemperature", borderTemperature);
        shader.SetFloat("alpha", r / (1 + 2 * r));
        shader.SetFloat("beta", (1 - 2 * r) / (1 + 2 * r));
        shader.SetFloat("pipeSize", pipeSize);
        shader.SetFloat("outerLayer", outerLayer+pipeSize);
        run = true;
        

    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (run)
        {
            shader.Dispatch(kernelHandle, renderTexture.width / 8, renderTexture.height / 8, 1);
            time = Time.realtimeSinceStartup;
            Graphics.Blit(renderTexture, destination);
        }
    }
}

//u[pos] = BorderTemperature;