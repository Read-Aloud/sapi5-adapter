using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace TextToSpeech
{
    class Program
    {
        static void Main(string[] args)
        {
            var voiceName = args.Length > 0 ? args[0] : null;
            if (voiceName == "-l")
            {
                var voices = new SpeechLib.SpVoice().GetVoices()
                    .Cast<SpeechLib.ISpeechObjectToken>()
                    .Select(token => new VoiceInfo(token))
                    .ToArray();
                new DataContractJsonSerializer(voices.GetType())
                    .WriteObject(Console.OpenStandardOutput(), voices);
            }
            else
            {
                Console.InputEncoding = System.Text.Encoding.UTF8;
                Speak(Console.In.ReadToEnd(), voiceName, Console.OpenStandardOutput());
            }
        }

        static void Speak(string text, string voiceName, Stream outputStream)
        {
            var ms = new SpeechLib.SpCustomStream();
            ms.BaseStream = new Wrapper(outputStream);
            var v = new SpeechLib.SpVoice();
            v.Voice = FindVoice(v.GetVoices(), voiceName);
            v.AudioOutputStream = ms;
            v.Speak(text);
        }

        static SpeechLib.SpObjectToken FindVoice(SpeechLib.ISpeechObjectTokens voices, string voiceName)
        {
            for (var i = 0; i < voices.Count; i++) if (voices.Item(i).GetAttribute("Name") == voiceName) return voices.Item(i);
            return null;
        }
    }

    class Wrapper : IStream
    {
        readonly Stream stream;

        public Wrapper(Stream stream)
        {
            this.stream = stream;
        }

        public void Clone(out IStream ppstm)
        {
            throw new NotImplementedException();
        }

        public void Commit(int grfCommitFlags)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
        {
            throw new NotImplementedException();
        }

        public void LockRegion(long libOffset, long cb, int dwLockType)
        {
            throw new NotImplementedException();
        }

        public void Read(byte[] pv, int cb, IntPtr pcbRead)
        {
            throw new NotImplementedException();
        }

        public void Revert()
        {
            throw new NotImplementedException();
        }

        public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
        {
        }

        public void SetSize(long libNewSize)
        {
            throw new NotImplementedException();
        }

        public void Stat(out STATSTG pstatstg, int grfStatFlag)
        {
            throw new NotImplementedException();
        }

        public void UnlockRegion(long libOffset, long cb, int dwLockType)
        {
            throw new NotImplementedException();
        }

        public void Write(byte[] pv, int cb, IntPtr pcbWritten)
        {
            stream.Write(pv, 0, cb);
        }
    }

    [DataContract]
    class VoiceInfo
    {
        [DataMember] public string Gender;
        [DataMember] public string Age;
        [DataMember] public string Name;
        [DataMember] public string Language;
        [DataMember] public string Vendor;

        public VoiceInfo(SpeechLib.ISpeechObjectToken token)
        {
            Gender = token.GetAttribute("Gender");
            Age = token.GetAttribute("Age");
            Name = token.GetAttribute("Name");
            Language = token.GetAttribute("Language");
            Vendor = token.GetAttribute("Vendor");
        }
    }
}
