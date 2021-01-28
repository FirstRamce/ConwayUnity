using UnityEngine;
using UnityEngine.UI;

public class MainCamera : MonoBehaviour
{
    private float _movementSpeed = 1.0f;
    private float _mouseWheelAccelerationMultiplier = 30f;
    private float _rotationSpeed = 0.5f;

    [SerializeField]
    private GameObject[] _BlockingFocusedObjects;
    // Start is called before the first frame update
    void Start()
    {
        if(_BlockingFocusedObjects == null || _BlockingFocusedObjects.Length == 0)
        {
            Debug.LogError("Blocking focused objects have not been assigned");
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (GameObject focusField in _BlockingFocusedObjects)
        {
            if (focusField.GetComponent<InputField>().isFocused)
            {
                return;
            }
        }

        transform.Rotate((_rotationSpeed * Input.GetAxis("Rotation")),0f,0f);
        transform.position += (Vector3.right * Input.GetAxis("Horizontal") + Vector3.forward * Input.GetAxis("Vertical") + Vector3.down * Input.GetAxis("Mouse ScrollWheel") * _mouseWheelAccelerationMultiplier) * _movementSpeed;
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, -4600f, 4600f), Mathf.Clamp(transform.position.y, 3f, 300f), Mathf.Clamp(transform.position.z, -4600f, 4600f));
        
        transform.eulerAngles = new Vector3(Mathf.Clamp(transform.eulerAngles.x, 1f, 90.0f), 0f, 0f);
        
    }

    public void cameraSpeedSliderChanged(Slider cameraSlider)
    {
        this._movementSpeed = cameraSlider.value;
    }

}
