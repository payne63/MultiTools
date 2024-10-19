using System;

namespace MultiTools.Models;

public class CleanHistoryModel
{
    public string From
    {
        get;
        set;
    }

    public string To
    {
        get;
        set;
    }
    
    public CleanHistoryModel(string from, string to)
    {
        From = from;
        To = to;
    }
}