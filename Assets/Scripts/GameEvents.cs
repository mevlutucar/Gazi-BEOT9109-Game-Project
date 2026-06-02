using System;

public static class GameEvents
{
    // Olay (Sinyal) Tanýmlamalarý
    public static event Action OnSunset;
    public static event Action OnConversationEnded;

    // Sinyalleri Ateþleyen Tetikleyiciler
    public static void TriggerSunset() => OnSunset?.Invoke();
    public static void TriggerConversationEnded() => OnConversationEnded?.Invoke();
}