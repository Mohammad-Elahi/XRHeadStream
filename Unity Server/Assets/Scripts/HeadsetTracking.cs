

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using TMPro; // Add this line to use TextMeshPro
using System.IO;

public class HeadsetTracking : MonoBehaviour
{
    // The path of the CSV file
    [SerializeField] private string filePath = "/storage/emulated/0/Download/HeadsetTrackingData.csv";
    //private string filePath = "A:/Mohammad/Project/MetaApp-AR/HeadsetTrackingData.csv";

    // Reference to the TextMeshPro text element
    //public TextMeshProUGUI trackingDataText; 

    void Start()
    {
        // Write the header to the CSV file
        string header = "Time,PositionX,PositionY,PositionZ,RotationX,RotationY,RotationZ,RotationW\n";
        File.WriteAllText(filePath, header);
    }

    void Update()
    {
        // Get the position and orientation of the headset
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;

          // Update the TextMeshPro text element with the tracking data
      //  trackingDataText.text = string.Format("Position: ({0}, {1}, {2})\nOrientation: ({3}, {4}, {5}, {6})",
     //    position.x, position.y, position.z, rotation.x, rotation.y, rotation.z, rotation.w);

        // Format the tracking data as a CSV row
        string csvRow = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}\n",
            Time.time, position.x, position.y, position.z, rotation.x, rotation.y, rotation.z, rotation.w);

        // Append the tracking data to the CSV file
        File.AppendAllText(filePath, csvRow);

      
    }
}   
