
using System;
using System.Threading.Tasks;
using Inventor;

namespace MultiTools.Helper;

public static class ApprenticeHelper
{
    private static ApprenticeServerComponent? _apprenticeServer;

    private static ApprenticeServerComponent GetApprenticeServerComponent() => _apprenticeServer ?? new();

    public static void PreLoadApprenticeServer() => _ = GetApprenticeServerComponent();
    
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

    public static async Task ResetApprenticeServerAsync()
    {
        await Task.Run(() =>
        {
            _apprenticeServer?.Close();
            _apprenticeServer = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        });
    }
    
}