namespace ConsoleApplication10
{
    using System.Collections.Generic;

    public class Scene
    {
        public Scene()
        {
            this.Colors = new Dictionary<string, Color>();
            this.Dialogue = new List<Dialogue>();
            this.Actors = new HashSet<string>();
        }

        public string Title { get; set;  }
        public Dictionary<string, Color> Colors { get; set; }

        public List<Dialogue> Dialogue { get; set; }

        public HashSet<string> Actors { get; set; }
    }
}