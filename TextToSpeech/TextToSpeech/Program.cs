using System;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Speech.AudioFormat;
using Microsoft.Win32;

namespace TextToSpeech
{
    class Program
    {
        static void Main(string[] args)
        {
            var voiceName = args.Length > 0 ? args[0] : null;
            if (voiceName == "-l")
            {
                // IMPORTANT: must target x64 (not "Any CPU") to see all voices
                var voices = new SpeechSynthesizer().GetInstalledVoices();
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.Write(string.Format("[{0}]", string.Join(",", voices.Select(voice => ToJson(voice.VoiceInfo)))));
            }
            else if (voiceName == "-c")
            {
                using (var r1 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Speech_OneCore\\Voices\\Tokens"))
                using (var r2 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Speech\\Voices\\Tokens", true))
                using (var enumerator = r1.GetSubKeyNames().Except(r2.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase).GetEnumerator())
                    while (enumerator.MoveNext())
                    {
                        string current = enumerator.Current;
                        Console.WriteLine(current);
                        using (var src = r1.OpenSubKey(current))
                        using (var dst = r2.CreateSubKey(current))
                            CopyReg(src, dst);
                    }
            }
            else
            {
                Console.InputEncoding = System.Text.Encoding.UTF8;
                Speak(Console.In.ReadToEnd(), voiceName, Console.OpenStandardOutput());
            }
        }

        static void CopyReg(RegistryKey src, RegistryKey dst)
        {
            foreach (string valueName in src.GetValueNames())
                dst.SetValue(valueName, src.GetValue(valueName), src.GetValueKind(valueName));

            foreach (string subKeyName in src.GetSubKeyNames())
                using (var srcSub = src.OpenSubKey(subKeyName))
                using (var dstSub = dst.CreateSubKey(subKeyName))
                    CopyReg(srcSub, dstSub);
        }

        static void Speak(string text, string voiceName, Stream outputStream)
        {
            var v = new SpeechSynthesizer();
            try
            {
                v.SelectVoice(voiceName);
                if (voiceName.StartsWith("Vocalizer")) v.Volume = 50;
            }
            catch
            {
            }
            v.SetOutputToAudioStream(new StreamWrapper(outputStream), new SpeechAudioFormatInfo(22050, AudioBitsPerSample.Sixteen, AudioChannel.Mono));
            if (text.StartsWith("<speak")) v.SpeakSsml(text);
            else v.Speak(text);
        }

        static string ToJson(VoiceInfo vi)
        {
            return "{" +
                string.Format("\"Name\":\"{0}\",", vi.Name) +
                string.Format("\"Gender\":\"{0}\",", vi.Gender.ToString()) +
                string.Format("\"Age\":\"{0}\",", vi.Age.ToString()) +
                string.Format("\"Language\":\"{0}\"", vi.Culture?.Name ?? "xx") +
                "}";
        }
    }

    class StreamWrapper : Stream
    {
        readonly Stream baseStream;

        public StreamWrapper(Stream baseStream)
        {
            this.baseStream = baseStream;
        }

        public override bool CanRead
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanSeek
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanWrite
        {
            get { throw new NotImplementedException(); }
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get { return 0; }
            set { throw new NotImplementedException(); }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (offset != 0 || origin != SeekOrigin.Begin) throw new NotImplementedException();
            return 0;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            baseStream.Write(buffer, offset, count);
        }
    }
}
