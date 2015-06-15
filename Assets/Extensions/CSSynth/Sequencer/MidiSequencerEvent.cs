using System.Collections.Generic;
using CSSynth.Midi;

namespace CSSynth.Sequencer
{
    public class MidiSequencerEvent
    {
        //--Variables
        public List<MidiEvent> Events; //List of Events
        //--Public Methods
        public MidiSequencerEvent()
        {
            Events = new List<MidiEvent>();
        }
    }
}
