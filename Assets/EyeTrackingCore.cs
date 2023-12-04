using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ViveSR.anipal.Eye;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.XR;
using TMPro;
using System.Linq;

public class EyeTracking : MonoBehaviour
{
    public EyeData data = new EyeData();
    Vector3 origin = new Vector3(0, 0, -2);
    readonly List<string> HitLocations2D = new List<string>();
    public GameObject player;
    public GameObject Hitter;
    StringBuilder sb = new StringBuilder();
    private readonly char[] toTrim = { '(', ')' };

    // Start is called before the first frame update
    [Obsolete]
    void Start()
    {
        sb.Clear();
        int runnum = PlayerPrefs.GetInt("Run Number");
        runnum = runnum + 1;
        PlayerPrefs.SetInt("Run Number", runnum);
        UnityEngine.XR.InputTracking.disablePositionalTracking = true; 

        List<XRDisplaySubsystem> displaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(displaySubsystems);
        if (!XRSettings.enabled)
        {
            XRSettings.enabled = true;
        }
        XRSettings.occlusionMaskScale = 2f;
        XRSettings.useOcclusionMesh = false;
    }

    // Update is called once per frame
    void Update()
    {
        ViveSR.Error error = SRanipal_Eye_API.GetEyeData(ref data);

        float x;
        float y;
        float z;
        Vector3 location = Vector3.zero;
        float ConvergeDist = 0.0f;
        var left = new SingleEyeData();
        var right = new SingleEyeData();
        var combined = new CombinedEyeData();

        if (error == ViveSR.Error.WORK)
        {
            left = data.verbose_data.left;
            right = data.verbose_data.right;
            combined = data.verbose_data.combined;

            x = combined.eye_data.gaze_direction_normalized.x;
            y = combined.eye_data.gaze_direction_normalized.y;
            z = combined.eye_data.gaze_direction_normalized.z;

            var tuple = CalculateConvergenceDistanceAndCoords(origin, new Vector3(-x, y, z), 1 << 3);

            location = tuple.Item1;
            ConvergeDist = tuple.Item2;

            if (Camera.main.gameObject.activeInHierarchy)
            {
                HitLocations2D.Add(string.Join(",", 0.0f, 0.0f));
            }
            else
            {
                HitLocations2D.Add(string.Join(",", 0.0f, 0.0f));
            }
            var alpha = Vector3.SignedAngle(player.transform.position, player.transform.position + new Vector3(-x, y, z), player.transform.forward) * Mathf.Deg2Rad;
            var hypo = 10.0f / Mathf.Cos(alpha);
            //Hitter.transform.position = new Vector3(-x * hypo, y * hypo, z * hypo);//-0.05f);
            Hitter.transform.position = location;
        }
        else
        {
            print("eye tracking error!");
            x = 0.0f;
            y = 0.0f;
            z = 0.0f;

            left.pupil_diameter_mm = 0.0f;
            left.eye_openness = 0.0f;
            right.pupil_diameter_mm = 0.0f;
            right.eye_openness = 0.0f;
        }

        string DateFormat = "HHmmss";
        string timehoursec= DateTime.Now.ToString(DateFormat);
        string milisec = DateTime.UtcNow.Millisecond.ToString();
        sb.Append(string.Format("{0},{1},{2},{3},{4},{5},{6}\n",
                           timehoursec,
                           milisec,
                           string.Join(",", x, y, z),
                           location.ToString("F8").Trim(toTrim).Replace(" ", ""),
                           ConvergeDist,
                           string.Join(",", left.pupil_diameter_mm, right.pupil_diameter_mm),
                           string.Join(",", left.eye_openness, right.eye_openness)));

        if (Input.GetKey(KeyCode.Return))
        {
            string firstLine = "TrialTime(HrMinSec),Milisec,GazeX,GazeY,GazeZ,HitX,HitY,HitZ,ConvergeDist,LeftPupilDiam,RightPupilDiam,LeftOpen,RightOpen\n";
            string contPath = PlayerPrefs.GetString("Path") + "/continuous_data_" + PlayerPrefs.GetString("Name") + "_" + DateTime.Today.ToString("MMddyyyy") + "_" + PlayerPrefs.GetInt("Run Number").ToString("D3") + ".txt";
            File.WriteAllText(contPath, firstLine);
            File.AppendAllText(contPath, sb.ToString());
            sb.Clear();
            SceneManager.LoadScene("MainMenu");
        }
    }
    public (Vector3, float) CalculateConvergenceDistanceAndCoords(Vector3 origin, Vector3 direction, int layerMask)
    {
        Vector3 coords = Vector3.zero;
        float hit = Mathf.Infinity;

        if (Physics.Raycast(origin, Quaternion.AngleAxis(Vector3.SignedAngle(Vector3.forward, player.transform.forward, Vector3.up), Vector3.up) * direction, out RaycastHit hitInfo, Mathf.Infinity, layerMask))
        {
            coords = hitInfo.point;
            hit = hitInfo.distance;
        }

        return (coords, hit);
    }
}
