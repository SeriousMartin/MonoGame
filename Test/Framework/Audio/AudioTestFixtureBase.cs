using Microsoft.Xna.Framework.Audio;
using MonoGame.Tests;
using MonoGame.Tests.Visual;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace MonoGame.Tests.Framework.Audio
{
    internal class AudioTestFixtureBase
    {
        private VisualTestGame _game;
        protected VisualTestGame Game { get { return _game; } }

        private WasapiCapture wi;
        private WaveFileWriter wfw;

#if OPENAL
        private OpenALSoundController soundControllerInstance = null;
#endif

        [SetUp]
        public virtual void SetUp()
        {
            Paths.SetStandardWorkingDirectory();
            _game = new VisualTestGame();

            // experimental workaround for https://connect.microsoft.com/VisualStudio/feedback/details/797525/unexplained-appdomainunloadedexception-when-running-a-unit-test-on-tfs-build-server
            AppDomain.CurrentDomain.DomainUnload += (sender, args) =>
            {
                int activeIo;
                var sw = Stopwatch.StartNew();
                var timeout = TimeSpan.FromSeconds(3);

                do
                {
                    if (sw.Elapsed > timeout)
                    {
                        Trace.WriteLine("AppDomainUnloadHack: timeout waiting for threads to complete.");
                        sw.Stop();
                        break;
                    }

                    Thread.Sleep(500);

                    int maxWorkers;
                    int availWorkers;
                    int maxIo;
                    int availIo;
                    ThreadPool.GetMaxThreads(out maxWorkers, out maxIo);
                    ThreadPool.GetAvailableThreads(out availWorkers, out availIo);
                    activeIo = maxIo - availIo;

                    Trace.WriteLine(string.Format("AppDomainUnloadHack: active completion port threads: {0}", activeIo));

                } while (activeIo > 0);

                Trace.WriteLine(string.Format("AppDomainUnloadHack: complete after {0}", sw.Elapsed));
            };
        }

        [TearDown]
        public virtual void TearDown()
        {
            _game.Exit();
        }

        protected void AssertAudioFilesSimilar(string waveFileName, float tolerancePercentage = 5)
        {
            double checksum = CalcSampleBasedChecksum(waveFileName);
            double expectedChecksum = CalcSampleBasedChecksum(Path.Combine("Assets/ReferenceAudio", waveFileName));

            double tolerance = expectedChecksum * tolerancePercentage * 0.01;
            Assert.That(checksum, Is.EqualTo(expectedChecksum).Within(tolerance));
        }

        protected void StartRecording(string filename)
        {
            MMDevice device = WasapiLoopbackCapture.GetDefaultLoopbackCaptureDevice();
            wi = new WasapiLoopbackCapture(device);
            wi.DataAvailable += new EventHandler<WaveInEventArgs>(OnDataAvailable);
            wi.RecordingStopped += new EventHandler<StoppedEventArgs>(OnRecordingStopped);

            // set volume to 100%
            device.AudioEndpointVolume.MasterVolumeLevelScalar = 1;
            device.AudioSessionManager.SimpleAudioVolume.Volume = 1;
            // TODO: would be great to mute the other sessions, but it seems not possible to enumerate them

            // TODO: on my windows 10 PCs format is always IEEEFloat with 48k samplerate and two channels
            // Unit tests wont work when this is not the case. Might be necessary to add a conversion
            wfw = new WaveFileWriter(filename, wi.WaveFormat);
            wi.StartRecording();
        }

        protected void StopRecording()
        {
            wi.StopRecording();

            // TODO: would be better to do this in CalcSampleBasedChecksum
            wfw.Close();
            wfw.Dispose();
            wfw = null;
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (wfw != null)
            {
                wfw.Write(e.Buffer, 0, e.BytesRecorded);
            }
        }

        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            // TODO: Somehow this is not called when used in unit tests

            wi.Dispose();
            wi = null;
        }

        protected double CalcSampleBasedChecksum(string filename)
        {
            WaveFileReader reader = new WaveFileReader(filename);
            int readerLength = (int)reader.Length / (reader.WaveFormat.BitsPerSample / 8);

            ISampleProvider sampleProvider = new WaveToSampleProvider(reader);
            if (sampleProvider.WaveFormat.SampleRate != 44100)
            {
                sampleProvider = new WdlResamplingSampleProvider(sampleProvider, 44100);
            }

            float[] data = new float[readerLength];
            sampleProvider.Read(data, 0, readerLength);

            double checksum = 0;
            for (int i = 0; i < readerLength; i++)
            {
                checksum += Math.Abs(data[i]);
            }

            return checksum;
        }
    }
}
