using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Sirenix.OdinInspector;
using System.IO;
public class BlurImage : MonoBehaviour
{

    #if UNITY_EDITOR
    public Texture2D tex;
    public RawImage rawImage => GetComponent<RawImage>();
    Texture2D texture2D;
        //size kernal
    public int size = 3;
    public int integration = 1;
    [Button]
    public string GetImagePath(){
        if (rawImage != null)
        {
            string dataPath = Application.dataPath;
            //Remove the last /Assets of dataPath
            dataPath = dataPath.Substring(0, dataPath.Length - 6);


            string texturePath = AssetDatabase.GetAssetPath(tex);
            texturePath = dataPath + texturePath;
            //Debug.Log("Texture File Path: " + texturePath);
            return texturePath;
        }
        else
        {
            Debug.LogError("RawImage or Texture is not assigned.");
        }
        return null;
    }

    [Button]
    public void StartBlur(){
        string texturePath = GetImagePath();
        if(string.IsNullOrEmpty(texturePath)){
            return;
        }
        
        var textureTemp = new Texture2D(2, 2);
        textureTemp.LoadImage(System.IO.File.ReadAllBytes(texturePath));
        //textureTemp.ReadPixels(new Rect(0, 0, 2, 2), 0, 0);
        //textureTemp.Apply();
        //Debug.Log("textureTemp: "+textureTemp.width+"x"+textureTemp.height);
        var img = new Color[textureTemp.width, textureTemp.height];
        TexToArr(textureTemp, img);

        for (int i = 0; i < integration; i++)
        {
            img = GaussianBlur(img);
        }

        texture2D = new Texture2D(img.GetLength(0), img.GetLength(1));
        texture2D.wrapMode = TextureWrapMode.Repeat;
        texture2D.filterMode = FilterMode.Bilinear;
        ArrToTex(img, texture2D);
        texture2D.Apply();
        texture2D.name = rawImage.texture.name + "_blur";

        rawImage.texture = texture2D;
        //Resources.UnloadUnusedAssets();
        
    }
    [Button]
    void Dispose(){
        Resources.UnloadUnusedAssets();
    }

    [Button("Save Image")]
    void SaveImage()
    {
        if (texture2D != null)
        {
            // Define a path to save the image (change this as needed).
            string filePath = Application.dataPath + "/MainAsset/SavedImages/"+texture2D.name+".png";
            Debug.Log("filePath: "+filePath);
            // Encode the texture to a PNG format.
            byte[] bytes = texture2D.EncodeToPNG();

            // Write the encoded bytes to a file.
            File.WriteAllBytes(filePath, bytes);

            Debug.Log("Image saved to: " + filePath);
        }
        else
        {
            Debug.LogWarning("Texture is null. Please assign a texture before saving.");
        }
    }

    public Color[,] GaussianBlur(Color[,] colors)
    {
        
        int w = colors.GetLength(0);
        int h = colors.GetLength(1);

        Color[,] newArr;
        newArr = new Color[w, h];
        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {

                int num = 0;
                float sumA = 0;
                float sumR = 0;
                float sumG = 0;
                float sumB = 0;
                for (int y = i-size; y < i+size; y++)
                {
                    for (int x = j-size; x < j+size; x++)
                    {
                        try
                        {
                            var selectX = x;
                            var selectY = y;

                            if (selectX < 0)
                            {
                                selectX = w - Mathf.Abs(selectX);
                            }

                            if (selectY < 0)
                            {
                                selectY = h - Mathf.Abs(selectY);
                            }

                            if (selectX >= w)
                            {
                                selectX = selectX - w;
                            }

                            if (selectY >= h)
                            {
                                selectY = selectY - h;
                            }

                            var cor1 = colors[selectX, selectY];
                            sumR += cor1.r;
                            sumG += cor1.g;
                            sumB += cor1.b;

                            sumA += cor1.a;
                            num++;

                        }
                        catch
                        {
                            Debug.Log("Error at: " + x + " " + y);
                            continue;
                        }


                    }

                }

                newArr[j, i] = colors[j, i];
                newArr[j, i].r = sumR / num;
                newArr[j, i].g = sumG / num;
                newArr[j, i].b = sumB / num;
                
                newArr[j, i].a = sumA / num;


                //colors[j, i].a = sumA/ num;

            }

            
        }

        return newArr;
    }


    void ArrToTex(Color[,] colors, Texture2D texture2D)
    {
        int width = texture2D.width;
        int height = texture2D.height;

        Color[] texColors = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = x + y * width;
                texColors[index] = colors[x, y];
            }
        }

        texture2D.SetPixels(texColors);
    }
    void TexToArr(Texture2D texture2D, Color[,] colors)
    {
        int width = texture2D.width;
        int height = texture2D.height;

        Color[] texColors = texture2D.GetPixels();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = x + y * width;
                colors[x, y] = texColors[index];
            }
        }
    }
#endif

}
