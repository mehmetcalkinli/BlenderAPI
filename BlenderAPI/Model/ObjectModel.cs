namespace BlenderAPI.Model
{
    public class ObjectModel
    {
        public string name { get; set; } = string.Empty;
        public string projectName { get; set; } = string.Empty;
        public string content { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
        public string path { get; set; } = string.Empty;
        public Coordinates coordinates { get; set; }

        public Rotation rotation { get; set; }
        public int i { get; set; }
        //public override string ToString()
        //{
        //    return name;
        //}
    }

    public class Coordinates
    {
        public double? X { get; set; }
        public double? Y { get; set; }
        public double? Z { get; set; }

    }

    public class Rotation
    {
        public double? Rotation_X { get; set; }
        public double? Rotation_Y { get; set; }
        public double? Rotation_Z { get; set; }

    }
}
