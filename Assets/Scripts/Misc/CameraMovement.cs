using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour {
    public float CameraMovementSpeed = 50;
    public float CameraZoomSpeed = 5000;
    public float CameraRotateSpeed = 100;

    Vector3 clickLocation;
    Transform rotatePivot;


    void Start () {
	
	}
	
	void Update () {
        //Forward and sideways movement
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0) {
            transform.Translate(new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * CameraMovementSpeed * Time.deltaTime);
        }

        //Zoom in and out, in relation to the main cameras forward axis
        if (Input.GetAxis("Mouse ScrollWheel") != 0) {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));//Input.mousePosition);
            float zoomDistance = CameraZoomSpeed * Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime;
            Camera.main.transform.Translate(ray.direction * zoomDistance, Space.World);
        }

        //Rotate camera
        if (Input.GetMouseButtonDown(1)) {
            //Store where the user clicked
            clickLocation = Input.mousePosition;
            //Store where to rotate around
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            
        }

        if (Input.GetMouseButton(1)) {
            rotatePivot = transform.GetChild(0);
            //While holding down click, rotate left or right depending on if its less than or greater than clickLocation
            if (Input.mousePosition.x > clickLocation.x) {
                transform.Rotate(Vector3.up, 1 * CameraRotateSpeed * Time.deltaTime); //(new Vector3(0, 1, 0) * CameraRotateSpeed * Time.deltaTime, Space.World);
            }
            else if (Input.mousePosition.x < clickLocation.x) {
                //transform.Rotate(new Vector3(0, -1, 0) * CameraRotateSpeed * Time.deltaTime, Space.World);
                transform.Rotate(Vector3.up, -1 * CameraRotateSpeed * Time.deltaTime);
            }
            clickLocation = Input.mousePosition;
        }

        //Make sure camera pivot is always at y = 0
        Vector3 pivotPosition = transform.GetChild(0).position;
        transform.GetChild(0).position = new Vector3(pivotPosition.x, 0, pivotPosition.z);
    }
}
