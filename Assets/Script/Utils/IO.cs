using UnityEngine;
using System.IO;
using UnityEngine.Experimental.Rendering;

namespace Qkyo.Utils
{
    public static class IO
    {
        public static void SaveVector3ArrayToFile (Vector3[] vector3Array, string fileName)
        {
            string data = "";
            foreach (Vector3 vector3 in vector3Array)
                data += vector3.x.ToString("0.000000").PadRight(10) + "," + vector3.y.ToString("0.000000").PadRight(10) + "," + vector3.z.ToString("0.000000").PadRight(10) + "\n";
            string filePath = fileName + ".txt";

            File.WriteAllText(filePath, data);
            Debug.Log("Vector3 array saved to: " + filePath);
        }

        public static void SaveVector4ArrayToFile(Vector4[] vector4Array, string fileName)
        {
            string data = "";
            foreach (Vector4 vector4 in vector4Array)
                data += vector4.x.ToString("0.000000").PadRight(10) + "," + vector4.y.ToString("0.000000").PadRight(10) + "," + vector4.z.ToString("0.000000").PadRight(10) + "," + vector4.w.ToString("0.000000").PadRight(10) + "\n";
            string filePath = fileName + ".txt";

            File.WriteAllText(filePath, data);
            Debug.Log("Vector4 array saved to: " + filePath);
        }

        public static void SaveIntegerArrayToFile(int[] intArray, string fileName)
        {
            string data = "";
            foreach (int perInt in intArray)
                data += perInt + "\n";
            string filePath = fileName + ".txt";

            File.WriteAllText(filePath, data);
            Debug.Log("Int array saved to: " + filePath);
        }

        unsafe public static Vector3[] ConvertPointerToArray(Vector3* vector3Pointer, int length)
        {
            Vector3[] vector3Array = new Vector3[length];

            for (int i = 0; i < length; i++)
                vector3Array[i] = *(vector3Pointer + i);

            return vector3Array;
        }

        unsafe public static int[] ConvertPointerToArray(int* ptr, int length)
        {
            int[] intArray = new int[length];

            for (int i = 0; i < length; i++)
                intArray[i] = *(ptr + i); // Copy value from pointer to array
            
            return intArray;
        }

        unsafe public static Vector4[] ConvertPointerToArray(Vector4* vector3Pointer, int length)
        {
            Vector4[] vector4Array = new Vector4[length];

            for (int i = 0; i < length; i++)
                vector4Array[i] = *(vector3Pointer + i);

            return vector4Array;
        }

        public static RenderTexture GetRenderTexture(byte[] m_byteArray)
        {
            // Create a new RenderTexture with the same dimensions as the byte array
            RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 0, GraphicsFormat.R32G32B32A32_SFloat);

            // Set the RenderTexture as the active render target
            RenderTexture.active = rt;

            // Create a new Texture2D and load the byte array data into it
            Texture2D texture2D = new Texture2D(Screen.width, Screen.height, GraphicsFormat.R32G32B32A32_SFloat, TextureCreationFlags.None);
            texture2D.LoadRawTextureData(m_byteArray);
            texture2D.Apply();

            // Use the Graphics API to set the texture data for the RenderTexture
            Graphics.Blit(texture2D, rt);

            // Reset the active render target
            RenderTexture.active = null;

            return rt;
        }
    }
}
