using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum INTERACTION_TYPE
{
    RAYCASTING,
    MY_TECHNIQUE
}

public class TaskManager : MonoBehaviour
{
    public bool isTraining = false;
    public int userId = 0;
    public INTERACTION_TYPE interactionType = INTERACTION_TYPE.RAYCASTING;
    public List<GameObject> objectsToSelect;

    [SerializeField]
    RaycastTechnique raycastTechnique;

    [SerializeField]
    MyTechnique myTechnique;

    private int currentTaskIndex = 0;

    // Log variables
    private List<TaskLog> taskLogs;
    private TaskLog currentTaskLog;

    private void Start()
    {
        // Disabling all technique scripts except the one selected
        switch (interactionType)
        {
            case INTERACTION_TYPE.RAYCASTING:
                myTechnique.enabled = false;
                break;
            case INTERACTION_TYPE.MY_TECHNIQUE:
                raycastTechnique.enabled = false;
                break;
        }

        taskLogs = new List<TaskLog>();

        if (isTraining)
        {
            objectsToSelect = objectsToSelect.GetRange(1, 10);//setting training objects as a subset
        }

        SelectableObject objectToSelectScript = objectsToSelect[0].GetComponent<SelectableObject>();

        currentTaskLog = new TaskLog(Time.time, objectToSelectScript.GetObjectName());
        objectToSelectScript.SetAsTarget();
    }

    public GameObject GetCurrentObjectToSelect()
    {
        return objectsToSelect[currentTaskIndex];
    }

    public void OnSelectionEvent(GameObject selectedObject)
    {
        SelectableObject selectedObjectScript = selectedObject.GetComponent<SelectableObject>();
        SelectableObject objectToSelectScript = objectsToSelect[currentTaskIndex].GetComponent<SelectableObject>();
        if (selectedObjectScript == null || selectedObjectScript.GetObjectName() != objectToSelectScript.GetObjectName())
        {
            HandleSelectionError();
        }
        else
        {
            objectToSelectScript.SetAsSuccess();
            BeginNextSelectionTask();
        }
    }

    private void HandleSelectionError()
    {
        currentTaskLog.IncrementErrorCount();
    }

    private void BeginNextSelectionTask()
    {
        currentTaskLog.SetEndTimestamp(Time.time);
        taskLogs.Add(currentTaskLog);

        if (currentTaskIndex + 1 < objectsToSelect.Count)
        {
            currentTaskIndex++;

            SelectableObject objectToSelectScript = objectsToSelect[currentTaskIndex].GetComponent<SelectableObject>();

            currentTaskLog = new TaskLog(Time.time, objectToSelectScript.GetObjectName());
            objectToSelectScript.SetAsTarget();
        }
        else
        {
            if (!isTraining)
            {
                GenerateReport();
            }
            // TODO handle end of study
        }
    }

    private void GenerateReport()
    {
        string fileName = userId + "_" + interactionType.ToString() + ".csv";
        string logLines = "userId,interactionType,objectName,startTimestamp,endTimestamp,errorCount\n";
        foreach(TaskLog taskLog in taskLogs)
        {
            logLines += userId + "," + interactionType + "," + taskLog.GetObjectName() + "," + taskLog.GetStartTimestamp() + "," + taskLog.GetEndTimestamp() + "," + taskLog.GetErrorCount() + "\n";
        }

        CreateNewDataFile(fileName, logLines);
    }

    private void CreateNewDataFile(string filename, string content)
    {
        //** N.B. use .persistentDataPath if running on Quest/device unlinked,
        //**      else use .dataPath if Quest/device is linked/connected to machine via USB.
        string path = Application.dataPath + "/" + filename;
        //string path = Application.persistentDataPath + "/" + filename;

        //** Create new file if it doesn't exist, if append to file to avoid overwritting data due to selection error from previous scene.
        if (!File.Exists(path))
        {
            //var text = System.DateTime.Now + ",START\n";
            File.WriteAllText(path, content);
        }
        else
        {
            //** Add data to file
            //string content = System.DateTime.Now + ",File Already Exists\n";
            File.AppendAllText(path, content);
        }
    }
}
