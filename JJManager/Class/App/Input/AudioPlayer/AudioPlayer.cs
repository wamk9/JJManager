using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JJManager.Class.App.Input.AudioPlayer
{
    public class AudioPlayer
    {
        private IWavePlayer _waveOut;
        private AudioFileReader _audioFileReader;
        private string _actualFileName = null;
        private string _tmpFilePath = null;
        private string _audioFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JohnJohn3D", "JJManager", "AudioPlayerSongs");
        private bool _removeAudio = false;

        public static event EventHandler AudioStarted;

        public bool HasAudio
        {
            get => File.Exists(Path.Combine(_audioFolderPath, _actualFileName));
        }

        public AudioPlayer(uint profileId, uint inputId)
        {
            _actualFileName = profileId + "_" + inputId + ".mp3";
            _audioFileReader = File.Exists(Path.Combine(_audioFolderPath, _actualFileName)) ? new AudioFileReader(Path.Combine(_audioFolderPath, _actualFileName)) : null;

            AudioStarted += OnAudioStarted;
        }

        public void PlayAudio()
        {
            try
            {
                _waveOut?.Stop();

                if (File.Exists(Path.Combine(_audioFolderPath, _actualFileName)))
                {
                    AudioStarted?.Invoke(this, EventArgs.Empty);
                    _waveOut = new WaveOutEvent();
                    _audioFileReader = new AudioFileReader(Path.Combine(_audioFolderPath, _actualFileName));
                    _waveOut.Init(_audioFileReader);
                    _waveOut.Play();
                }
            }
            catch (Exception ex)
            {
                Log.Insert("AudioPlayer", $"Ocorreu um problema ao executar o áudio '{_actualFileName}'", ex);
            }
        }

        public static void PlayAudioIndependent(string filePath)
        {
            try
            {
                IWavePlayer waveOut = new WaveOutEvent();
                AudioFileReader audioFileReader;
                
                if (File.Exists(filePath))
                {
                    audioFileReader = new AudioFileReader(filePath);
                    waveOut.Init(audioFileReader);
                    waveOut.Play();
                }
            }
            catch (Exception ex)
            {
                Log.Insert("AudioPlayer", $"Ocorreu um problema ao executar o áudio '{filePath}'", ex);
            }
        }

        public void Update()
        {
            if (_tmpFilePath != null) // Case has a new audio
            {
                File.Copy(_tmpFilePath, Path.Combine(_audioFolderPath, _actualFileName), true);
            }
            else if (_removeAudio) // Case audio need's to be removed
            {
                File.Delete(Path.Combine(_audioFolderPath, _actualFileName));
            }
        }

        public void SetAudio(string completePath)
        {
            if (File.Exists(completePath))
            {
                _tmpFilePath = completePath;
            }
        }

        public void RemoveAudio()
        {
            _removeAudio = true;
        }

        private void OnAudioStarted(object sender, EventArgs e)
        {
            // Stop the currently playing sound
            _waveOut?.Stop();
            _waveOut?.Dispose();
            _audioFileReader?.Dispose();
        }
    }
}
