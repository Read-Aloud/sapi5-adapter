using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace TextToSpeech
{
    class Program
    {
        static void Main(string[] args)
        {
            var voiceName = args.Length > 0 ? args[0] : null;
            if (voiceName == "-l")
            {
                var voices = new SpeechLib.SpVoice().GetVoices();
                for (var i = 0; i < voices.Count; i++) Console.WriteLine(voices.Item(i).GetAttribute("Name"));
            }
            else
            {
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
}
