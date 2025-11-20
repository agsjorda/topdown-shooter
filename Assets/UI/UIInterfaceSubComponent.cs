public interface UIInterfaceSubComponent
{
    void Show();
    void Hide();
    bool IsValid { get; }
    bool IsVisible { get; } // Added to support toggle functionality
}