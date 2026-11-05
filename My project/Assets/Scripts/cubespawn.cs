using UnityEngine;

public class cubespawn : MonoBehaviour
{
    private string[] cubee = { "Куб 0", "Куб 1", "Куб 2", "Куб 3", "куб 4", "куб 5" };

    void Start()
    {
        for (int i = 0; i < cubee.Length; i++)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = cubee[i];

            Rigidbody rb = cube.AddComponent<Rigidbody>();
            rb.useGravity = false;

            cube.AddComponent<stolknovenie>();

            float spacing = 5f;
            cube.transform.position = new Vector3(i * spacing, 0, 0);
            cube.GetComponent<Renderer>().material.color = Random.ColorHSV();

        }
    }
}
