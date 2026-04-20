using UnityEngine;

public class SkyboxRotation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;

    private void Update()
    {
        if (RenderSettings.skybox != null && RenderSettings.skybox.HasFloat("_Rotation"))
        {
            float rotation = RenderSettings.skybox.GetFloat("_Rotation");
            rotation += rotationSpeed * Time.deltaTime;
            RenderSettings.skybox.SetFloat("_Rotation", rotation);
        }
    }
}