using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CalendarView : MonoBehaviour
{
    [SerializeField] private TMP_Text monthText;
    [SerializeField] private Transform dateGrid;
    [SerializeField] private Transform streakContainer;
    [SerializeField] private GameObject dateCellPrefab;
    [SerializeField] private GameObject streakBarPrefab;
    [SerializeField] private GameObject singleCirclePrefab;

    [SerializeField] private float streakBarOffsetX = 20f;
    [SerializeField] private float streakBarOffsetY = 25f;
    [SerializeField] private float singleCircleOffsetX = 20f;
    [SerializeField] private float singleCircleOffsetY = 25f;

    private float cellSize;
    private float spacingX;
    private float spacingY;
    private float paddingTop;
    private float paddingLeft;

    public void Setup(int month, List<CalendarDayData> days, int offset)
    {
        if (monthText != null) monthText.text = month.ToString();
        InitCalendar(days, offset);
    }

    private void InitCalendar(List<CalendarDayData> dayDataList, int offset)
    {
        var grid = dateGrid.GetComponent<GridLayoutGroup>();
        cellSize = grid.cellSize.x;
        spacingX = grid.spacing.x;
        spacingY = grid.spacing.y;
        paddingTop = grid.padding.top;
        paddingLeft = grid.padding.left;

        foreach (Transform child in dateGrid) Destroy(child.gameObject);
        foreach (Transform child in streakContainer) Destroy(child.gameObject);

        for (int i = 0; i < offset; i++)
        {
            GameObject emptyCell = Instantiate(dateCellPrefab, dateGrid);
            emptyCell.GetComponentInChildren<TMP_Text>().gameObject.SetActive(false);
        }

        for (int i = 0; i < dayDataList.Count; i++)
        {
            GameObject cell = Instantiate(dateCellPrefab, dateGrid);
            TMP_Text cellText = cell.GetComponentInChildren<TMP_Text>();
            cellText.text = dayDataList[i].day.ToString();

            if (!dayDataList[i].isAttended) cellText.color = new Color32(153, 153, 153, 255);
            else cellText.color = new Color32(255, 138, 61, 255);
        }
        DrawStreakBars(dayDataList, offset);
    }

    private void DrawStreakBars(List<CalendarDayData> days, int offset)
    {
        int startDayIndex = -1;
        for (int i = 0; i < days.Count; i++)
        {
            int gridIndex = i + offset;
            if (days[i].isAttended)
            {
                if (startDayIndex == -1) startDayIndex = i;
                if (gridIndex % 7 == 6)
                {
                    CreateBar(startDayIndex, i, offset);
                    startDayIndex = -1;
                }
            }
            else
            {
                if (startDayIndex != -1)
                {
                    CreateBar(startDayIndex, i - 1, offset);
                    startDayIndex = -1;
                }
            }
        }
        if (startDayIndex != -1) CreateBar(startDayIndex, days.Count - 1, offset);
    }

    private void CreateBar(int start, int end, int offset)
    {
        int startIndex = start + offset;
        int endIndex = end + offset;

        float startX = paddingLeft + (startIndex % 7) * (cellSize + spacingX);
        float startY = -((startIndex / 7) * (cellSize + spacingY) + streakBarOffsetY);
        float endX = paddingLeft + (endIndex % 7) * (cellSize + spacingX);

        if (start == end)
        {
            GameObject circle = Instantiate(singleCirclePrefab, streakContainer);
            RectTransform rt = circle.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(startX + singleCircleOffsetX, startY - 10f);
            rt.sizeDelta = new Vector2((endX - startX) + cellSize, rt.sizeDelta.y);
        }
        else
        {
            GameObject bar = Instantiate(streakBarPrefab, streakContainer);
            RectTransform rt = bar.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(startX + streakBarOffsetX, startY - 10f);
            rt.sizeDelta = new Vector2((endX - startX) + cellSize, rt.sizeDelta.y);
        }
    }
}