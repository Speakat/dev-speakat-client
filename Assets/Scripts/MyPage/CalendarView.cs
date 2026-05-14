using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Pool;

public class CalendarView : MonoBehaviour
{
    [SerializeField] private TMP_Text monthText;
    [SerializeField] private Transform dateGrid;
    [SerializeField] private Transform streakContainer;

    [Header("Prefab")]
    [SerializeField] private GameObject dateCellPrefab;
    [SerializeField] private GameObject streakBarPrefab;
    [SerializeField] private GameObject singleCirclePrefab;

    [Header("Offset")]
    [SerializeField] private float streakBarOffsetX = 20f;
    [SerializeField] private float streakBarOffsetY = 25f;
    [SerializeField] private float singleCircleOffsetX = 20f;
    [SerializeField] private float singleCircleOffsetY = 25f;

    // Object Pools
    private IObjectPool<GameObject> dateCellPool;
    private IObjectPool<GameObject> streakBarPool;
    private IObjectPool<GameObject> singleCirclePool;

    // 현재 사용 중인 오브젝트 추적 (풀로 반납하기 위해...)
    private List<GameObject> activeObjects = new List<GameObject>();

    private float cellSize;
    private float spacingX;
    private float spacingY;
    private float paddingTop;
    private float paddingLeft;

    public void Awake()
    {
        // Initialize date cell pool
        dateCellPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(dateCellPrefab, dateGrid),
            actionOnGet:  (obj) => obj.SetActive(true),
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj),
            defaultCapacity: 35, maxSize: 50);

        // Initialize streak bar pool
        streakBarPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(streakBarPrefab, streakContainer),
            actionOnGet: (obj) => obj.SetActive(true),
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj),
            defaultCapacity: 35, maxSize: 50);

        // Initialize single circle pool
        singleCirclePool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(singleCirclePrefab, streakContainer),
            actionOnGet: (obj) => obj.SetActive(true),
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj),
            defaultCapacity: 35, maxSize: 50);
    }

    public void Setup(int month, List<CalendarDayData> days, int offset)
    {
        if (monthText != null) monthText.text = month.ToString();

        ClearCalendar(); // 기본 오브젝트들 파괴하지 않고 풀로 반납

        InitCalendar(days, offset);
    }

    private void ClearCalendar()
    {
        foreach (var obj in activeObjects)
        {
            if (obj.name.Contains("DateCell")) dateCellPool.Release(obj);
            else if (obj.name.Contains("StreakBar")) streakBarPool.Release(obj);
            else if (obj.name.Contains("SingleCircle")) singleCirclePool.Release(obj);
        }
        activeObjects.Clear();
    }

    private void InitCalendar(List<CalendarDayData> dayDataList, int offset)
    {
        var grid = dateGrid.GetComponent<GridLayoutGroup>();
        cellSize = grid.cellSize.x;
        spacingX = grid.spacing.x;
        spacingY = grid.spacing.y;
        paddingTop = grid.padding.top;
        paddingLeft = grid.padding.left;

        //foreach (Transform child in dateGrid) Destroy(child.gameObject);
        //foreach (Transform child in streakContainer) Destroy(child.gameObject);

        for (int i = 0; i < offset; i++)
        {
            //GameObject emptyCell = Instantiate(dateCellPrefab, dateGrid);
            GameObject emptyCell = dateCellPool.Get();
            emptyCell.name = "DateCell";
            emptyCell.GetComponentInChildren<TMP_Text>().gameObject.SetActive(false);
            activeObjects.Add(emptyCell);
        }

        for (int i = 0; i < dayDataList.Count; i++)
        {
            //GameObject cell = Instantiate(dateCellPrefab, dateGrid);
            GameObject cell = dateCellPool.Get();
            cell.name = "DateCell";
            activeObjects.Add(cell);

            TMP_Text cellText = cell.GetComponentInChildren<TMP_Text>();
            cellText.gameObject.SetActive(true);
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

        GameObject obj;
        if (start == end)
        {
            //GameObject circle = Instantiate(singleCirclePrefab, streakContainer);
            //RectTransform rt = circle.GetComponent<RectTransform>();
            obj = singleCirclePool.Get();
            obj.name = "SingleCircle";
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(startX + singleCircleOffsetX, startY - 10f);
            //rt.sizeDelta = new Vector2((endX - startX) + cellSize, rt.sizeDelta.y);
        }
        else
        {
            //GameObject bar = Instantiate(streakBarPrefab, streakContainer);
            //RectTransform rt = bar.GetComponent<RectTransform>();
            obj = streakBarPool.Get();
            obj.name = "StreakBar";
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(startX + streakBarOffsetX, startY - 10f);
            rt.sizeDelta = new Vector2((endX - startX) + cellSize, rt.sizeDelta.y);
        }
        activeObjects.Add(obj);
    }
}