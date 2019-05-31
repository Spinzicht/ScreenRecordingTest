namespace SoundTouchSharp
{
    public class SoundTouchProfile
    {
        public bool UseTempo { get; set; }
        public bool UseAntiAliasing { get; set; }
        public bool UseQuickSeek { get; set; } = true;

        public SoundTouchProfile(bool useTempo, bool useAntiAliasing)
        {
            UseTempo = useTempo;
            UseAntiAliasing = useAntiAliasing;
        }
    }
}