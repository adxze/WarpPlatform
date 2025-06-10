using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    Transform cam; // Main Camera
    Vector3 camStartPos;
    float distance; // Distance between the camera start position and current position

    GameObject[] backgrounds;
    Material[] mat;
    float[] backSpeed;

    float farthestBack;

    [Range(0.01f, 0.05f)]
    public float parallaxSpeed;

    void Start()
    {
        cam = Camera.main.transform;
        camStartPos = cam.position;

        int backCount = transform.childCount;
        mat = new Material[backCount];
        backSpeed = new float[backCount];
        backgrounds = new GameObject[backCount];

        for (int i = 0; i < backCount; i++)
        {
            backgrounds[i] = transform.GetChild(i).gameObject;
            mat[i] = backgrounds[i].GetComponent<Renderer>().material;
        }

        BackSpeedCalculate(backCount);
    }

    void BackSpeedCalculate(int backCount)
    {
        for (int i = 0; i < backCount; i++) // Find the farthest background
        {
            float zDiff = backgrounds[i].transform.position.z - cam.position.z;
            if (zDiff > farthestBack)
            {
                farthestBack = zDiff;
            }
        }

        for (int i = 0; i < backCount; i++) // Set the speed of backgrounds
        {
            float zDiff = backgrounds[i].transform.position.z - cam.position.z;
            backSpeed[i] = 1 - (zDiff / farthestBack);
        }
    }

    private void LateUpdate()
    {
        distance = cam.position.x - camStartPos.x;

        // Make the background follow camera in both X and Y
        transform.position = new Vector3(cam.position.x, cam.position.y, transform.position.z);
        
        for (int i = 0; i < backgrounds.Length; i++)
        {
            float speed = backSpeed[i] * parallaxSpeed;

       
            mat[i].SetTextureOffset("_MainTex", new Vector2(distance, 0) * speed);
        }
    }
}
