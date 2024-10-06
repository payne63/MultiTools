
using System;
using Inventor;

namespace MultiTools.Helper;

public static class ApprenticeHelper
{
    private static ApprenticeServerComponent? _apprenticeServer;

    private static ApprenticeServer GetApprenticeServerComponent() => _apprenticeServer ?? new();
    
    public static ApprenticeServerDrawingDocument? GetApprenticeDrawingDocument(string filePath)
    {
        return (ApprenticeServerDrawingDocument)GetApprenticeServerComponent()?.Open(filePath);
    }

    public static ApprenticeServerDocument? GetApprenticeDocument(string filePath)
    {
        return GetApprenticeServerComponent()?.Open(filePath) as ApprenticeServerDocument;
    }

    public static void ResetApprenticeServer()
    {
        _apprenticeServer?.Close();
        _apprenticeServer = null;
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
    
}