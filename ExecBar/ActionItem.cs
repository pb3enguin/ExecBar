namespace ExecBar
{
    public class ActionItem
    {
        public string ActionName { get; set; }
        public string Path { get; set; }

        public string TruncatedPath
        {
            get
            {
                const int maxLength = 50; // Change this to the desired max length
                if (Path.Length <= maxLength)
                    return Path;
                return "..." + Path.Substring(Path.Length - maxLength);
            }
        }
    }
}
