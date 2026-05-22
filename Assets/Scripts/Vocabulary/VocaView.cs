using UnityEngine;
using System.Collections.Generic;

// 단어 하나에 들어갈 데이터
[System.Serializable]
public class WordData
{
    public string word;              // 영단어 (예: market)
    public string pronunciation;     // 발음 기호 (예: /'mɑ:rkɪt/)
    public string meaning;           // 한국어 뜻 (예: 1. 시장, 가게...)
    public string questName;         // 퀘스트 이름 (예: Quest 1)
    public bool isBookmarked;        // 즐겨찾기(북마크) 여부
}

// 단어장 전체 화면에 넘겨줄 데이터
public class VocabularyData
{
    public int wordsToReviewCount;   // "잊기 전에 복습하기" 에 뜰 남은 단어 수 (선택)
    public List<WordData> wordList;  // 전체 단어 리스트
    public List<string> questFilters;
}