using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MessageSchedule", menuName = "Game/Configs/MessageSchedule")]
public class MessageSchedule : ScriptableObject
{
    public List<DayData> days;
}

[Serializable]
public class DayData
{
    public int dayIndex;
    public List<MessageData> messages;
}

[Serializable]
public class MessageData
{
    [TextArea(2, 5)]
    public string messageText;

    public float time;

    public AnswerOption answerA;
    public AnswerOption answerB;
}

[Serializable]
public class AnswerOption
{
    public string text;
}