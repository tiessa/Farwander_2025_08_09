namespace TJNK.Farwander.Modules.Fov
{
    /// <summary>
    /// Read-only access to fog-of-war state.
    /// </summary>
    public interface IFovQuery
    {
        int Width { get; }
        int Height { get; }
        bool InBounds(int x, int y);
        bool IsVisible(int x, int y);
        bool IsExplored(int x, int y);
    }
}