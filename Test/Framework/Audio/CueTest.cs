using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonoGame.Tests.Framework.Audio
{
    [TestFixture]
    class CueTest : AudioTestFixtureBase
    {
        private const string CUE_2D = "achievement";
        private const string CUE_2D_TRACKS = "achievementDouble";
        private const string CUE_3D = "fire";

        private const string VAR_VOLUME = "Volume";
        private const string VAR_DISTANCE = "Distance";
        private const string VAR_SPEED = "Speed";

        private AudioEngine audioEngine;
        private WaveBank waveBank;
        private SoundBank soundBank;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            audioEngine = new AudioEngine("Assets/Audio/XactContent.xgs");
            waveBank = new WaveBank(audioEngine, "Assets/Audio/WaveBank.xwb");
            soundBank = new SoundBank(audioEngine, "Assets/Audio/SoundBank.xsb");         

            audioEngine.Update();
        }

        [Test]
        public void SetVariable()
        {
            Cue cue2D = soundBank.GetCue(CUE_2D);
            Assert.AreEqual(100, cue2D.GetVariable(VAR_VOLUME), "Initial volume souble be 100 (see xap-file)");

            cue2D.SetVariable(VAR_VOLUME, 50);
            Assert.AreEqual(50, cue2D.GetVariable(VAR_VOLUME));

            cue2D.SetVariable(VAR_VOLUME, 100.001f);
            Assert.AreEqual(100, cue2D.GetVariable(VAR_VOLUME));

            cue2D.SetVariable(VAR_VOLUME, 101);
            Assert.AreEqual(100, cue2D.GetVariable(VAR_VOLUME));

            cue2D.SetVariable(VAR_VOLUME, 100);
            Assert.AreEqual(100, cue2D.GetVariable(VAR_VOLUME));

            cue2D.SetVariable(VAR_VOLUME, -0.001f);
            Assert.AreEqual(0, cue2D.GetVariable(VAR_VOLUME));

            cue2D.SetVariable(VAR_VOLUME, -1000);
            Assert.AreEqual(0, cue2D.GetVariable(VAR_VOLUME));
        }

        [Test]
        public void Apply3dDistance()
        {
            Cue cue3D = soundBank.GetCue(CUE_3D);

            AudioListener listener = new AudioListener();
            listener.Position = new Vector3(0, 0, -100);
            AudioEmitter emitter = new AudioEmitter();
            emitter.Position = new Vector3(0, 0, 100);

            cue3D.Apply3D(listener, emitter);
            Assert.AreEqual(200, cue3D.GetVariable(VAR_DISTANCE));
        }

        [Test]
        public void Play2DCue()
        {
            Cue cue2D = soundBank.GetCue(CUE_2D);
            // init
            Game.RunOneFrame();

            string waveFileName = "Play2DCue.wav";
            StartRecording(waveFileName);

            cue2D.Play();
            do
            {
                // TODO: is this a good way to do this?
                Game.RunOneFrame();
                audioEngine.Update();
                Thread.Sleep(100);                
            } while (cue2D.IsPlaying);

            StopRecording();

            AssertAudioFilesSimilar(waveFileName);
        }

        [Test]
        public void Play2DCueTracks()
        {
            Cue cue2D = soundBank.GetCue(CUE_2D_TRACKS);
            // init
            Game.RunOneFrame();

            string waveFileName = "Play2DCueTracks.wav";
            StartRecording(waveFileName);

            cue2D.Play();
            do
            {
                // TODO: is this a good way to do this?
                Game.RunOneFrame();
                audioEngine.Update();
                Thread.Sleep(100);
            } while (cue2D.IsPlaying);

            StopRecording();

            AssertAudioFilesSimilar(waveFileName);
        }

        /// <summary>
        /// Tests whether a track specific RPC works
        /// </summary>
        [Test]
        public void Play2DCueVolumeTrackRpc()
        {
            Cue cue2D = soundBank.GetCue(CUE_2D_TRACKS);
            // init
            Game.RunOneFrame();

            string waveFileName = "Play2DCueVolumeTrackRpc.wav";
            StartRecording(waveFileName);

            cue2D.SetVariable(VAR_VOLUME, 50);
            cue2D.Play();
            do
            {
                // TODO: is this a good way to do this?
                Game.RunOneFrame();
                audioEngine.Update();
                Thread.Sleep(100);
            } while (cue2D.IsPlaying);

            Thread.Sleep(200);

            StopRecording();

            AssertAudioFilesSimilar(waveFileName);
        }

        [Test]
        public void Play2DCueTwice()
        {
            Cue cue2D1 = soundBank.GetCue(CUE_2D);
            Cue cue2D2 = soundBank.GetCue(CUE_2D);
            // init
            Game.RunOneFrame();

            string waveFileName = "Play2DCueTwice.wav";
            StartRecording(waveFileName);

            cue2D1.Play();
            cue2D2.Play();
            do
            {
                // TODO: is this a good way to do this?
                Game.RunOneFrame();
                audioEngine.Update();
                Thread.Sleep(100);
            } while (cue2D1.IsPlaying || cue2D2.IsPlaying);

            StopRecording();

            AssertAudioFilesSimilar(waveFileName);
        }

        [Test]
        public void Play2DCueWithCategoryVolume()
        {
            Cue cue2D = soundBank.GetCue(CUE_2D);
            // init
            Game.RunOneFrame();

            AudioCategory category = audioEngine.GetCategory("Default");
            category.SetVolume(0.66f);

            string waveFileName = "Play2DCueWithCategoryVolume.wav";
            StartRecording(waveFileName);

            cue2D.SetVariable(VAR_VOLUME, 100);
            cue2D.Play();
            do
            {
                // TODO: is this a good way to do this?
                Game.RunOneFrame();
                audioEngine.Update();
                Thread.Sleep(100);
            } while (cue2D.IsPlaying);

            StopRecording();

            AssertAudioFilesSimilar(waveFileName);
        }

        [Test]
        public void Play2DCueWithPitch()
        {
            Cue cue2D = soundBank.GetCue(CUE_2D);
            // init
            Game.RunOneFrame();

            string waveFileName = "Play2DCueWithPitch.wav";
            StartRecording(waveFileName);

            cue2D.SetVariable(VAR_SPEED, 2);
            cue2D.Play();
            do
            {
                // TODO: is this a good way to do this?
                Game.RunOneFrame();
                audioEngine.Update();
                Thread.Sleep(100);
            } while (cue2D.IsPlaying);

            cue2D = soundBank.GetCue(CUE_2D);
            cue2D.SetVariable(VAR_SPEED, -4);
            cue2D.Play();
            do
            {
                // TODO: is this a good way to do this?
                Game.RunOneFrame();
                audioEngine.Update();
                Thread.Sleep(100);
            } while (cue2D.IsPlaying);

            StopRecording();

            AssertAudioFilesSimilar(waveFileName);
        }

        [Test]
        public void Play3DCue()
        {
            Cue cue3D = soundBank.GetCue(CUE_3D);
            // init
            Game.RunOneFrame();

            AudioListener listener = new AudioListener();
            listener.Position = new Vector3(0, 0, 0);
            AudioEmitter emitter = new AudioEmitter();
            emitter.Position = new Vector3(50, 0, 50);
            cue3D.SetVariable(VAR_VOLUME, 100);
            cue3D.Apply3D(listener, emitter);

            string waveFileName = "Play3DCue.wav";
            StartRecording(waveFileName);

            cue3D.Play();
            do
            {
                // TODO: is this a good way to do this?
                Game.RunOneFrame();
                audioEngine.Update();
                Thread.Sleep(100);
            } while (cue3D.IsPlaying);

            StopRecording();

            AssertAudioFilesSimilar(waveFileName);
        }
    }
}
