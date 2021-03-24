namespace Domain.Book.Messages
{
    public class GetPath
    {
        private GetPath() { }
        public static GetPath Instance { get; } = new();
        
    }
}