using CSCore;
using CSCore.Codecs;
using CSCore.SoundOut;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCB.Library.Models.Commands.Misc
{
    public class SfxCommand : Command
    {
        private List<string> sfxPaths;
        private ISoundOut SoundOut;
        private IWaveSource WaveSource;
        private float Volume = 1f;

        public SfxCommand()
        {
            ContainsSeveralCommands = true;
            sfxPaths = Directory.GetFiles($"{AppDomain.CurrentDomain.BaseDirectory}sfx").Where(x => x.EndsWith(".mp3") || x.EndsWith(".wav") || x.EndsWith(".m4a")).ToList();
        }

        public override string[] Synonyms()
        {
            List<string> allSfx = new List<string>();
            foreach (var file in sfxPaths)
            {
                allSfx.Add(Path.GetFileNameWithoutExtension(file));
            }
            return allSfx.ToArray();
        }

        public override void Run(CommandContext context)
        {

            var sfxMatch = sfxPaths.Where(x => Path.GetFileNameWithoutExtension(x) == context.CommandText).FirstOrDefault();
            if (sfxMatch != null)
            {
                WaveSource = CodecFactory.Instance.GetCodec(sfxMatch)
                    .ToSampleSource()
                    .ToMono()
                    .ToWaveSource();
                SoundOut = new WasapiOut();
                SoundOut.Initialize(WaveSource);
                SoundOut.Volume = Volume;
                SoundOut.Play();
            }
        }
    }
}
