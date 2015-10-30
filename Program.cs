namespace ConsoleApplication10
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using HipChat;

    public class Program
    {
        static readonly Random RandomGenerator = new Random();

        public static Dictionary<string, Color> PreferredColors = new Dictionary<string, Color>
        {
            { "Buttercup", Color.Purple },
            { "Count Rugen", Color.Red },
            { "Dread Pirate Roberts", Color.Green },
            { "Humperdinck", Color.Purple },
            { "Inigo", Color.Yellow },
            { "Man In Black", Color.Green },
            { "NARRATOR", Color.Gray },
            { "R.O.U.S.", Color.Red },
            { "Valerie", Color.Purple },
            { "Vizzini", Color.Yellow },
            { "Westley", Color.Green },

            //{ "CEREAL", Color.Yellow },
            //{ "DADE", Color.Green },
            //{ "NARRATOR", Color.Gray },
            //{ "NIKON", Color.Purple },
            //{ "KATE", Color.Purple },
            //{ "PLAGUE", Color.Red },

            //{ "Bill", Color.Green },
            //{ "Bill & Ted", Color.Green },
            //{ "Bill and Ted", Color.Green },
            //{ "Bill2", Color.Gray },
            //{ "Bill2 & Ted2", Color.Red },
            //{ "NARRATOR", Color.Gray },
            //{ "Ted", Color.Yellow },
            //{ "Ted2", Color.Red },
            //{ "Rufus", Color.Purple },
        };

        public static void Main(string[] args)
        {
            int startScene = 0;
            if (args.Length == 1)
            {
                startScene = Int32.Parse(args[0]);
            }

            var script = System.IO.File.ReadAllLines(@"C:\Work\HipchatCinema\tpb.txt");

            var scenes = new List<Scene>();

            foreach (var scriptLine in script)
            {
                var tokens = scriptLine.Split(new[] { ':' }, 2);
                var actor = tokens[0].Trim();
                var message = tokens[1].Trim();

                // starts a new scene
                if (tokens[0].Trim().Equals("scene", StringComparison.InvariantCultureIgnoreCase))
                {
                    scenes.Add(new Scene { Title = string.Format("<em>{0}</em>", message) });
                    continue;
                }

                scenes.Last().Actors.Add(actor);
                scenes.Last().Dialogue.Add(new Dialogue { Actor = actor, Line = message });
            }

            // assign colors to the actors in each scene
            foreach (var scene in scenes)
            {
                var colors = new Dictionary<string, Color>();
                Tuple<string, Color> last = null;

                // first try to assign preferred colors
                foreach (var actor in scene.Actors)
                {
                    if (!colors.ContainsKey(actor) && PreferredColors.ContainsKey(actor))
                    {
                        colors.Add(actor, PreferredColors[actor]);
                    }
                }

                foreach (var d in scene.Dialogue)
                {
                    // if this actor has already been assigned a color, we're good to go
                    if (colors.ContainsKey(d.Actor))
                    {
                        last = new Tuple<string, Color>(d.Actor, colors[d.Actor]);
                        continue;
                    }

                    // otherwise, pick a random color for this actor, but don't pick the same color
                    // as the actor before them in the dialogue
                    var color = RandomColor();
                    while (last != null && color == last.Item2 && color == Color.Gray)
                    {
                        color = RandomColor();
                    }

                    colors.Add(d.Actor, color);
                    last = new Tuple<string, Color>(d.Actor, color);
                }

                scene.Colors = colors;
            }

            PlayScenes(scenes, startScene);
        }

        private static void PlayScenes(List<Scene> scenes, int startScene = 0)
        {
            const string AuthToken = "";
            const int RoomId = 0; // HipChat Cinema?

            var sceneClient = new HipChatClient(AuthToken, RoomId, "SCENE");

            // play the scenes, waiting 3 seconds in between lines
            for (var i = startScene; i < scenes.Count; ++i)
            {
                var scene = scenes[i];
                var clients = scene.Actors.ToDictionary(actor => actor, actor => new HipChatClient(AuthToken, RoomId, actor));

                sceneClient.SendMessage(scene.Title, HipChatClient.BackgroundColor.gray, false);

                Thread.Sleep(TimeSpan.FromSeconds(5));

                foreach (var d in scene.Dialogue)
                {
                    clients[d.Actor].SendMessage(d.Line, MapColor(scene.Colors[d.Actor]), false);
                    var seconds = (d.Line.Split(new[] { ' ' }).Length / 4);
                    Thread.Sleep(TimeSpan.FromSeconds(Math.Max(seconds, 3)));
                }

                Console.WriteLine("Finished scene {0} - {1}", i, scene.Title);
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        }

        private static HipChatClient.BackgroundColor MapColor(Color color)
        {
            switch (color)
            {
                case Color.Gray:
                    return HipChatClient.BackgroundColor.gray;
                case Color.Green:
                    return HipChatClient.BackgroundColor.green;
                case Color.Purple:
                    return HipChatClient.BackgroundColor.purple;
                case Color.Red:
                    return HipChatClient.BackgroundColor.red;
                case Color.Yellow:
                    return HipChatClient.BackgroundColor.yellow;
                default:
                    return HipChatClient.BackgroundColor.random;
            }
        }

        private static Color RandomColor()
        {
            var values = Enum.GetValues(typeof(Color));
            return (Color)values.GetValue(RandomGenerator.Next(values.Length));
        }
    }
}