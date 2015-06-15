using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using CSSynth.Effects;
using CSSynth.Sequencer;
using CSSynth.Synthesis;
using CSSynth.Midi;
using System;
using UnityEngine.UI;


public class MidiPlayer : MonoBehaviour {
    public AudioSource audioSource;
    //Public variables
    public string midiFilePath;
    public string midiFileName = "from laputa.midi";
    public string bankFilePath = "GM Bank/gm";
    public int bufferSize = 1024;
    public Text songTitle;
    public Text songTime;
    public Text statusText;
    public Text playButtonLabel;

    //Private variables
    private float[] sampleBuffer;
    private float gain = 1f;
    private MidiSequencer midiSequencer;
    private StreamSynthesizer midiStreamSynthesizer;
    //for 16 channel
    private bool[] channelMute;

    protected DateTime[] noteTimer;
    //protected DateTime time;

	// Use this for initialization
	void Start () {
        noteTimer = new DateTime[128];
        channelMute = new bool[16];
        for (int i = 0; i < channelMute.Length; i++) {
            channelMute[i] = false;
        }
        songTitle.text = midiFileName;
        midiFilePath = Application.dataPath + "/Resources/" + midiFileName;
        byte[] songByteArray = File.ReadAllBytes((midiFilePath));
        MidiFile songFile = new MidiFile(songByteArray);
        
        
        //assume we are playing stereo 
        midiStreamSynthesizer = new StreamSynthesizer(44100, 2, bufferSize, 40);
        sampleBuffer = new float[midiStreamSynthesizer.BufferSize];

        midiStreamSynthesizer.LoadBank(bankFilePath);

        midiSequencer = new MidiSequencer(midiStreamSynthesizer);
        

         var tempoEvent = songFile.getAllMidiEventsofType(MidiHelper.MidiChannelEvent.None, MidiHelper.MidiMetaEvent.Tempo);
         print(string.Format("There are {0} event related to TEMPO taken, tempo is {1}", tempoEvent.Count, tempoEvent[0].Parameters[0]));
        if (tempoEvent.Count > 0) {
            songFile.MicroSecondsPerQuarterNote = (uint)tempoEvent[0].Parameters[0];
        }
        
        

        //These will be fired by the midiSequencer when a song plays. Check the console.
        midiSequencer.NoteOnEvent += new MidiSequencer.NoteOnEventHandler(MidiNoteOnHandler);
        midiSequencer.NoteOffEvent += new MidiSequencer.NoteOffEventHandler(MidiNoteOffHandler);

        //songFile.BeatsPerMinute = 120;
        //midiSequencer.LoadMidi(songFile, false);
        print(string.Format("MicroSecondsPerQuarterNote: {0}, BPM: {1}, DeltaTicksPerQuaterNote (PPQT): {2}", songFile.MicroSecondsPerQuarterNote, songFile.BeatsPerMinute, songFile.MidiHeader.deltaTicksPerQuaterNote));
        var secondsPerPulse = (double)songFile.MicroSecondsPerQuarterNote / 1000000 / (double)songFile.MidiHeader.deltaTicksPerQuaterNote;

        //songFile.CombineTracks();

        print("Seconds per pulse " + secondsPerPulse);
        var notesOn = songFile.getAllMidiEventsofType(0, MidiHelper.MidiChannelEvent.Note_On, MidiHelper.MidiMetaEvent.None);
        var noteOff = songFile.getAllMidiEventsofType(0, MidiHelper.MidiChannelEvent.Note_Off, MidiHelper.MidiMetaEvent.None);

        for (byte i = 0; i < 15; i++) {
            var noteOnEvents = songFile.getAllMidiEventsofType(i, MidiHelper.MidiChannelEvent.Note_On);
            var noteOffEvents = songFile.getAllMidiEventsofType(i, MidiHelper.MidiChannelEvent.Note_Off);

            print(string.Format("There are total {0} ON events and {1} OFF events on channel {2}", noteOnEvents.Count, noteOffEvents.Count, i));
            for (int j = 0; j < noteOnEvents.Count; j++) {
                print(string.Format("--Note ON number {0}, id: {1}, start at {2}, deltaTimeFromStart: {3}", j, noteOnEvents[j].parameter1, noteOnEvents[j].deltaTime, noteOnEvents[j].deltaTimeFromStart));
                print(string.Format("----Delta time from start: {0}, Delta time from time sample: {1}", noteOnEvents[j].deltaTimeFromStart * secondsPerPulse, noteOnEvents[j].sampleTime / (double)midiStreamSynthesizer.SampleRate));
            }
            for (int j = 0; j < noteOffEvents.Count; j++) {
                print(string.Format("--Note OFF number {0}, id: {1}, start at {2}, deltaTimeFromStart: {3}", j, noteOffEvents[j].parameter1, noteOffEvents[j].deltaTime, noteOffEvents[j].deltaTimeFromStart));
                print(string.Format("----Delta time from start: {0}, Delta time from time sample: {1}", noteOffEvents[j].deltaTimeFromStart * secondsPerPulse, noteOffEvents[j].sampleTime / (double)midiStreamSynthesizer.SampleRate));
            }
        }

            print("There are " + songFile.Tracks.Length + " tracks");        
        for (int i = 0; i < songFile.Tracks.Length; i++) {
            //print("--Displaying events for tracks " + i);
            print(string.Format("--Displaying events on track {0}, there are {1} events", i, songFile.Tracks[i].EventCount));
//             var trackEvents = songFile.Tracks[i].MidiEvents;
//             for (int j = 0; j < trackEvents.Length; j++) {
//                 if (trackEvents[j].isChannelEvent()) {
//                     print(string.Format("----Event number {0}, type: {1}, note {2}, start at {3}", j, trackEvents[i].midiChannelEvent, trackEvents[j].parameter1));
//                 }
//             }
            //print(string.Format("Note number {0}, name {4}, started at {1}, end at {2}, duration in ticks {3}, duration in seconds {5}", i, notesOn[i].deltaTime, noteOff[i].deltaTime, (noteOff[i].deltaTime - notesOn[i].deltaTime), notesOn[i].parameter1, (float)(noteOff[i].deltaTime - notesOn[i].deltaTime) * secondsPerPulse));
        }
        statusText.text = "Midi file loaded, file length: " + midiSequencer.EndTime;
	}

    void Update() {
        //songTime.text = midiSequencer.Time.ToString();
    }

    public void MidiNoteOnHandler(int channel, int note, int velocity) {
        Debug.Log(string.Format("NoteON: channel {0}, note {1}, velocity {2}", channel, note, velocity));
        noteTimer[note] = DateTime.Now;
    }

    public void MidiNoteOffHandler(int channel, int note) {
        //noteTimer[note] = Time.realtimeSinceStartup;
        Debug.Log(string.Format("NoteOFF: channel {0}, note {1}, duration: {2}", channel, note, (DateTime.Now - noteTimer[note]).TotalMilliseconds));
    }

    public void Stop() {
        playButtonLabel.text = "Play";
        midiSequencer.Stop(true);
        statusText.text = "Stopped";
        audioSource.Stop();
    }

    public void ToggleChannel(int channel) {
        channelMute[channel] = !channelMute[channel];
        if (channelMute[channel]) {
            midiSequencer.MuteChannel(channel);
        } else {
            midiSequencer.UnMuteChannel(channel);
        }
    }

    public void ResetLevel() {
        Application.LoadLevel(0);
    }

    public void PlayPause() {
        if (midiSequencer != null) {
            if (midiSequencer.isPlaying) {
                playButtonLabel.text = "Play";
                midiSequencer.Pause();
                statusText.text = "Paused";
            } else if(midiSequencer.isPaused){
                playButtonLabel.text = "Pause";
                midiSequencer.Unpause();
                statusText.text = "Resumed";
            } else {
                midiSequencer.Play();
                statusText.text = "Playing";
                audioSource.Play();
            }
        }
    }

    private void OnAudioFilterRead(float[] data, int channels) {

        //This uses the Unity specific float method added to get the buffer
        midiStreamSynthesizer.GetNext(sampleBuffer);

        for (int i = 0; i < data.Length; i++) {
            data[i] = sampleBuffer[i] * gain;
        }
    }
}
