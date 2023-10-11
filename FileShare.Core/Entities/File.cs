namespace FileShare.Entities
{
    public class File
    {
        public const int BytesInMb = 1024 * 1024;
        public const int BytesInKb = 1024;

        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Time { get; set; }
        public long Size { get; set; }

        public string GetSizeString()
        {
            string postfix = "bytes";
            double value = Size;
            if (Size > BytesInMb)
            {
                postfix = "Mb";
                value = GetSizeMb();
            }
            else if (Size > BytesInKb)
            {
                postfix = "Kb";
                value = GetSizeKb();
            }
            return $"{value} {postfix}";
        }

        public double GetSizeMb() 
        {
            return Math.Round(Size / (double)BytesInMb, 2);
        }

        public double GetSizeKb()
        {
            return Math.Round(Size / (double)BytesInKb, 2);
        }
    }
}
