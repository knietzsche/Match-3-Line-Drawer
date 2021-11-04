using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class InputManager : MonoBehaviour
{
    private List<Cell> selectedList = new List<Cell>();

    private bool mouseDown;
    private bool lockInput;

    private void OnEnable()
    {
        GameManager.AddDataUpdateListener(OnDataUpdate);
    }

    private void OnDisable()
    {
        GameManager.RemoveDataUpdateListener(OnDataUpdate);
    }


    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (Application.isMobilePlatform)
            {
                Application.Unload();
            }
            else
            {
                Application.Quit();
            }
        }

        if (lockInput)
        {
            return;
        }

        if (mouseDown && selectedList.Count > 0)
        {
            var last = selectedList[selectedList.Count - 1];

            if (!last.IsOverlapping(Input.mousePosition))
            {
                var cell = GameManager.Instance?.Cells.Where(x => x.IsOverlapping(Input.mousePosition)).FirstOrDefault();

                if (cell != null && last.IsNeighbor(cell))
                {
                    var index = selectedList.IndexOf(cell);
                    if (index == -1)
                    {
                        if (cell.Type == last.Type)
                        {
                            selectedList.Add(cell);
                            last.SetArrowTo(cell);
                        }
                    }
                    else if (index == selectedList.Count - 2)
                    {
                        selectedList[index].SetArrowTo(null);
                        selectedList.Remove(last);
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            foreach (var cell in GameManager.Instance?.Cells ?? Enumerable.Empty<Cell>())
            {
                    if (cell.IsOverlapping(Input.mousePosition))
                    {
                        selectedList.Add(cell);
                        break;
                    }
            }

            mouseDown = true;
        }

        if (Input.GetMouseButtonUp(0)) 
        {
            foreach (var cell in selectedList)
            {
                cell.SetArrowTo(null);
            }

            if (selectedList.Count > 2)
            {
                GameManager.Instance?.Collect(selectedList.ToArray());
            }

            selectedList.Clear();
            mouseDown = false;
        }
    }

    public void OnDataUpdate()
    {
        switch (GameManager.Instance?.State)
        {
            case GameManager.GameState.Round:
                lockInput = false;
                break;
            case GameManager.GameState.Start:
            case GameManager.GameState.Collect:
            case GameManager.GameState.End:
                lockInput = true;
                break;
        }
    }
}
