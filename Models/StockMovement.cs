using System;

namespace MultiTools.Models;

public record StockMovement
{
    public StockMovement(string[] arrayData)
    {
        if (arrayData.Length != 7)
        {
            throw new ArgumentException("Wrong number of data");
        }
        Date = DateOnly.Parse(arrayData[0]);
        Action = (ActionStock)Enum.Parse(typeof(ActionStock), arrayData[1]);
        Qt = int.Parse(arrayData[2]);
        StockItem = new StockItem(arrayData[3], arrayData[4], arrayData[5] == "Oui");
    }

    public StockMovement(string code, string designation, bool finishedProduct, int qt, ActionStock action, DateOnly date)
    {
        Date = date;
        Action = action;
        Qt = qt;
        StockItem = new StockItem(code, designation, finishedProduct);
    }
    
    public enum ActionStock
    {
        Init,
        EntryBrut,
        EntryFinished,
        ExitBrut,
        ExitFinished,
        Finish,
    }
    
    StockItem StockItem { get; init; }
    public int Qt { get; init; }
    public ActionStock Action { get; init; }

    public DateOnly Date { get; init; }
    
    
}