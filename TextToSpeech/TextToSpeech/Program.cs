using System;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Speech.AudioFormat;

namespace TextToSpeech
{
    class Program
    {
        static void Main(string[] args)
        {
            var voiceName = args.Length > 0 ? args[0] : null;
            if (voiceName == "-l")
            {
                var voices = new SpeechSynthesizer().GetInstalledVoices();
                Console.Write(string.Format("[{0}]", string.Join(",", voices.Select(voice => ToJson(voice.VoiceInfo)))));
            }
            else
            {
                Console.InputEncoding = System.Text.Encoding.UTF8;
                Speak(Console.In.ReadToEnd(), voiceName, Console.OpenStandardOutput());
            }
        }

        static void Speak(string text, string voiceName, Stream outputStream)
        {
            var v = new SpeechSynthesizer();
            try { v.SelectVoice(voiceName); } catch { }
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
                string.Format("\"Vendor\":\"{0}\",", vi.AdditionalInfo["Vendor"]) +
                string.Format("\"Language\":\"{0}\"", vi.Culture.Name) +
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
