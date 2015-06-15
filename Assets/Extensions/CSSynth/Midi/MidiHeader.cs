namespace CSSynth.Midi
{
    public class MidiHeader
    {
        //--Variables
        public int deltaTicksPerQuaterNote;
        public MidiHelper.MidiFormat MidiFormat;
        public MidiHelper.MidiTimeFormat TimeFormat;
        //--Public Methods
        public MidiHeader()
        {

        }
        public void setMidiFormat(int format)
        {
            if (format == 0)
                MidiFormat = MidiHelper.MidiFormat.SingleTrack;
            else if (format == 1)
                MidiFormat = MidiHelper.MidiFormat.MultiTrack;
            else if (format == 2)
                MidiFormat = MidiHelper.MidiFormat.MultiSong;
        }
    }
}
