using UnityEngine;

public class CoinController : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        transform.Rotate (new Vector3 (0, 0, 30) * Time.deltaTime);
    }
}
