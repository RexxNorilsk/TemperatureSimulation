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
    public float r = 0.1f;
    public int pipeSize = 256;
    public int outerLayer = 32;

    private float[] u, v, z;
    private ComputeBuffer uBuffer, vBuffer, zBuffer;
    private int kernelHandle = 0;
    public float time = 0f;
    public static TempShaderTest instance;
    

    public void Save()
    {
        uBuffer.GetData(u);
        string pathData = Path.Combine(Application.streamingAssetsPath, "test"+time+".csv");
        StreamWriter writer = new StreamWriter(pathData, false);
        writer.WriteLine(u[size * size / 2 + 0].ToString().Replace('.', ',')+ "; Размер области: " +size.ToString());
        writer.WriteLine(u[size * size / 2 + 1].ToString().Replace('.', ',')+ "; Радиус нагрева: " + radiusCenterHeater.ToString());
        writer.WriteLine(u[size * size / 2 + 2].ToString().Replace('.', ',')+ "; Температура нагрева: " + centerTemperature.ToString());
        writer.WriteLine(u[size * size / 2 + 3].ToString().Replace('.', ',')+ "; Проводимость: " + r.ToString());
        writer.WriteLine(u[size * size / 2 + 4].ToString().Replace('.', ',')+ "; Радиус проводника: " + pipeSize.ToString());
        writer.WriteLine(u[size * size / 2 + 5].ToString().Replace('.', ',')+ "; Толщина внешнего слоя: " + outerLayer.ToString());
        writer.WriteLine(u[size * size / 2 + 6].ToString().Replace('.', ',')+ "; Время: " + time.ToString());
        for (int i = 7; i < size; i++)
        {
            writer.WriteLine(u[size*size/2+i].ToString().Replace('.', ','));
        }
        writer.Close();
    }
    private void Awake()
    {
        instance = this;
        u = new float[size * size];
        v = new float[size * size];
        z = new float[size * size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                u[i * size + j] = 0;
                v[i * size + j] = u[i * size + j];
                z[i * size + j] = u[i * size + j];
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
        shader.SetFloat("CenterTemperature", centerTemperature);
        shader.SetFloat("alpha", r / (1 + 2 * r));
        shader.SetFloat("beta", (1 - 2 * r) / (1 + 2 * r));
        shader.SetFloat("pipeSize", pipeSize);
        shader.SetFloat("outerLayer", outerLayer+pipeSize);
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        shader.Dispatch(kernelHandle, renderTexture.width / 8, renderTexture.height / 8, 1);
        time = Time.realtimeSinceStartup;
        Graphics.Blit(renderTexture, destination);
    }
}

