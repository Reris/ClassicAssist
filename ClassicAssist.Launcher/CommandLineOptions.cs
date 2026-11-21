using CommandLine;

namespace ClassicAssist.Launcher
{
    public class CommandLineOptions
    {
        [Option( "shard", Required = false )]
        public string Shard { get; set; }

        [Option( "light", Required = false )]
        public bool Light { get; set; }

        [Option( "dark", Required = false )]
        public bool Dark { get; set; }
    }
}