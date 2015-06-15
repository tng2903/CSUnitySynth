using System;

namespace CSSynth.Wave
{
    public interface IChunk
    {
        WaveHelper.WaveChunkType GetChunkType();
        String GetChunkId();
        int GetChunkSize();
    }
}
