using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Rochester.ARTable.UI;

[CustomEditor(typeof(StructureWall))]
public class WallEditor : Editor {

    private StructureWall wall;
    private Vector3 position;

    public void OnSceneGUI()
    {

        CustomEditorHandles.DragHandleResult dhResult;
        StructureWall wall = target as StructureWall;
        if (wall.Positions.Count == 0)
            wall.AddPosition(wall.transform.position);

        Vector3 newPosition = CustomEditorHandles.DragHandle(wall.LastPosition(), 2, Handles.SphereCap, Color.red, out dhResult);



        switch (dhResult)
        {
            case CustomEditorHandles.DragHandleResult.LMBDoubleClick:
                Undo.RecordObject(wall, "Add node to wall");
                wall.AddPosition(newPosition);
                break;
            case CustomEditorHandles.DragHandleResult.RMBDoubleClick:
                Undo.RecordObject(wall, "Deleted Node");
                wall.DeletePosition();
                break;
            case CustomEditorHandles.DragHandleResult.LMBDrag:
                Undo.RecordObject(wall, "Moved wall Node");
                wall.SetPosition(newPosition);
                break;
            case CustomEditorHandles.DragHandleResult.LMBRelease:
                Undo.RecordObject(wall, "Moved wall node");
                wall.SetPosition(newPosition);
                break;
        }



    }
}
