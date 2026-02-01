using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class Letterbox16x9 : MonoBehaviour
{
    [SerializeField] private float targetAspect = 16f / 9f;

    private void Update()
    {
        var cam = GetComponent<Camera>();
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1f)
        {
            float y = (1f - scaleHeight) * 0.5f;
            cam.rect = new Rect(0f, y, 1f, scaleHeight);
        }
        else
        {
            float scaleWidth = 1f / scaleHeight;
            float x = (1f - scaleWidth) * 0.5f;
            cam.rect = new Rect(x, 0f, scaleWidth, 1f);
        }
    }
}
