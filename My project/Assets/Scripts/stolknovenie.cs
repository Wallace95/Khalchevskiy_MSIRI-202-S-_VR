using UnityEngine;

public class stolknovenie : MonoBehaviour
{
    [SerializeField] private float destroyDelay = 8f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject != gameObject)
        {
            Debug.Log("Столкновение!" + gameObject.name + " врезался в " + collision.gameObject.name);
           
            Destroy(gameObject, destroyDelay);
        }
    }
}
